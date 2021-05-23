using WAV_Osu_NetApi.Models.Gatari.Enums;

namespace WAV_Osu_NetApi.Models.Gatari
{ 
    public class GBeatmap
    {
        public float ar { get; set; } 
        public int beatmap_id { get; set; } 
        public string beatmap_md5 { get; set; } 
        public int beatmapset_id { get; set; } 
        public float bpm { get; set; } 
        public string creator { get; set; } 
        public double difficulty { get; set; } 
        public int fc { get; set; } 
        public int hit_length { get; set; } 
        public float od { get; set; } 
        public GRankStatus ranked { get; set; } 
        public double ranking_data { get; set; }
        public int ranked_status_frozen { get; set; } 
        public string song_name { get; set; } 
    }

}