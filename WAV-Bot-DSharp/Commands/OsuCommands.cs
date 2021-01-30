using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using WAV_Bot_DSharp.Services.Interfaces;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Предоставляет команды для osu!
    /// </summary>
    public class OsuCommands : BaseCommandModule
    {
        IOsuService osu;

        public OsuCommands(IOsuService osu)
        {
            this.osu = osu;
        }

        [Command("dummy"), Description("Send a message to a specified channel in a special guild")]
        public async Task DummyCommand(CommandContext commandContext)
        {
            await commandContext.RespondAsync("As dummy as me");
        }
    }
}
