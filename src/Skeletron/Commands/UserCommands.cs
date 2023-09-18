using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

using Skeletron.Configurations;

namespace Skeletron.Commands
{
    /// <summary>
    /// Commands that can be used by @everyone. 
    /// </summary>
    [RequireGuild]
    public sealed class UserCommands : SkBaseCommandModule
    {
        private ILogger<UserCommands> logger;

        private Settings settings;

        public UserCommands(ILogger<UserCommands> logger,
                            DiscordClient client,
                            Settings settings)
        {
            ModuleName = "Разное";

            this.logger = logger;
            this.settings = settings;
            
            logger.LogInformation("UserCommands loaded");
        }

        [Command("words"), Description("Посчитать количество слов в указанном сообщении")]
        public async Task WordsCount(CommandContext commandContext, DiscordMessage msg)
        {
            if (msg is null || msg.Content is null || msg.Content.Length == 0)
            {
                await commandContext.RespondAsync("Вы указали пустое сообщение");
                return;
            }

            await commandContext.RespondAsync($"Количество слов в сообщении: {msg.Content.Split().Length}");
        }

        [Command("words"), Description("Посчитать количество слов в указанном сообщении")]
        public async Task WordsCount(CommandContext commandContext)
        {
            if (commandContext.Message.ReferencedMessage is null)
            {
                await commandContext.RespondAsync("Вы указали пустое сообщение");
                return;
            }

            await commandContext.RespondAsync($"Количество слов в сообщении: {commandContext.Message.ReferencedMessage.Content.Split().Length}");
        }

        [Command("r"), Description("Переслать сообщение в другой канал."), RequireGuild]
        public async Task PingAsync(CommandContext commandContext,
            [Description("Текстовый канал, куда необходимо перенаправить сообщение.")] DiscordChannel targetChannel)
        {
            if (commandContext.Message.Reference is null)
                await commandContext.RespondAsync("Вы не указали сообщение, которое необходимо переслать.");

            if (targetChannel is null)
                await commandContext.RespondAsync("Вы не указали канал, куда необходимо переслать сообщение.");

            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Reference.Message.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithFooter($"Sent: {msg.Timestamp}")
                .WithDescription(msg.Content);

            if (!(msg.Author is null))
                builder.WithAuthor(name: $"From {msg.Channel.Name} by {msg.Author.Username}",
                                   iconUrl: msg.Author.AvatarUrl);

            await targetChannel.SendMessageAsync(embed: builder.Build());

            if (msg.Embeds?.Count != 0)
                foreach(var embed in msg.Embeds)
                    await targetChannel.SendMessageAsync(embed: embed);
        }

        /// <summary>
        /// Prints out the latency between the bot and discord api servers.
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <returns></returns>
        [Command("ping"), Description("Показывает пинг бота."), Hidden]
        public async Task PingAsync(CommandContext commandContext)
        {
            await commandContext.RespondAsync($"Bot latency to the discord api server: {commandContext.Client.Ping}");
        }

        /// <summary>
        /// Try to recognize timespan
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <param name="timeSpan">Timespan to recognize</param>
        /// <returns></returns>
        [Command("timespan"), Description("Предпринимает попытку конвертировать строку в тип данных TimeSpan."), Hidden]
        public async Task GetTimespan(CommandContext commandContext,
            [Description("Конвертируемая строка")] TimeSpan timeSpan)
        {
            await commandContext.RespondAsync(timeSpan.ToString());
        }

        /// <summary>
        /// Try to recognize datetime. DateTime inputs in AMERICAN STYLE!!!
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <param name="datetime">Datetime to recognize</param>
        /// <returns></returns>
        [Command("datetime"), Description("Предпринимает попытку конвертировать строку в тип данных DateTime."), Hidden]
        public async Task GetDatetime(CommandContext commandContext, 
            [Description("Конвертируемая строка")] DateTime datetime)
        {
            await commandContext.RespondAsync($"{datetime.ToShortDateString()} {datetime.ToLongTimeString()}");
        }

        [Command("lmgt"), Description("Let me do that job for you")]
        public async Task Lmgtfy(CommandContext commandContext,
            [Description("Search querry"), RemainingText] string querry)
        {
            string searchQuerry = @$"https://letmegooglethat.com/?q={HttpUtility.UrlEncode(querry)}";
            await commandContext.RespondAsync(searchQuerry);
        }
    }
}
