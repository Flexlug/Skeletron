using System;
using System.Collections.Generic;
using System.Text;

namespace OsuNET_Api.Models.Bancho
{
    public class BeatmapCompact
    {
        public float difficulty_rating { get; set; }
        public int id { get; set; }
        public string mode { get; set; }
        public int total_length { get; set; }
        public string version { get; set; }
    }
}
