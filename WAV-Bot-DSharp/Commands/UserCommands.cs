using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Commands that can be used by @everyone. 
    /// </summary>
    public sealed class UserCommands : SkBaseCommandModule
    {
        private ILogger<UserCommands> logger;

        public UserCommands(ILogger<UserCommands> logger)
        {
            ModuleName = "Utils";
            
            this.logger = logger;
            
            logger.LogInformation("UserCommands loaded");
        }

        [Command("r"), Description("Resend message to specified channel")]
        public async Task PingAsync(CommandContext commandContext,
            [Description("Channel, where message has to be resent")] DiscordChannel targetChannel)
        {
            if (commandContext.Message.Reference is null)
                await commandContext.RespondAsync("Resending message is not specified");

            if (targetChannel is null)
                await commandContext.RespondAsync("Target channel is not specified");

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
        [Command("ping"), Description("Shows bot ping to discord api server")]
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
        [Command("timespan"), Description("Try to recognize timespan")]
        public async Task GetTimespan(CommandContext commandContext,
            [Description("Timespan to recognize")] TimeSpan timeSpan)
        {
            await commandContext.RespondAsync(timeSpan.ToString());
        }

        /// <summary>
        /// Try to recognize datetime. DateTime inputs in AMERICAN STYLE!!!
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <param name="datetime">Datetime to recognize</param>
        /// <returns></returns>
        [Command("datetime"), Description("Try to recognize datetime (american style)")]
        public async Task GetDatetime(CommandContext commandContext, 
            [Description("Datetime to recognize")] DateTime datetime)
        {
            await commandContext.RespondAsync($"{datetime.ToShortDateString()} {datetime.ToLongTimeString()}");
        }
    }
}
