using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface ITrackService
    {
        /// <summary>
        /// Start user tracking
        /// </summary>
        /// <param name="u">Gatari user</param>
        /// <returns></returns>
        public Task AddTrackRecent(GUser u);

        /// <summary>
        /// Stop user tracking
        /// </summary>
        /// <param name="u">Gatari user</param>
        /// <returns></returns>
        public Task RemoveTrackRecent(GUser u);
    }
}
