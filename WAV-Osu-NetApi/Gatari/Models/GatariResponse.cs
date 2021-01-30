using System.Collections.Generic;
namespace WAV_Osu_NetApi.Gatari.Models
{ 

    public class GatariResponse    {
        public int code { get; set; } 
        public int count { get; set; } 
        public List<Score> scores { get; set; } 
    }

}