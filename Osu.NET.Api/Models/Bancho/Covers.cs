using Newtonsoft.Json;

namespace OsuNET_Api.Models.Bancho
{
    public class Covers
    {
        public string cover { get; set; }

        [JsonProperty("cover@2x")]
        public string Cover2x { get; set; }

        public string card { get; set; }

        [JsonProperty("card@2x")]
        public string Card2x { get; set; }

        public string list { get; set; }

        [JsonProperty("list@2x")]
        public string List2x { get; set; }

        public string slimcover { get; set; }

        [JsonProperty("slimcover@2x")]
        public string Slimcover2x { get; set; }
    }
}