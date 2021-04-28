using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Gatari.Models.Responses
{
    public class GUserStatsResponse
    {
        public int code { get; set; }
        public GStatistics stats { get; set; }
    }
}
