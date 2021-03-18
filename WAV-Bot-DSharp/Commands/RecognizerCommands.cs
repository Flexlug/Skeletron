using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using WAV_Bot_DSharp.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Предоставляет команды для osu!
    /// </summary>
    [Hidden]
    public class RecognizerCommands : SkBaseCommandModule
    {
        private IRecognizerService osu;
        private ILogger<RecognizerCommands> logger;

        public RecognizerCommands(IRecognizerService osu, ILogger<RecognizerCommands> logger)
        {
            this.osu = osu;
            this.logger = logger;

            logger.LogInformation("RecognizerCommands loaded");
        }

        [Command("dummy"), Description("Send a message to a specified channel in a special guild")]
        public async Task DummyCommand(CommandContext commandContext)
        {
            await commandContext.RespondAsync("As dummy as me");
        }
    }
}
