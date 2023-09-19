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
        private readonly IMessageResendService resendMessageService;
        private readonly IMessageDeleteService deleteMessageService;
        //private readonly IWordsService words;

        private DiscordChannel LogChannel;

        public AdminCommands(ILogger<AdminCommands> logger, 
                            DiscordClient client,
                            IShedulerService sheduler,
                            IMessageResendService resendMessageService,
                            IMessageDeleteService deleteMessageService)
                            //IWordsService words)
        {
            ModuleName = "Администрирование";

            this.logger = logger;
            this.sheduler = sheduler;
            this.resendMessageService = resendMessageService;
            this.deleteMessageService = deleteMessageService;
            //this.words = words;

            LogChannel = client.GetChannelAsync(816396082153521183).Result;

            logger.LogInformation("AdminCommands loaded");
        }

        /// <summary>
        /// Получить информацию о времени работы бота
        /// </summary>
        /// <param name="context"></param>
        [Command("status"), Description("Получить информацию о времени работы бота.")]
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
            
            DiscordMember user = await msg.Channel.Guild.GetMemberAsync(msg.Author.Id);
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
            DiscordMember user = await msg.Channel.Guild.GetMemberAsync(msg.Author.Id);
            if (!user.IsBot)
            {
                DiscordDmChannel dmChannel = await user.CreateDmChannelAsync();

                try
                {
                    await dmChannel.SendMessageAsync(content: $"Перенаправлено по причине: {reason}", embed: redirectedMsg.Build());
                }
                catch(Exception ex)
                {
                    logger.LogError("Error in rd command", ex);
                }

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
