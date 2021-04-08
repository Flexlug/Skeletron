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
using WAV_Bot_DSharp.Services;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Commands that can only be used by Server Administrators. The Administrator permission is required (or to be the server owner).
    /// </summary>
    [RequirePermissions(Permissions.BanMembers | Permissions.KickMembers | Permissions.ManageRoles), RequireGuild]
    public sealed class AdminCommands : SkBaseCommandModule
    {
        private DiscordEmoji[] _pollEmojiCache;

        private ILogger<AdminCommands> logger;

        private DiscordChannel LogChannel;

        public AdminCommands(ILogger<AdminCommands> logger, DiscordClient client)
        {
            ModuleName = "Admin";

            this.logger = logger;

            LogChannel = client.GetChannelAsync(816396082153521183).Result;

            logger.LogInformation("AdminCommands loaded");
        }

        /// <summary>
        /// A simple emoji based yes/no poll.
        /// </summary>
        /// <param name="commandContext">CommandContext of the message that has executed this command</param>
        /// <param name="duration">Amount of time how long the poll should last.</param>
        /// <param name="question">Polls question</param>
        /// <returns></returns>
        [Command("emojipoll"), Description("Start a simple emoji poll for a simple yes/no question"), Cooldown(2, 30, CooldownBucketType.Guild)]
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

        [Command("sendtochannel"), Description("Send a message to a special channel")]
        public async Task SendToChannelAsync(CommandContext commandContext,
            [Description("Target discord channel")] DiscordChannel targetChannel,
            [Description("Message to send"), RemainingText] string message)
        {
            await targetChannel.SendMessageAsync(message);
        }

        [Command("mute"), Description("Mute specified user")]
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

        [Command("unmute"), Description("Unmute specified user")]
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

        [Command("kick"), Description("Kick specified user")]
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

        [Command("ban"), Description("Ban specified user")]
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
                                                .AddField("**Reason**:", (reason != string.Empty ? reason : "not stated"), true)
                                                .WithFooter()
                                                .Build());
        }

        [Command("rd"), Description("Resend message to specified channel and delete it")]
        public async Task ResendAndDeleteAsync(CommandContext commandContext,
        [Description("Channel, where message has to be resent")] DiscordChannel targetChannel,
        [Description("Resend reason"), RemainingText] string reason)
        {
            if (commandContext.Message.Reference is null)
                await commandContext.RespondAsync("Resending message is not specified");

            if (targetChannel is null)
                await commandContext.RespondAsync("Target channel is not specified");

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
                                    .AddField("**Reason**:", (reason != string.Empty ? reason : "not stated"), true)
                                    .WithFooter()
                                    .Build());
            await msg.Channel.DeleteMessagesAsync(new[] { msg, commandContext.Message }, reason);
        }
    }
}
