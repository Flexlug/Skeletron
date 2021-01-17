using System.Collections.Generic; 
namespace WAV_Osu_NetApi.Models.Gatari{ 

    public class GatariResponse    {
        public int code { get; set; } 
        public int count { get; set; } 
        public List<Score> scores { get; set; } 
    }

}