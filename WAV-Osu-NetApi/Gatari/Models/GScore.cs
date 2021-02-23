using System;
using WAV_Osu_NetApi.Bancho.Models.Enums;

namespace WAV_Osu_NetApi.Gatari.Models
{ 
    public class GScore
    {
        public double accuracy { get; set; } 
        public GBeatmap beatmap { get; set; } 
        public int completed { get; set; } 
        public int count_100 { get; set; } 
        public int count_300 { get; set; } 
        public int count_50 { get; set; } 
        public int count_gekis { get; set; } 
        public int count_katu { get; set; } 
        public int count_miss { get; set; } 
        public bool full_combo { get; set; } 
        public int id { get; set; } 
        public bool isfav { get; set; } 
        public int max_combo { get; set; } 
        public Mods mods { get; set; } 
        public int play_mode { get; set; } 
        public double pp { get; set; } 
        public string ranking { get; set; } 
        public int score { get; set; } 
        public DateTime time { get; set; } 
        public int views { get; set; } 
    }

}