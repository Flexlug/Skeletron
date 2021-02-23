using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using WAV_Bot_DSharp.Services;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Services.Interfaces;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Class with demonstration of possibilities.
    /// Disclaimer: The code shouldn't be used exactly this way as it is, it's just there to give you some ideas.
    /// </summary>
    public class TrackCommands : BaseCommandModule
    {
        private ITrackService tracking;
        private GatariApi gapi;

        public TrackCommands(ITrackService trackService)
        {
            tracking = trackService;

            gapi = new GatariApi();
        }

        [Command("track-gatari-recent"), Description("Start user's recent scores on gatari")]
        public async Task TrackGatariRecent(CommandContext commandContext,
            [Description("Gatari username"), RemainingText] string nickname)
        {
            GUser guser;
            if (!gapi.TryGetUser(nickname, out guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {nickname} on gatari.");
                return;
            }

            await tracking.AddTrackRecent(guser);
            await commandContext.RespondAsync($"User's {(guser is null ? "" : $"[{guser.abbr}]")} {guser.username} recent scores are being tracked.");
        }

        [Command("stop-track"), Description("Start user's recent scores on gatari")]
        public async Task StopTrackGatariTop(CommandContext commandContext,
            [Description("Gatari username"), RemainingText] string nickname)
        {
            GUser guser;
            if (!gapi.TryGetUser(nickname, out guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {nickname} on gatari.");
                return;
            }

            if (!await tracking.RemoveTrackRecent(guser))
            {
                await commandContext.RespondAsync($"Couldn't delete user. Maybe this user is not being tracked.");
                return;
            }

            await commandContext.RespondAsync($"User's {(guser is null ? "" : $"[{guser.abbr}]")} {guser.username} recent scores are being tracked.");
        }
    }
}
