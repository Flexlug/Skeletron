using System.Threading.Tasks;

using OsuNET_Api.Models.Gatari;

namespace Skeletron.Services.Interfaces
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
