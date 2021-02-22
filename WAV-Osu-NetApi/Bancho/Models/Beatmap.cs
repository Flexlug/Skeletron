using System;
using WAV_Osu_NetApi.Bancho.Models.Enums;

namespace WAV_Osu_NetApi.Bancho.Models
{
    public class Beatmap : BeatmapCompact
    { 
        public float accuracy { get; set; }
        public float ar { get; set; }
        public int beatmapset_id { get; set; }
        public float? bpm { get; set; }
        public bool convert { get; set; }
        public int count_circles { get; set; }
        public int count_sliders { get; set; }
        public int count_spinners { get; set; }
        public float cs { get; set; }
        public DateTime? deleted_at { get; set; }
        public float drain { get; set; }
        public int hit_length { get; set; }
        public bool is_scoreable { get; set; }
        public DateTime last_updated { get; set; }
        public int mode_int { get; set; }
        public int passcount { get; set; }
        public int playcount { get; set; }
        public RankStatus ranked { get; set; }
        public string status { get; set; }
        public string url { get; set; }
    }
}