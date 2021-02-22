using System.Collections.Generic;

namespace WAV_Osu_NetApi.Gatari.Models
{
    public class GScoreResponse    
    {
        public int code { get; set; } 
        public int count { get; set; } 
        public List<GScore> scores { get; set; } 
    }

}