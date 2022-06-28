using System;
using System.Collections.Generic;

using RestSharp;

using OsuNET_Api.Converters;
using OsuNET_Api.Models.Bancho;

using Newtonsoft.Json;

namespace OsuNET_Api
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

        public List<Beatmapset> Search(string v, object any)
        {
            throw new NotImplementedException();
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
            RestRequest req = new RestRequest(UrlBase + $@"oauth/token")
                .AddParameter("client_id", ClientId)
                .AddParameter("client_secret", Secret)
                .AddParameter("grant_type", "client_credentials")
                .AddParameter("scope", "public");

            RestResponse resp = client.Execute(req, Method.Post);

            if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                return false;

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
            RestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmapsets/search/")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            if (!string.IsNullOrEmpty(querry))
                req.AddParameter("q", querry);

            if (type != null)
                req.AddParameter("s", BanchoConverter.MapTypeToString((MapType)type));

            if (mode != null)
                req.AddParameter("m", (int)mode);

            if (lang != null)
                req.AddParameter("l", (int)lang);

            RestResponse resp = client.Execute(req);

            SearchResponse beatmaps = null;
            try
            {
                beatmaps = JsonConvert.DeserializeObject<SearchResponse>(resp.Content);
            }
            catch(Exception e)
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
        /// <param name="mode">Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</param>
        public List<Score> GetUserRecentScores(int user, bool include_fails, int mode, int limit)
        {
            string playmode = "osu";

            switch(mode)
            {
                case 0:
                    playmode = "osu";
                    break;
                case 1:
                    playmode = "taiko";
                    break;
                case 2:
                    playmode = "fruits";
                    break;
                case 3:
                    playmode = "mania";
                    break;
            }

            RestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/recent")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("include_fails", include_fails ? 1 : 0)
                .AddParameter("mode", playmode)
                .AddParameter("limit", limit);

            RestResponse resp = client.Execute(req);

            if (string.IsNullOrEmpty(resp.Content))
                return null;

            List<Score> scores = JsonConvert.DeserializeObject<List<Score>>(resp.Content);

            return scores;
        }

        /// <summary>
        /// Get user's best scores
        /// </summary>
        /// <param name="user">User's id</param>
        /// <param name="limit">Scores count per one querry</param>
        /// <param name="mode">Mode: 0: osu, 1: taiko, 2: ctb, 3: mania</param>
        /// <returns></returns>
        public List<Score> GetUserBestScores(int user, int limit, int mode = 0)
        {
            string playmode = "osu";

            switch (mode)
            {
                case 0:
                    playmode = "osu";
                    break;
                case 1:
                    playmode = "taiko";
                    break;
                case 2:
                    playmode = "fruits";
                    break;
                case 3:
                    playmode = "mania";
                    break;
            }

            RestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{user}/scores/best")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("limit", limit)
                .AddParameter("mode", playmode);

            RestResponse resp = client.Execute(req);

            if (string.IsNullOrEmpty(resp.Content))
                return null;

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
            RestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmapsets/{beatmapsetId}")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            RestResponse resp = client.Execute(req);

            Beatmapset bm = JsonConvert.DeserializeObject<Beatmapset>(resp.Content);

            return bm;
        }

        /// <summary>
        /// Get beatmap by it's id
        /// </summary>
        /// <param name="beatmapId">Beatmap id</param>
        /// <returns></returns>
        public Beatmap GetBeatmap(int beatmapId)
        {
            RestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmaps/{beatmapId}")
                .AddHeader(@"Authorization", $@"Bearer {Token}");
            
            RestResponse resp = client.Execute(req);

            Beatmap bm = JsonConvert.DeserializeObject<Beatmap>(resp.Content);

            return bm;
        }

        /// <summary>
        /// Get beatmap by it's MD5 hash
        /// </summary>
        /// <param name="beatmapId">Beatmap hash</param>
        /// <returns></returns>
        public Beatmap GetBeatmap(string hash)
        {
            RestRequest req = new RestRequest(UrlBase + $@"api/v2/beatmaps/lookup")
                .AddHeader(@"Authorization", $@"Bearer {Token}")
                .AddParameter("checksum", hash);

            RestResponse resp = client.Execute(req);

            Beatmap bm = JsonConvert.DeserializeObject<Beatmap>(resp.Content);

            return bm;
        }

        /// <summary>
        /// Tries to get user by his id
        /// </summary>
        /// <param name="userNickname">User's nickname</param>
        /// <param name="user">If successful, user's info will be returned via this ref</param>
        /// <returns></returns>
        public bool TryGetUser(string userNickname, ref User user)
        {
            RestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{userNickname}")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            RestResponse resp = null;
            User userInfo;
            try
            {
                resp = client.Execute(req);
                userInfo = JsonConvert.DeserializeObject<User>(resp.Content);

                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    user = null;
                    return false;
                }
            }
            catch (Exception)
            {
                user = null;
                return false;
            }


            user = userInfo;
            return true;
        }

        /// <summary>
        /// Tries to get user by his id
        /// </summary>
        /// <param name="userId">User's id</param>
        /// <param name="user">If successful, user's info will be returned via this ref</param>
        /// <returns></returns>
        public bool TryGetUser(int userId, ref User user)
        {
            RestRequest req = new RestRequest(UrlBase + $@"api/v2/users/{userId}")
                .AddHeader(@"Authorization", $@"Bearer {Token}");

            RestResponse resp = null;
            User userInfo;
            try
            {
                resp = client.Execute(req);
                userInfo = JsonConvert.DeserializeObject<User>(resp.Content);

                if (resp.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    user = null;
                    return false;
                }
            }
            catch (Exception)
            {
                user = null;
                return false;
            }


            user = userInfo;
            return true;
        }
    }
}