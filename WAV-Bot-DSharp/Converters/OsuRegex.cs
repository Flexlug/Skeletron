using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Converters
{
    public class OsuRegex
    {
        private Regex banchoBMandBMSUrl { get; set; }
        private Regex banchoUserId { get; set; }
        private Regex gatariUserId { get; set; }
        private Regex gatariBMSUrl { get; set; }
        private Regex gatariBMUrl { get; set; }

        private ILogger<OsuRegex> logger;

        public OsuRegex(ILogger<OsuRegex> logger)
        {
            this.banchoBMandBMSUrl = new Regex(@"http[s]?:\/\/osu.ppy.sh\/beatmapsets\/([0-9]*)#osu\/([0-9]*)");
            this.gatariBMSUrl = new Regex(@"http[s]?:\/\/osu.gatari.pw\/s\/([0-9]*)");
            this.gatariBMUrl = new Regex(@"http[s]?:\/\/osu.gatari.pw\/b\/([0-9]*)");
            this.banchoUserId = new Regex(@"http[s]?:\/\/osu.ppy.sh\/users\/([0-9]*)");
            this.gatariUserId = new Regex(@"http[s]?:\/\/osu.gatari.pw\/u\/([0-9]*)");

            this.logger = logger;
            logger.LogInformation("OsuRegex loaded");
        }


        /// <summary>
        /// Get beatmapset and beatmap id from bancho url
        /// </summary>
        /// <param name="msg">Message, which contains url</param>
        /// <returns>Tuple, where first element is beatmapset id and second element - beatmap id</returns>
        public Tuple<int, int> GetBMandBMSIdFromBanchoUrl(string msg)
        {
            Match match = banchoBMandBMSUrl.Match(msg);

            if (match is null || match.Groups.Count != 3)
                return null;

            int bms_id, bm_id;

            if (int.TryParse(match.Groups[1].Value, out bms_id) && int.TryParse(match.Groups[2].Value, out bm_id))
                return Tuple.Create(bms_id, bm_id);

            return null;
        }

        /// <summary>
        /// Get beatmapset id from gatari url
        /// </summary>
        /// <param name="msg">Message which contains url</param>
        /// <returns>Id of beatmapset</returns>
        public int? GetBMSIdFromGatariUrl(string msg)
        {
            Match match = gatariBMSUrl.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int bms_id;

            if (int.TryParse(match.Groups[1].Value, out bms_id))
                return bms_id;

            return null;
        }

        /// <summary>
        /// Get beatmapset and beatmap id from gatari url
        /// </summary>
        /// <param name="msg">Message, which contains url</param>
        /// <returns>Beatmap id</returns>
        public int? GetBMIdFromGatariUrl(string msg)
        {
            Match match = gatariBMUrl.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int bm_id;

            if (int.TryParse(match.Groups[1].Value, out bm_id))
                return bm_id;

            return null;
        }

        /// <summary>
        /// Get user id from bancho url
        /// </summary>
        /// <param name="msg">Message, which contains bancho url</param>
        /// <returns>User id</returns>
        public int? GetUserIdFromBanchoUrl(string msg)
        {
            Match match = banchoUserId.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int user_id;

            if (int.TryParse(match.Groups[1].Value, out user_id))
                return user_id;

            return null;
        }

        /// <summary>
        /// Get user id from gatari url
        /// </summary>
        /// <param name="msg">Message, which contains gatari url</param>
        /// <returns>User id</returns>
        public int? GetUserIdFromGatariUrl(string msg)
        {
            Match match = gatariUserId.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int user_id;

            if (int.TryParse(match.Groups[1].Value, out user_id))
                return user_id;

            return null;
        }
    }
}
