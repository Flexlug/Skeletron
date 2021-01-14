using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using RestSharp;

using WAV_Osu_NetApi.Models;

namespace WAV_Osu_NetApi
{
    /// <summary>
    /// Класс для взаимодействия с Bancho API
    /// </summary>
    public class BanchoApi
    {
        private readonly string Secret;
        private readonly int ClientId;

        private string Token = string.Empty;

        private readonly string UrlBase = @"https://osu.ppy.sh/";

        private RestClient client = new RestClient();

        public BanchoApi(int client_id, string secret)
        {
            this.Secret = secret;
            this.ClientId = client_id;
        }

        public bool Authorize()
        {
            IRestRequest req = new RestRequest(UrlBase + $@"oauth/token")
                .AddParameter("client_id", ClientId)
                .AddParameter("client_secret", Secret)
                .AddParameter("grant_type", "client_credentials")
                .AddParameter("scope", "public");

            IRestResponse resp = client.Execute(req, Method.POST);
            try
            {
                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(resp.Content);
                this.Token = token.Token;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<RecentScore> GetUserRecentScores(string user)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/recent")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("limit", 3);

            IRestResponse resp = client.Execute(req);

            List<RecentScore> scores = JsonConvert.DeserializeObject<List<RecentScore>>(resp.Content);

            return null;
        }
    }
}
