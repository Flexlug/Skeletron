using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Services;
using WAV_Bot_DSharp.Services.Entities;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Class with demonstration of possibilities.
    /// Disclaimer: The code shouldn't be used exactly this way as it is, it's just there to give you some ideas.
    /// </summary>
    public class TrackCommands : BaseCommandModule
    {
        private ITrackService tracking;

        public TrackCommands(ITrackService trackService)
        {
            tracking = trackService;
        }

        [Command("dummy"), Description("Send a message to a specified channel in a special guild")]
        public async Task DummyCommand(CommandContext commandContext)
        {
            await commandContext.RespondAsync("As dummy as me");
        }
    }
}
