using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WAV_Bot_DSharp.Commands
{
    [RequireGuild]
    public sealed class FunCommands : SkBaseCommandModule
    {
        private DiscordClient client;
        private ILogger<FunCommands> logger;

        public FunCommands(DiscordClient client, ILogger<FunCommands> logger)
        {
            ModuleName = "Fun commands";

            this.logger = logger;
            this.client = client;
            this.client.MessageCreated += Client_DetectSayHi;

            logger.LogInformation("FunCommands loaded");
        }

        private async Task Client_DetectSayHi(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string msg = e.Message.Content.ToLower();

            if (msg.Contains("привет") && msg.Contains("скелетик"))
            {
                await e.Message.RespondAsync("https://discord.com/channels/708860200341471264/776568856167972904/836541957962072084");
                return;
            }

            //if ((msg.Contains("привет") && msg.Contains("скелетик")) || msg.Contains("привет виталий") || msg.Contains("вставай припадочный") || msg.Contains("привет виталя"))
            if (msg.Contains("привет") && (msg.Contains("скелетик") || msg.Contains("виталий") || msg.Contains("припадочный") || msg.Contains("виталя"))
                || msg.Contains("вставай припадочный"))
                await e.Message.RespondAsync(":skull:");
        }

        /// <summary>
        /// Prints out the latency between the bot and discord api servers.
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <returns></returns>
        [Command("hi"), Description("Greet the bot")]
        public async Task PingAsync(CommandContext commandContext)
        {
            await commandContext.RespondAsync($"{DiscordEmoji.FromName(client, ":skull:")}");
        }

        [Command("google"), Description("Let me do that job for you")]
        public async Task Lmgtfy(CommandContext commandContext,
            [Description("Search querry"), RemainingText] string querry)
        {
            string searchQuerry = @$"https://letmegooglethat.com/?q={HttpUtility.UrlEncode(querry)}";
            await commandContext.RespondAsync(searchQuerry);
        }
    }
}
