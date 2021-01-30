using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using RestSharp;

using WAV_Osu_NetApi.Bancho.Models;
using WAV_Osu_NetApi.Bancho.QuerryParams;
using WAV_Osu_NetApi.Converters;

namespace WAV_Osu_NetApi
{
    /// <summary>
    /// Класс для взаимодействия с Bancho API
    /// </summary>
    public class BanchoApi
    {
        /// <summary>
        /// Client token, generated in bancho profile
        /// </summary>
        private readonly string Secret;

        /// <summary>
        /// Client id, generated in bancho profile
        /// </summary>
        private readonly int ClientId;

        private string _token;
        /// <summary>
        /// Actual oauth2 token
        /// </summary>
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

        /// <summary>
        /// Default base url
        /// </summary>
        private readonly string UrlBase = @"https://osu.ppy.sh/";
        private RestClient client = new RestClient();

        /// <summary>
        /// Initializes bancho api and gets token
        /// </summary>
        /// <param name="client_id">Client id, which has to be generated in bancho profile</param>
        /// <param name="secret">Client secret, which has to be generated in bancho profile</param>
        public BanchoApi(int client_id, string secret)
        {
            this.Secret = secret;
            this.ClientId = client_id;

            ReloadToken();
        }

        /// <summary>
        /// Reload oauth2 token. Token reloads automaticly, when expires
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Search beatmaps
        /// </summary>
        /// <param name="querry">Text querry. Can be beatmap name, artist etc.</param>
        /// <param name="type">Ranked status</param>
        /// <param name="mode">Gamemode</param>
        /// <param name="lang">Map language</param>
        /// <returns></returns>
        public List<Beatmapset> Search(string querry, MapType? type = null, MapMode? mode = null, MapLang? lang = null)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmapsets/search/")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            if (!string.IsNullOrEmpty(querry))
                req.AddParameter("q", querry);

            if (type != null)
                req.AddParameter("s", BanchoConverter.MapTypeToString((MapType)type));

            if (mode != null)
                req.AddParameter("m", mode);

            if (lang != null)
                req.AddParameter("l", lang);

            IRestResponse resp = client.Execute(req);

            SearchResponse beatmaps = null;
            try
            {
                beatmaps = JsonConvert.DeserializeObject<SearchResponse>(resp.Content);
            }
            catch(Exception)
            {
                return null;
            }

            return beatmaps.Beatmapsets;
        }

        /// <summary>
        /// Get recent user's scores
        /// </summary>
        /// <param name="user">User's id</param>
        /// <param name="include_fails">Also show failed scores</param>
        /// <param name="limit">Scores count per one querry</param>
        /// <returns></returns>
        public List<Score> GetUserRecentScores(int user, bool include_fails, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/recent")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("include_fails", include_fails)
                .AddParameter("limit", limit);

            IRestResponse resp = client.Execute(req);

            List<Score> scores = JsonConvert.DeserializeObject<List<Score>>(resp.Content);

            return scores;
        }

        /// <summary>
        /// Get user's best scores
        /// </summary>
        /// <param name="user">User's id</param>
        /// <param name="limit">Scores count per one querry</param>
        /// <returns></returns>
        public List<Score> GetUserBestScores(int user, int limit)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/best")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("limit", limit);

            IRestResponse resp = client.Execute(req);

            List<Score> scores = JsonConvert.DeserializeObject<List<Score>>(resp.Content);

            return scores;
        }

        /// <summary>
        /// Get beatmapset by it's id
        /// </summary>
        /// <param name="beatmapsetId">Beatmapset id</param>
        /// <returns></returns>
        public Beatmapset GetBeatmapset(int beatmapsetId)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmapsets/{beatmapsetId}")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            IRestResponse resp = client.Execute(req);

            Beatmapset bm = JsonConvert.DeserializeObject<Beatmapset>(resp.Content);

            return bm;
        }

        public void GetSmth()
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmaps/372510")
                            .AddHeader(@"Authorization", $@"Bearer {Token}");

            IRestResponse resp = client.Execute(req);

        }

        /// <summary>
        /// Tries to get user by his id
        /// </summary>
        /// <param name="userId">User's id</param>
        /// <param name="user">If successful, user's info will be returned via this ref</param>
        /// <returns></returns>
        public bool TryGetUser(int userId, out User user)
        {
            IRestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{userId}")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            IRestResponse resp = null;
            try
            {
                resp = client.Execute(req);
            }
            catch (Exception)
            {
                user = null;
                return false;
            }

            User userInfo = JsonConvert.DeserializeObject<User>(resp.Content);

            user = userInfo;
            return true;
        }
    }
}

