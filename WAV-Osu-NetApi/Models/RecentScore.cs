using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models
{
    public class RecentScore
    {
        [JsonProperty("beatmap_id")]
        public string BeatmapId { get; set; }
        
        [JsonProperty("score")]
        public string ScoreNum { get; set; }

        [JsonProperty("maxcombo")]
        public string MaxCombo { get; set; }

        [JsonProperty("count50")]
        public string Count50 { get; set; }

        [JsonProperty("count100")]
        public string Count100 { get; set; }

        [JsonProperty("count300")]
        public string Count300 { get; set; }

        [JsonProperty("countrmiss")]
        public string CountMiss { get; set; }

        [JsonProperty("countkatu")]
        public string CountKatu { get; set; }

        [JsonProperty("countgeki")]
        public string CountGeki { get; set; }

        [JsonProperty("perfect")]
        public string Perfect { get; set; }

        [JsonProperty("enabled_mods")]
        public string EnabledMods { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("rank")]
        public string Rank { get; set; }
    }
}
