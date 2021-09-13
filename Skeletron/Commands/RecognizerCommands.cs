using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Skeletron.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace Skeletron.Commands
{
    /// <summary>
    /// Предоставляет команды для osu!
    /// </summary>
    [Hidden, RequireGuild]
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

        [Command("dummy"), Description("Send a message to a specified channel in a special guild"), Hidden]
        public async Task DummyCommand(CommandContext commandContext)
        {
            await commandContext.RespondAsync("As dummy as me");
        }
    }
}
