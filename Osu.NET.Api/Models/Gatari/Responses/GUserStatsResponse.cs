using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models.Gatari.Responses
{
    public class GUserStatsResponse
    {
        public int code { get; set; }
        public GStatistics stats { get; set; }
    }
}
