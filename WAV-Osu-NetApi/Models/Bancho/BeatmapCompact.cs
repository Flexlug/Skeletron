using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models.Bancho
{
    public class BeatmapCompact
    {
        public float difficulty_rating { get; set; }
        public int id { get; set; }
        public string mode { get; set; }
        public int total_length { get; set; }
        public string version { get; set; }

        public Beatmapset beatmapset { get; set; }
        public string checksum { get; set; }
        public Failtimes failtimes { get; set; }
        public int max_combo { get; set; }
    }
}
