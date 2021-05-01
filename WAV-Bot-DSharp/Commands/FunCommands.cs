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
                await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/776568856167972904/836541954779119616/4a5b505b4026b6fe30376b0b79d3e108fa755e07r1-540-540_hq.gif");
                return;
            }

            if (msg.Contains("вставай припадочный"))
            {
                await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/776568856167972904/838014941884579880/JeRWf8iDd_4.png");
                return;
            }

            if (msg.Contains("привет") && (msg.Contains("виталий") || msg.Contains("припадочный") || msg.Contains("виталя")))
            {
                await e.Message.RespondAsync(":skull:");
                return;
            }
        }

        /// <summary>
        /// Prints out the latency between the bot and discord api servers.
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <returns></returns>
        [Command("hi"), Description("Greet the bot")]
        public async Task PingAsync(CommandContext commandContext)
        {
            await commandContext.Message.RespondAsync("https://cdn.discordapp.com/attachments/776568856167972904/836541954779119616/4a5b505b4026b6fe30376b0b79d3e108fa755e07r1-540-540_hq.gif");
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
