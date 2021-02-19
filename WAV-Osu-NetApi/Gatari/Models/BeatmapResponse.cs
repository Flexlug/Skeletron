using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Gatari.Models
{
    public class BeatmapResponse
    {
        public int code { get; set; }
        public int count { get; set; }
        public List<Beatmap> data { get; set; }
    }
}
