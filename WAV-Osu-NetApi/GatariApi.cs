using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

using WAV_Osu_NetApi.Gatari.Models;
using WAV_Osu_NetApi.Gatari.Models.Responses;

namespace WAV_Osu_NetApi
{
    public class GatariApi
    {
        private readonly string UrlBase = @"https://api.gatari.pw/";

        private RestClient client = new RestClient();

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

        public GStatistics GetUserStats(int user, int mode = 0)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/recent")
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

        public GStatistics GetUserStats(string user, int mode = 0)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/recent")
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
