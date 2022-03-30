using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

using Microsoft.Extensions.Logging;

using Skeletron.Services.Interfaces;

namespace Skeletron.Commands
{
    /// <summary>
    /// Commands that can only be used by Server Administrators. The Administrator permission is required (or to be the server owner).
    /// </summary>
    [RequireGuild]
    public sealed class AdminCommands : SkBaseCommandModule
    {
        private DiscordEmoji[] _pollEmojiCache;

        private ILogger<AdminCommands> logger;

        private readonly IShedulerService sheduler;
        private readonly IUtilityService service;
        //private readonly IWordsService words;

        private DiscordChannel LogChannel;
        private DiscordGuild wavGuild;

        public AdminCommands(ILogger<AdminCommands> logger, 
                            DiscordClient client,
                            DiscordGuild wavGuild,
                            IShedulerService sheduler,
                            IUtilityService service)
                            //IWordsService words)
        {
            ModuleName = "Администрирование";

            this.logger = logger;
            this.sheduler = sheduler;
            this.service = service;
            //this.words = words;

            this.wavGuild = wavGuild;
            LogChannel = client.GetChannelAsync(816396082153521183).Result;

            logger.LogInformation("AdminCommands loaded");
        }

        //[Command("words-clear"), RequireRoles(RoleCheckMode.Any, "Admin"), Description("Полностью сбросить игру \'words\'"), RequireGuild]
        //public async Task ResetWords(CommandContext ctx)
        //{
        //    words.ClearWords();
        //    await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));

        //    logger.LogInformation($"Words cleared by {ctx.Member.Nickname}");
        //}

        //[Command("words-delete"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Удалить слово из коллекции"), RequireGuild]
        //public async Task DeleteWord(CommandContext ctx, 
        //    [Description("Удаляемое слово")] string word)
        //{
        //    string checkingWord = word.ToLower();

        //    if (words.CheckWord(checkingWord)) {
        //        words.DeleteWord(checkingWord);
        //        await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
        //        logger.LogInformation($"Word {word} deleted by {ctx.Member.Nickname}");
        //    }
        //    else
        //    {
        //        await ctx.RespondAsync("Такого слова и так нет");
        //    }
        //}

        //[Command("words-get"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Получить список всех использованных слов"), RequireGuild]
        //public async Task DeleteWord(CommandContext ctx)
        //{
        //    var wordsList = words.GetWords();

        //    if (wordsList is null || wordsList.Count == 0)
        //    {
        //        await ctx.RespondAsync("Слов нет");
        //        return;
        //    }

        //    string allWords = string.Join(", ", words.GetWords());
        //    await ctx.RespondAsync(new DiscordEmbedBuilder()
        //        .WithTitle("Использованные слова:")
        //        .WithDescription(allWords)
        //        .Build());
        //}

        [Command("uptime"), Description("Получить информацию о времени работы бота.")]
        public async Task Uptime(CommandContext context)
        {
            TimeSpan uptime = DateTime.Now - Program.StartTime;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .AddField("Билд", Program.BuildString)
                .AddField("Дата запуска", $"{Program.StartTime.ToLongTimeString()}, {Program.StartTime.ToLongDateString()}")
                .AddField("Время работы", $"{uptime.Days}d, {uptime.Hours}h, {uptime.Minutes}m, {uptime.Seconds}s");

            if (Program.LastFailure is not null)
                embed.AddField("Дата последнего падения", $"{Program.LastFailure?.ToLongDateString()}, {Program.LastFailure?.ToLongTimeString()}")
                     .AddField("Количество падений", Program.Failures.ToString());

            await context.RespondAsync(embed);
        }

        /// <summary>
        /// A simple emoji based yes/no poll.
        /// </summary>
        /// <param name="commandContext">CommandContext of the message that has executed this command</param>
        /// <param name="duration">Amount of time how long the poll should last.</param>
        /// <param name="question">Polls question</param>
        /// <returns></returns>
        [Command("emojipoll"), RequireUserPermissions(Permissions.Administrator), Description("Создать опрос типа \"да\\нет\" с заданной длительностью."), Cooldown(2, 30, CooldownBucketType.Guild)]                                  
        public async Task EmojiPollAsync(CommandContext commandContext, 
            [Description("Длительность опроса. (к примеру 1m = 1 минута).")] TimeSpan duration, 
            [Description("Формулировка обсуждаемого вопроса."), RemainingText] string question)
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

        [Command("sendtochannel"), Aliases("stc"), RequireUserPermissions(Permissions.Administrator), Description("Отправить в заданный канал простое текстовое сообщение.")]
        public async Task SendToChannelAsync(CommandContext commandContext,
            [Description("Текстовый канал, куда будет отправлено сообщение.")] DiscordChannel targetChannel,
            [Description("Отправляемое сообщение."), RemainingText] string message)
        {
            await targetChannel.SendMessageAsync(message);
        }

        [Command("react"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder"), Description("Добавить к указанному сообщению реакцию")]
        public async Task AddEmojiAsync(CommandContext ctx,
            [Description("Сообщение, к которому необходимо добавить эмодзи")] DiscordMessage message,
            [Description("Добавляемый эмодзи")] DiscordEmoji emoji)
        {
            await message.CreateReactionAsync(emoji);
            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
        }

        [Command("mute"), RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.BanMembers), Description("Замьютить указанного пользователя.")]
        public async Task MuteUser(CommandContext commandContext,
            [Description("Пользователь, которого необходимо отправить в мьют.")] DiscordMember discordMember,
            [Description("Причина."), RemainingText] string reason)
        {
            DiscordRole muteRole = commandContext.Guild.Roles.FirstOrDefault(x => x.Value.Name == "Muted").Value;
            await discordMember.GrantRoleAsync(muteRole, reason);

            await commandContext.RespondAsync("", embed: new DiscordEmbedBuilder().WithAuthor(discordMember.DisplayName, iconUrl: discordMember.AvatarUrl)
                                                                           .WithTitle("**MUTED**")
                                                                           .WithDescription($"Reason: {(!string.IsNullOrEmpty(reason) ? reason : "not stated")}")
                                                                           .Build());


            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                                .AddField("**Action**:", "muted", true)
                                                .AddField("**Target**:", discordMember.ToString(), true)
                                                .AddField("**Reason**:", (reason != string.Empty ? reason : "not stated"), true)
                                                .WithFooter()
                                                .Build());
        }

        [Command("unmute"), RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.BanMembers), Description("Размьютить указанного пользователя.")]
        public async Task UnmuteUser(CommandContext commandContext,
            [Description("Пользователь, с короторого необходимо снять мьют.")] DiscordMember discordMember)
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

        [Command("kick"), RequireUserPermissions(Permissions.Administrator | Permissions.KickMembers | Permissions.BanMembers), Description("Кикнуть заданного пользователя.")]
        public async Task KickUser(CommandContext commandContext,
            [Description("Пользователь, которого необходимо кикнуть.")] DiscordMember discordMember,
            [Description("Причина."), RemainingText] string reason = "")
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

        [Command("ban"), RequireUserPermissions(Permissions.Administrator | Permissions.BanMembers), Description("Забанить указанного ползователя.")]
        public async Task BanUser(CommandContext commandContext,
            [Description("Пользователь, которого необходимо забанить.")] DiscordMember discordMember,
            [Description("Причина."), RemainingText] string reason = "")
        {
            await discordMember.RemoveAsync(reason);
            await commandContext.RespondAsync("Забанен по причине конченный долбоёб.", embed: new DiscordEmbedBuilder()
                                                                           .WithAuthor(discordMember.DisplayName, iconUrl: discordMember.AvatarUrl)
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

        [Command("d"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Удалить сообщение и уведомить автора об этом.")]
        public async Task DeleteMessageByLinkAndNotify(CommandContext commandContext,
            [Description("Удаляемое сообщение.")] DiscordMessage msg,
            [Description("Причина."), RemainingText] string reason = "not stated")
        {
            if (msg is null)
            {
                await commandContext.RespondAsync("Deleting message is not specified");
                return;
            }

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
                await targetChannel.SendMessageAsync(content: $"Удалено по причине: {reason}", embed: builder.Build());

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
                        .AddField("**Violator**:", msg.Author.Mention, true)
                        .AddField("**From**:", msg.Channel.Name, true)
                        .AddField("**Reason**:", reason, true)
                        .WithFooter()
                        .Build());

            // await LogChannel.SendMessageAsync(content: $"Deleted message: \n{new string('=', 20)}\n{msg.Content}");

            if (msg.Embeds?.Count != 0)
                foreach (var embed in msg.Embeds)
                    await LogChannel.SendMessageAsync(embed: embed);

            if (msg.Attachments?.Count != 0)
                foreach (var att in msg.Attachments)
                    await LogChannel.SendMessageAsync(att.Url);

            await msg.Channel.DeleteMessageAsync(msg, reason);
        }

        [Command("d"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Удалить сообщение и уведомить автора об этом.")]
        public async Task DeleteMessageAndNotify(CommandContext commandContext,
            [Description("Причина."), RemainingText] string reason = "not stated")
        {
            if (commandContext.Message.Reference is null)
            {
                await commandContext.RespondAsync("Resending message is not specified");
                return;
            }

            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Reference.Message.Id);

            await DeleteMessageByLinkAndNotify(commandContext, msg, reason);

            await commandContext.Message.Channel.DeleteMessageAsync(commandContext.Message);
        }

        [Command("rd"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), Description("Переслать сообщение в другой канал и удалить его с предыдущего.")]
        public async Task ResendAndDeleteAsync(CommandContext commandContext,
            [Description("Канал, куда необходимо переслать сообщение.")] DiscordChannel targetChannel,
            [Description("Причина."), RemainingText] string reason = "not stated")
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

            // redirect message
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Reference.Message.Id);

            DiscordEmbedBuilder redirectedMsg = new DiscordEmbedBuilder()
                .WithFooter($"Mod: {commandContext.Message.Author.Username} {msg.Timestamp}", iconUrl: commandContext.Message.Author.AvatarUrl)
                .WithDescription(msg.Content);

            if (!(msg.Author is null))
                redirectedMsg.WithAuthor(name: $"From {msg.Channel.Name} by {msg.Author.Username}",
                                   iconUrl: msg.Author.AvatarUrl);

            await targetChannel.SendMessageAsync(content: $"{msg.Author.Mention}\nПеренаправлено по причине: {reason}", embed: redirectedMsg.Build());

            if (msg.Embeds?.Count != 0)
                foreach (var embed in msg.Embeds)
                    await targetChannel.SendMessageAsync(embed: embed);

            if (msg.Attachments?.Count != 0)
            {
                WebClient webClient = new WebClient();
                foreach (var att in msg.Attachments)
                {
                    string fileName = $"{DateTime.Now.Ticks}-{att.FileName}";
                    webClient.DownloadFile(new Uri(att.Url), $"downloads/{fileName}");

                    using (FileStream fs = new FileStream($"downloads/{fileName}", FileMode.Open))
                        await targetChannel.SendMessageAsync(new DiscordMessageBuilder().WithFile(fs));                
                }
            }

            // log
            await LogChannel.SendMessageAsync(
                embed: new DiscordEmbedBuilder().WithAuthor(name: commandContext.Message.Author.Username, iconUrl: commandContext.Message.Author.AvatarUrl)
                                    .AddField("**Action**:", "resend message", true)
                                    .AddField("**Violator**:", msg.Author.Mention, true)
                                    .AddField("**From**:", msg.Channel.Name, true)
                                    .AddField("**To**:", targetChannel.Name, true)
                                    .AddField("**Reason**:", reason, true)
                                    .WithFooter()
                                    .Build());
            await msg.Channel.DeleteMessagesAsync(new[] { msg, commandContext.Message }, reason);

            // notify in DM
            DiscordMember user = await wavGuild.GetMemberAsync(msg.Author.Id);
            if (!user.IsBot)
            {
                DiscordDmChannel dmChannel = await user.CreateDmChannelAsync();
                await dmChannel.SendMessageAsync(content: $"Перенаправлено по причине: {reason}", embed: redirectedMsg.Build());

                if (msg.Embeds?.Count != 0)
                    foreach (var embed in msg.Embeds)
                        await dmChannel.SendMessageAsync(embed: embed);

                if (msg.Attachments?.Count != 0)
                    foreach (var att in msg.Attachments)
                        await dmChannel.SendMessageAsync(att.Url);
            }
        }

        [Command("get-sheduled-tasks-list"), RequirePermissions(Permissions.Administrator)]
        public async Task GetListOFSheduledTasks(CommandContext context)
        {
            var sheduledTasks = sheduler.GetAllTasks();

            if (sheduledTasks is null || sheduledTasks.Count == 0)
            {
                await context.RespondAsync("No sheduled tasks");
                return;
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Запланированные задачи");

            foreach (var task in sheduledTasks)
                embedBuilder.AddField(task.Name, $"Interval: {task.Interval}, Repeat: {task.Repeat}");

            await context.RespondAsync(embed: embedBuilder.Build());
        }
    }
}
