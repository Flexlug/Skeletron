using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using RestSharp;

using WAV_Osu_NetApi.Models.Bancho;

namespace WAV_Osu_NetApi
{
    /// <summary>
    /// Класс для взаимодействия с Bancho API
    /// </summary>
    public class BanchoApi
    {
        private readonly string Secret;
        private readonly int ClientId;

        private string _token;
        private string Token
        {
            get
            {
                if (DateTime.Now >= TokenExpireDate)
                    ReloadToken();

                return _token;
            }
            set
            {
                _token = value;
            }
        }
        private DateTime TokenExpireDate;

        private readonly string UrlBase = @"https://osu.ppy.sh/";

        private RestClient client = new RestClient();

        public BanchoApi(int client_id, string secret)
        {
            this.Secret = secret;
            this.ClientId = client_id;

            ReloadToken();
        }

        public bool ReloadToken()
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
                this.TokenExpireDate = DateTime.Now + TimeSpan.FromSeconds(token.Expires);
                this.Token = token.Token;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<Score> GetUserRecentScores(string user, bool include_fails, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/recent")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("include_fails", include_fails)
                .AddParameter("limit", limit);

            IRestResponse resp = client.Execute(req);

            List<Score> scores = JsonConvert.DeserializeObject<List<Score>>(resp.Content);

            return scores;
        }

        public List<Score> GetUserBestScores(string user, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/best")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("limit", limit);

            IRestResponse resp = client.Execute(req);

            List<Score> scores = JsonConvert.DeserializeObject<List<Score>>(resp.Content);

            return scores;
        }
    }
}
