using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

//using NumbersAPI.NET;
//using NumbersAPI.NET.Exceptions;

namespace Skeletron.Commands
{
    [RequireGuild]
    public sealed class FunCommands : SkBaseCommandModule
    {
        private Random _random = new(DateTime.Now.Millisecond);
        private DiscordClient client;
        private ILogger<FunCommands> logger;

        private Regex _flexlugHelpRegex = new Regex(@"фле+кс по+мо+ги+ +(.+)", RegexOptions.Compiled);

        private readonly string[] _sayHiVariants =
        {
            "https://cdn.discordapp.com/attachments/776568856167972904/838014941884579880/JeRWf8iDd_4.png",
            "https://tenor.com/view/int-crawling-int-crawling-skeleton-okbr-gif-21774748"
        };
        
        public FunCommands(DiscordClient client, ILogger<FunCommands> logger
            //NumbersApi api
            )
        {
            ModuleName = "Развлечения";

            this.logger = logger;
            this.client = client;
            this.client.MessageCreated += Client_DetectSayHi;
            this.client.MessageCreated += Client_FlexlugHelp;

            //this.numbersApi = api;

            logger.LogInformation("FunCommands loaded");
        }
        
        private async Task Client_FlexlugHelp(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string msg = e.Message.Content.ToLower();

            var matches = _flexlugHelpRegex.Match(msg);
            
            if (matches is null || matches.Groups.Count != 2)
                return;

            var question = matches.Groups[1].Value;
            string searchQuerry = @$"https://letmegooglethat.com/?q={HttpUtility.UrlEncode(question)}";
            
            await e.Message.RespondAsync($"Опять все делать вместо вас???\n{searchQuerry}");
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
                var respond = _sayHiVariants[
                    _random.Next(0, _sayHiVariants.Length)];
                await e.Message.RespondAsync(respond);
                return;
            }

            if (msg.Contains("вставай") && msg.Contains("ержан"))
            {
                await e.Message.RespondAsync(
                    "https://cdn.discordapp.com/attachments/839633777491574785/1076823929185898567/skeletron_sleeps.jpg");
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
    }
}
