using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Osu_NetApi.Models.Gatari;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface ITrackService
    {
        /// <summary>
        /// Start user tracking
        /// </summary>
        /// <param name="u">Gatari user</param>
        /// <returns></returns>
        public Task AddGatariTrackRecentAsync(GUser u);

        /// <summary>
        /// Stop user tracking
        /// </summary>
        /// <param name="u">Gatari user</param>
        /// <returns></returns>
        public Task<bool> RemoveGagariTrackRecentAsync(GUser u);

        /// <summary>
        /// Start user tracking
        /// </summary>
        /// <param name="u">Bancho user</param>
        /// <returns></returns>
        public Task AddBanchoTrackRecentAsync(int u);

        /// <summary>
        /// Stop user tracking
        /// </summary>
        /// <param name="u">Bancho user</param>
        /// <returns></returns>
        public Task<bool> RemoveBanchoTrackRecentAsync(int u);
    }
}
