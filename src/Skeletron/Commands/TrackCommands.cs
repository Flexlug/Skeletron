using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Skeletron.Services.Interfaces;

using Microsoft.Extensions.Logging;

using OsuNET_Api;
using OsuNET_Api.Models.Gatari;
using OsuNET_Api.Models.Bancho;

namespace Skeletron.Commands
{
    [RequireUserPermissions(DSharpPlus.Permissions.Administrator), RequireGuild]
    public class TrackCommands : SkBaseCommandModule
    {
        private ITrackService tracking;
        private GatariApi gapi;
        private BanchoApi bapi;
        private ILogger<TrackCommands> logger;

        public TrackCommands(ITrackService trackService, BanchoApi bapi, GatariApi gapi, ILogger<TrackCommands> logger)
        {
            ModuleName = "Трекинг";

            tracking = trackService;
            this.logger = logger;
            this.gapi = gapi;
            this.bapi = bapi;

            logger.LogInformation("TrackCommands loaded");
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
            [Description("Bancho id")] int id)
        {
            User guser = null;
            if (!bapi.TryGetUser(id, ref guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {id} on bancho.");
                return;
            }

            await tracking.AddBanchoTrackRecentAsync(id);
            await commandContext.RespondAsync($"User's {guser.username} recent scores are being tracked.");
        }

        [Command("stop-track-bancho-recent"), Description("Stop tracking user's recent scores on bancho")]
        public async Task StopTrackBachoRecent(CommandContext commandContext,
            [Description("Bancho id")] int id)
        {

            User guser = null;
            if (!bapi.TryGetUser(id, ref guser))
            {
                await commandContext.RespondAsync($"Couldn't find user {id} on bancho.");
                return;
            }

            if (!await tracking.RemoveBanchoTrackRecentAsync(id))
            {
                await commandContext.RespondAsync($"Couldn't delete user. Maybe this user is not being tracked.");
                return;
            }

            await commandContext.RespondAsync($"Stop tracking {guser.username}.");
        }
    }
}
