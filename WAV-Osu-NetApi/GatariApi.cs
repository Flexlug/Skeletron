using System;
using System.Text;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Osu_NetApi
{
    public class GatariApi
    {
        private readonly string UrlBase = @"https://api.gatari.pw/";

        private RestClient client = new RestClient();



        public List<Score> GetUserRecentScores(int user_id, bool include_fails, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/recent")
                .AddParameter("id", user_id)
                .AddParameter("p", 1)
                .AddParameter("l", limit)
                .AddParameter("f", Convert.ToInt32(include_fails));

            IRestResponse resp = client.Execute(req);

            GScoresResponse g_resp = JsonConvert.DeserializeObject<GScoresResponse>(resp.Content);

            return g_resp.data;
        }

        public List<Score> GetUserBestScores(int user_id, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/best")
                .AddParameter("id", user_id)
                .AddParameter("p", 1)
                .AddParameter("l", limit);

            IRestResponse resp = client.Execute(req);

            GScoresResponse g_resp = JsonConvert.DeserializeObject<GScoresResponse>(resp.Content);

            return g_resp.data;
        }

        public Beatmap RetrieveBeatmap(int id)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"beatmaps/get")
                .AddParameter("id", id);

            IRestResponse resp = client.Execute(req);

            GBeatmapResponse g_resp = JsonConvert.DeserializeObject<GBeatmapResponse>(resp.Content);

            return g_resp.data;
        }
    }
}
