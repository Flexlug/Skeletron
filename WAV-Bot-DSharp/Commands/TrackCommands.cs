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
using WAV_Osu_NetApi.Bancho.Models;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Class with demonstration of possibilities.
    /// Disclaimer: The code shouldn't be used exactly this way as it is, it's just there to give you some ideas.
    /// </summary>
    [RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
    public class TrackCommands : SkBaseCommandModule
    {
        private ITrackService tracking;
        private GatariApi gapi;
        private BanchoApi bapi;

        public TrackCommands(ITrackService trackService, BanchoApi bapi, GatariApi gapi)
        {
            ModuleName = "Tracking";

            tracking = trackService;
            this.gapi = gapi;
            this.bapi = bapi;
        }

        [Command("track-gatari-recent"), Description("Start tracking user's recent scores on gatari")]
        public async Task TrackGatariRecent(CommandContext commandContext,
            [Description("Gatari username"), RemainingText] string nickname)
        {
            GUser guser = null;
            if (!gapi.TryGetUser(nickname, ref guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {nickname} on gatari.");
                return;
            }

            await tracking.AddGatariTrackRecentAsync(guser);
            await commandContext.RespondAsync($"User's {(guser is null ? "" : $"[{guser.abbr}]")} {guser.username} recent scores are being tracked.");
        }

        [Command("stop-track-gatari-recent"), Description("Stop tracking user's recent scores on gatari")]
        public async Task StopTrackGatariRecent(CommandContext commandContext,
            [Description("Gatari username"), RemainingText] string nickname)
        {
            GUser guser = null;
            if (!gapi.TryGetUser(nickname, ref guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {nickname} on gatari.");
                return;
            }

            if (!await tracking.RemoveGagariTrackRecentAsync(guser))
            {
                await commandContext.RespondAsync($"Couldn't delete user. Maybe this user is not being tracked.");
                return;
            }

            await commandContext.RespondAsync($"Stop tracking {(guser is null ? "" : $"[{guser.abbr}]")} {guser.username}.");
        }

        [Command("track-bancho-recent"), Description("Start tracking user's recent scores on bancho")]
        public async Task TrackBanchoRecent(CommandContext commandContext,
            [Description("Gatari username"), RemainingText] string nickname)
        {
            await commandContext.RespondAsync($"Disabled");
            return;


            User guser = null;
            if (!bapi.TryGetUser(123, ref guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {nickname} on gatari.");
                return;
            }

            await tracking.RemoveBanchoTrackRecentAsync(guser);
            //await commandContext.RespondAsync($"User's {(guser is null ? "" : $"[{guser.abbr}]")} {guser.username} recent scores are being tracked.");
        }

        [Command("stop-track-bancho-recent"), Description("Stop tracking user's recent scores on bancho")]
        public async Task StopTrackBachoRecent(CommandContext commandContext,
            [Description("Gatari username"), RemainingText] string nickname)
        {
            await commandContext.RespondAsync($"Disabled");
            return;

            User guser = null;
            if (!bapi.TryGetUser(123, ref guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {nickname} on gatari.");
                return;
            }

            if (!await tracking.RemoveBanchoTrackRecentAsync(guser))
            {
                await commandContext.RespondAsync($"Couldn't delete user. Maybe this user is not being tracked.");
                return;
            }

            //await commandContext.RespondAsync($"Stop tracking {(guser is null ? "" : $"[{guser.abbr}]")} {guser.username}.");
        }
    }
}
