using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

using OsuNET_Api.Models.Gatari;
using OsuNET_Api.Models.Gatari.Responses;

namespace OsuNET_Api
{
    /// <summary>
    /// Gatari server methods implementation
    /// </summary>
    public class GatariApi
    {
        private readonly string UrlBase = @"https://api.gatari.pw/";

        private RestClient client = new RestClient();

        /// <summary>
        /// Get user recent scores
        /// </summary>
        /// <param name="user_id">User id</param>
        /// <param name="mode">Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</param>
        /// <param name="limit">Scores count per one querry</param>
        /// <param name="include_fails">Include failed scores</param>
        /// <returns>Collection of scores</returns>
        public List<GScore> GetUserRecentScores(int user_id, int mode, int limit, bool include_fails)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/recent")
                .AddParameter("id", user_id)
                .AddParameter("mode", mode)
                .AddParameter("p", 1)
                .AddParameter("l", limit)
                .AddParameter("f", Convert.ToInt32(include_fails));

            IRestResponse resp = client.Execute(req);

            GScoresResponse g_resp = JsonConvert.DeserializeObject<GScoresResponse>(resp.Content);

            return g_resp.scores;
        }

        /// <summary>
        /// Get user best scores
        /// </summary>
        /// <param name="user_id">User id</param>
        /// <param name="limit">Scores count per one querry</param>
        /// <param name="mode">Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</param>
        /// <returns></returns>
        public List<GScore> GetUserBestScores(int user_id, int limit, int mode = 0)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/best")
                .AddParameter("id", user_id)
                .AddParameter("mode", mode)
                .AddParameter("p", 1)
                .AddParameter("l", limit);

            IRestResponse resp = client.Execute(req);

            GScoresResponse g_resp = JsonConvert.DeserializeObject<GScoresResponse>(resp.Content);

            return g_resp.scores;
        }

        /// <summary>
        /// Trying to get bitmap by it's id
        /// </summary>
        /// <param name="id">Beatmap id</param>
        /// <returns></returns>
        public GBeatmap TryGetBeatmap(int id)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"beatmaps/get")
                .AddParameter("bb", id);

            IRestResponse resp = client.Execute(req);

            GBeatmapResponse g_resp = null;
            try
            {
                g_resp = JsonConvert.DeserializeObject<GBeatmapResponse>(resp.Content);

                if (g_resp.code != 200)
                    return null;
            }
            catch(Exception)
            {
                return null;
            }

            return g_resp?.data.FirstOrDefault();
        }
        
        /// <summary>
        /// Trying to get user by nickname
        /// </summary>
        /// <param name="user">User's nickname</param>
        /// <param name="guser">GUser reference, where recieved object will be stored</param>
        /// <returns>If querry ended up successfuly</returns>
        public bool TryGetUser(string user, ref GUser guser)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"users/get")
                .AddParameter("u", user);

            IRestResponse resp = client.Execute(req);

            GUserResponse g_resp = null;
            try
            {
                g_resp = JsonConvert.DeserializeObject<GUserResponse>(resp.Content);

                if (g_resp.code != 200)
                    return false;

                if (g_resp.users is null || g_resp.users.Count == 0)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            guser = g_resp?.users.FirstOrDefault();
            return true;
        }

        /// <summary>
        /// Trying to get user by nickname
        /// </summary>
        /// <param name="user">User's nickname</param>
        /// <param name="guser">GUser reference, where recieved object will be stored</param>
        /// <returns>If querry ended up successfuly</returns>
        public bool TryGetUser(int user, ref GUser guser)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"users/get")
                .AddParameter("u", user);

            IRestResponse resp = client.Execute(req);

            GUserResponse g_resp = null;
            try
            {
                g_resp = JsonConvert.DeserializeObject<GUserResponse>(resp.Content);

                if (g_resp.code != 200)
                    return false;

                if (g_resp.users is null || g_resp.users.Count == 0)
                    return false;
            }
            catch (Exception)
            {
                guser = null;
                return false;
            }

            guser = g_resp?.users.FirstOrDefault();
            return true;
        }

        /// <summary>
        /// Get user statistics by id
        /// </summary>
        /// <param name="user">User's id</param>
        /// <param name="mode">Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</param>
        /// <returns>Profile statistics</returns>
        public GStatistics GetUserStats(int user, int mode = 0)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/stats")
                .AddParameter("u", user)
                .AddParameter("mode", mode);

            IRestResponse resp = client.Execute(req);

            GStatistics stats = null;

            try
            {
                GUserStatsResponse g_resp = JsonConvert.DeserializeObject<GUserStatsResponse>(resp.Content);
                stats = g_resp?.stats;
            }
            catch(Exception)
            {
                return null;
            }

            return stats;
        }

        /// <summary>
        /// Get user statistics by nickname
        /// </summary>
        /// <param name="user">User's id</param>
        /// <param name="mode">Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</param>
        /// <returns>Profile statistics</returns>
        public GStatistics GetUserStats(string user, int mode = 0)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/stats")
                .AddParameter("u", user)
                .AddParameter("mode", mode);

            IRestResponse resp = client.Execute(req);

            GStatistics stats = null;

            try
            {
                GUserStatsResponse g_resp = JsonConvert.DeserializeObject<GUserStatsResponse>(resp.Content);
                stats = g_resp?.stats;
            }
            catch (Exception)
            {
                return null;
            }

            return stats;
        }
    }
}
