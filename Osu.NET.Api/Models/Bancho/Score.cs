using System;
using System.Collections.Generic;

namespace WAV_Osu_NetApi.Models.Bancho
{
    public class Score
    {
        public long id { get; set; }
        public long? best_id { get; set; }
        public int user_id { get; set; }
        public double accuracy { get; set; }
        public List<string> mods { get; set; }
        public int score { get; set; }
        public int? max_combo { get; set; }
        public bool perfect { get; set; }
        public ScoreStatistics statistics { get; set; }
        public double? pp { get; set; }
        public string rank { get; set; }
        public DateTime created_at { get; set; }
        public string mode { get; set; }
        public int mode_int { get; set; }
        public bool replay { get; set; }
        public Beatmap beatmap { get; set; }
        public Beatmapset beatmapset { get; set; }
        public User user { get; set; }
    }
}