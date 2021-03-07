using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Services.Structures;
using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.Databases.Interfaces
{
    public interface ITrackedUsersDbService
    {
        /// <summary>
        /// Update time for latest score for gatari user
        /// </summary>
        /// <param name="id">Gatari id</param>
        /// <param name="dateTime">New DateTime for latest score</param>
        public Task UpdateGatariRecentTimeAsync(ulong id, DateTime? dateTime);

        /// <summary>
        /// Add gatari user to track list
        /// </summary>
        /// <param name="u">Gatari user</param>
        /// <returns></returns>
        public Task AddGatariTrackRecentAsync(GUser u);

        /// <summary>
        /// Remove gatari user from track list
        /// </summary>
        /// <param name="u">Gatari user</param>
        /// <returns></returns>
        public Task<bool> RemoveGagariTrackRecentAsync(GUser u);

        /// <summary>
        /// Get next gatari user
        /// </summary>
        /// <returns></returns>
        public Task<TrackedUser> NextGatariUserAsync();

        /// <summary>
        /// Update time for latest score for gatari user
        /// </summary>
        /// <param name="id">Bancho id</param>
        /// <param name="dateTime">New DateTime for latest score</param>
        public Task UpdateBanchoRecentTimeAsync(ulong id, DateTime? dateTime);

        /// <summary>
        /// Add bancho user to track list
        /// </summary>
        /// <param name="u">Bancho id</param>
        /// <returns></returns>
        public Task AddBanchoTrackRecentAsync(int u);

        /// <summary>
        /// Remove gatari user from track list
        /// </summary>
        /// <param name="u">Bancho id</param>
        /// <returns></returns>
        public Task<bool> RemoveBanchoTrackRecentAsync(int u);

        /// <summary>
        /// Get next bacho user
        /// </summary>
        /// <returns></returns>
        public Task<TrackedUser> NextBanchoUserAsync();
    }
}
