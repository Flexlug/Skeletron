using System;
using System.Text;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

using WAV_Osu_NetApi.Gatari.Models;
using System.Linq;

namespace WAV_Osu_NetApi
{
    public class GatariApi
    {
        private readonly string UrlBase = @"https://api.gatari.pw/";

        private RestClient client = new RestClient();

        public List<GScore> GetUserRecentScores(int user_id, bool include_fails, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/recent")
                .AddParameter("id", user_id)
                .AddParameter("p", 1)
                .AddParameter("l", limit)
                .AddParameter("f", Convert.ToInt32(include_fails));

            IRestResponse resp = client.Execute(req);

            GatariResponse<List<GScore>> g_resp = JsonConvert.DeserializeObject<GatariResponse<List<GScore>>>(resp.Content);

            return g_resp.data;
        }

        public List<GScore> GetUserBestScores(int user_id, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"user/scores/best")
                .AddParameter("id", user_id)
                .AddParameter("p", 1)
                .AddParameter("l", limit);

            IRestResponse resp = client.Execute(req);

            GatariResponse<List<GScore>> g_resp = JsonConvert.DeserializeObject<GatariResponse<List<GScore>>>(resp.Content);

            return g_resp.data;
        }

        public GBeatmap TryRetrieveBeatmap(int id)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"beatmaps/get")
                .AddParameter("bb", id);

            IRestResponse resp = client.Execute(req);

            GatariResponse<List<GBeatmap>> g_resp = null;
            try
            {
                g_resp = JsonConvert.DeserializeObject<GatariResponse<List<GBeatmap>>>(resp.Content);
            }
            catch(Exception)
            {
                return null;
            }

            return g_resp?.data.FirstOrDefault();
        }

        public bool TryGetUser(string user, out GUser guser)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"beatmaps/get")
                .AddParameter("u", user);

            IRestResponse resp = client.Execute(req);

            GatariResponse<List<GUser>> g_resp = null;
            try
            {
                g_resp = JsonConvert.DeserializeObject<GatariResponse<List<GUser>>>(resp.Content);
            }
            catch (Exception)
            {
                guser = null;
                return false;
            }

            guser = g_resp?.data.FirstOrDefault();
            return true;
        }
    }
}
