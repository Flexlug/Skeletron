using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Services;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Commands that can only be used by Server Administrators. The Administrator permission is required (or to be the server owner).
    /// </summary>
    [RequireGuild]
    public sealed class AdminCommands : SkBaseCommandModule
    {
        private DiscordEmoji[] _pollEmojiCache;

        private ILogger<AdminCommands> logger;

        private DiscordChannel LogChannel;

        private DiscordGuild wavGuild;

        public AdminCommands(ILogger<AdminCommands> logger, DiscordClient client)
        {
            ModuleName = "Admin";

            this.logger = logger;

            LogChannel = client.GetChannelAsync(816396082153521183).Result;
            wavGuild = client.GetGuildAsync(708860200341471264).Result;

            logger.LogInformation("AdminCommands loaded");
        }

        /// <summary>
        /// A simple emoji based yes/no poll.
        /// </summary>
        /// <param name="commandContext">CommandContext of the message that has executed this command</param>
        /// <param name="duration">Amount of time how long the poll should last.</param>
        /// <param name="question">Polls question</param>
        /// <returns></returns>
        [Command("emojipoll"), RequireUserPermissions(Permissions.Administrator), Description("Start a simple emoji poll for a simple yes/no question"), Cooldown(2, 30, CooldownBucketType.Guild)]
        public async Task EmojiPollAsync(CommandContext commandContext, 
            [Description("How long should the poll last. (e.g. 1m = 1 minute)")] TimeSpan duration, 
            [Description("Poll question"), RemainingText] string question)
        {
            if (!string.IsNullOrEmpty(question))
            {
                var client = commandContext.Client;
                var interactivity = client.GetInteractivity();
                if (_pollEmojiCache == null)
                {
                    _pollEmojiCache = new[] {
                        DiscordEmoji.FromName(client, ":white_check_mark:"),
                        DiscordEmoji.FromName(client, ":x:")
                    };
                }

                // Creating the poll message
                var pollStartText = new StringBuilder();
                pollStartText.Append("**").Append("Poll started for:").AppendLine("**");
                pollStartText.Append(question);
                var pollStartMessage = await commandContext.RespondAsync(pollStartText.ToString());

                // DoPollAsync adds automatically emojis out from an emoji array to a special message and waits for the "duration" of time to calculate results.
                var pollResult = await interactivity.DoPollAsync(pollStartMessage, _pollEmojiCache, PollBehaviour.DeleteEmojis, duration);
                var yesVotes = pollResult[0].Total;
                var noVotes = pollResult[1].Total;

                // Printing out the result
                var pollResultText = new StringBuilder();
                pollResultText.AppendLine(question);
                pollResultText.Append("Poll result: ");
                pollResultText.Append("**");
                if (yesVotes > noVotes)
                {
                    pollResultText.Append("Yes");
                }
                else if (yesVotes == noVotes)
                {
                    pollResultText.Append("Undecided");
                }
                else
                {
                    pollResultText.Append("No");
                }
                pollResultText.Append("**");
                await commandContext.RespondAsync(pollResultText.ToString());
            }
            else
            {
                await commandContext.RespondAsync("Error: the question can't be empty");
            }
        }

        [Command("sendtochannel"), RequireUserPermissions(Permissions.Administrator), Description("Send a message to a special channel")]
        public async Task SendToChannelAsync(CommandContext commandContext,
            [Description("Target discord channel")] DiscordChannel targetChannel,
            [Description("Message to send"), RemainingText] string message)
        {
            await targetChannel.SendMessageAsync(message);
        }

        [Command("dm-ignore"), RequireUserPermissions(Permissions.Administrator), Description("Add specified user to DM ignore list")]
        public async Task AddDMIgnore(CommandContext commandContext,
            [Description("Target member")] DiscordMember targetMember)
        {
            if (Settings.KOSTYL.IgnoreDMList.Contains(targetMember.Id))
            {
                await commandContext.RespondAsync("This person is already there");
                return;
            }
            else
            {
                Settings.KOSTYL.IgnoreDMList.Add(targetMember.Id);
            }
            SettingsLoader loader = new SettingsLoader();
            loader.SaveToFile(Settings.KOSTYL);

            await commandContext.RespondAsync($"Added {targetMember} to blacklist.");
        }

        [Command("dm-ignore-list"), RequireUserPermissions(Permissions.Administrator), Description("Return dm ignore list")]
        public async Task DMIgnoreList(CommandContext commandContext)
        {
            await commandContext.RespondAsync($"Blacklisted persons: \n{string.Join('\n', Settings.KOSTYL.IgnoreDMList.Select(x => x.ToString()))}\nend;");
        }

        [Command("dm-pardon"), RequirePermissions(Permissions.Administrator), Description("Remove specified user from DM ignore list")]
        public async Task AddDMPardon(CommandContext commandContext,
            [Description("Target member")] DiscordMember targetMember)
        {
            if (Settings.KOSTYL.IgnoreDMList.Contains(targetMember.Id))
            {
                Settings.KOSTYL.IgnoreDMList.Remove(targetMember.Id);
            }
            else
            {
                await commandContext.RespondAsync("No such person in ignore list");
                return;
            }

            SettingsLoader loader = new SettingsLoader();
            loader.SaveToFile(Settings.KOSTYL);

            await commandContext.RespondAsync($"Removed {targetMember} from blacklist.");
        }

        [Command("mute"), RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.BanMembers), Description("Mute specified user")]
        public async Task MuteUser(CommandContext commandContext,
            [Description("User which should be muted")] DiscordMember discordMember,
            [Description("Reason"), RemainingText] string reason)
        {
            DiscordRole muteRole = commandContext.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Muted").Value;
            await discordMember.GrantRoleAsync(muteRole, reason);

            await commandContext.RespondAsync("", embed: new DiscordEmbedBuilder().WithAuthor(discordMember.DisplayName, iconUrl: discordMember.AvatarUrl)
                                                                           .WithTitle("**MUTED**")
                                                                           .WithDescription($"Reason: {(reason != string.Empty ? reason : "not stated")}")
                                                                           .Build());


            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                                .AddField("**Action**:", "muted", true)
                                                .AddField("**Target**:", discordMember.ToString(), true)
                                                .AddField("**Reason**:", (reason != string.Empty ? reason : "not stated"), true)
                                                .WithFooter()
                                                .Build());
        }

        [Command("unmute"), RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.BanMembers), Description("Unmute specified user")]
        public async Task UnmuteUser(CommandContext commandContext,
            [Description("User to unmute")] DiscordMember discordMember)
        {
            DiscordRole muteRole = commandContext.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Muted").Value;
            if (discordMember.Roles.Contains(muteRole))
            {
                await discordMember.RevokeRoleAsync(muteRole);
                await commandContext.RespondAsync($"User **{discordMember.DisplayName}** is **unmuted**");
            }
            else
            {
                await commandContext.RespondAsync($"User **{discordMember.DisplayName}** is not muted");
            }

            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                                .AddField("**Action**:", "unmuted", true)
                                                .AddField("**Target**:", discordMember.ToString(), true)
                                                .WithFooter()
                                                .Build());
        }

        [Command("kick"), RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.BanMembers), Description("Kick specified user")]
        public async Task KickUser(CommandContext commandContext,
            [Description("User to kick")] DiscordMember discordMember,
            [Description("Reason"), RemainingText] string reason = "")
        {
            await discordMember.RemoveAsync(reason);
            await commandContext.RespondAsync("Отправляйся в вальхаллу.", embed: new DiscordEmbedBuilder().WithAuthor(discordMember.DisplayName, iconUrl: discordMember.AvatarUrl)
                                                                           .WithTitle("**KICKED**")
                                                                           .WithDescription($"Reason: {(reason != string.Empty ? reason : "not stated")}")
                                                                           .Build());

            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                                .AddField("**Action**:", "kick", true)
                                                .AddField("**Target**:", discordMember.ToString(), true)
                                                .AddField("**Reason**:", (reason != string.Empty ? reason : "not stated"), true)
                                                .WithFooter()
                                                .Build());
        }

        [Command("ban"), RequireUserPermissions(Permissions.Administrator | Permissions.BanMembers), Description("Ban specified user")]
        public async Task BanUser(CommandContext commandContext,
            [Description("User to ban")] DiscordMember discordMember,
            [Description("Reason"), RemainingText] string reason = "")
        {
            await discordMember.RemoveAsync(reason);
            await commandContext.RespondAsync("Забанен по причине долбоёб.", embed: new DiscordEmbedBuilder().WithAuthor(discordMember.DisplayName, iconUrl: discordMember.AvatarUrl)
                                                                           .WithTitle("**BANNED**")
                                                                           .WithDescription($"Reason: {(reason != string.Empty ? reason : "not stated")}")
                                                                           .Build());

            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                                .AddField("**Action**:", "ban", true)
                                                .AddField("**Target**:", discordMember.ToString(), true)
                                                .AddField("**Reason**:", (string.IsNullOrEmpty(reason) ? "not stated" : reason), true)
                                                .WithFooter()
                                                .Build());
        }

        [Command("d"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Delete message and notify about this in DM")]
        public async Task DeleteMessageByLinkAndNotify(CommandContext commandContext,
        [Description("Deleteing message")] DiscordMessage msg,
        [Description("Deleting reason"), RemainingText] string reason)
        {
            if (msg is null)
            {
                await commandContext.RespondAsync("Deleting message is not specified");
                return;
            }

            if (string.IsNullOrEmpty(reason))
                reason = "not stated";

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithFooter($"Mod: {commandContext.Message.Author.Username} {msg.Timestamp}", iconUrl: commandContext.Message.Author.AvatarUrl)
                .WithDescription(msg.Content);
            

            if (!(msg.Author is null))
                builder.WithAuthor(name: $"From {msg.Channel.Name} by {msg.Author.Username}",
                                   iconUrl: msg.Author.AvatarUrl);

            DiscordMember user = await wavGuild.GetMemberAsync(msg.Author.Id);
            if (!user.IsBot)
            {
                DiscordDmChannel targetChannel = await user.CreateDmChannelAsync();
                await targetChannel.SendMessageAsync(content: $"Удалено по причине по причине: {reason}", embed: builder.Build());

                if (msg.Embeds?.Count != 0)
                    foreach (var embed in msg.Embeds)
                        await targetChannel.SendMessageAsync(embed: embed);

                if (msg.Attachments?.Count != 0)
                    foreach (var att in msg.Attachments)
                        await targetChannel.SendMessageAsync(att.Url);
            }

            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                        .AddField("**Action**:", "delete message", true)
                        .AddField("**From**:", msg.Channel.Name, true)
                        .AddField("**Reason**:", reason, true)
                        .WithFooter()
                        .Build());

            await LogChannel.SendMessageAsync(content: msg.Content);

            if (msg.Embeds?.Count != 0)
                foreach (var embed in msg.Embeds)
                    await LogChannel.SendMessageAsync(embed: embed);

            if (msg.Attachments?.Count != 0)
                foreach (var att in msg.Attachments)
                    await LogChannel.SendMessageAsync(att.Url);

            await msg.Channel.DeleteMessageAsync(msg, reason);
        }

        [Command("d"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Delete message and notify about this in DM")]
        public async Task DeleteMessageAndNotify(CommandContext commandContext,
        [Description("Deleting reason"), RemainingText] string reason)
        {
            if (commandContext.Message.Reference is null)
            {
                await commandContext.RespondAsync("Resending message is not specified");
                return;
            }

            if (string.IsNullOrEmpty(reason))
                reason = "not stated";

            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Reference.Message.Id);

            await DeleteMessageByLinkAndNotify(commandContext, msg, reason);

            await commandContext.Message.Channel.DeleteMessageAsync(commandContext.Message);
        }

        [Command("rd"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Resend message to specified channel and delete it")]
        public async Task ResendAndDeleteAsync(CommandContext commandContext,
        [Description("Channel, where message has to be resent")] DiscordChannel targetChannel,
        [Description("Resend reason"), RemainingText] string reason)
        {
            if (commandContext.Message.Reference is null)
            {
                await commandContext.RespondAsync("Resending message is not specified");
                return;
            }

            if (targetChannel is null)
            {
                await commandContext.RespondAsync("Target channel is not specified");
                return;
            }

            if (string.IsNullOrEmpty(reason))
                reason = "not stated";

            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Reference.Message.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithFooter($"Mod: {commandContext.Message.Author.Username} {msg.Timestamp}", iconUrl: commandContext.Message.Author.AvatarUrl)
                .WithDescription(msg.Content);

            if (!(msg.Author is null))
                builder.WithAuthor(name: $"From {msg.Channel.Name} by {msg.Author.Username}",
                                   iconUrl: msg.Author.AvatarUrl);

            await targetChannel.SendMessageAsync(content: $"{msg.Author.Mention}\nПеренаправлено по причине: {reason}", embed: builder.Build());

            if (msg.Embeds?.Count != 0)
                foreach (var embed in msg.Embeds)
                    await targetChannel.SendMessageAsync(embed: embed);

            if (msg.Attachments?.Count != 0)
                foreach (var att in msg.Attachments)
                    await targetChannel.SendMessageAsync(att.Url);

            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                    .AddField("**Action**:", "resend message", true)
                                    .AddField("**From**:", msg.Channel.Name, true)
                                    .AddField("**To**:", targetChannel.Name, true)
                                    .AddField("**Reason**:", reason, true)
                                    .WithFooter()
                                    .Build());
            await msg.Channel.DeleteMessagesAsync(new[] { msg, commandContext.Message }, reason);
        }
    }
}
