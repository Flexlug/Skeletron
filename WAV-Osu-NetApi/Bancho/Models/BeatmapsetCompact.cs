using System;
using System.Collections.Generic;

namespace WAV_Osu_NetApi.Bancho.Models
{
    public class BeatmapsetCompact
    {
        public string artist { get; set; }
        public string artist_unicode { get; set; }
        public List<Beatmap> beatmaps { get; set; }
        public Covers covers { get; set; }
        public string creator { get; set; }
        public double favourite_count { get; set; }
        public double id { get; set; }
        public bool nsfw { get; set; }
        public double play_count { get; set; }
        public string preview_url { get; set; }
        public string status { get; set; }
        public string title { get; set; }
        public string title_unicode { get; set; }
        public double user_id { get; set; }
        public bool video { get; set; }
    }
}