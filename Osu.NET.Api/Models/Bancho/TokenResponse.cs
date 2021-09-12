using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models.Bancho
{
    public class TokenResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int Expires { get; set; }

        [JsonProperty("access_token")]
        public string Token { get; set; }
    }
}
