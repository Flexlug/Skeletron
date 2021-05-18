using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Gatari.Models
{
    public class GStatistics
    {
        public int a_count { get; set; }
        public double avg_accuracy { get; set; }
        public double avg_accuracy_ap { get; set; }
        public double avg_accuracy_rx { get; set; }
        public double avg_hits_play { get; set; }
        public int country_rank { get; set; }
        public int id { get; set; }
        public int level { get; set; }
        public int level_progress { get; set; }
        public int max_combo { get; set; }
        public int playcount { get; set; }
        public int playtime { get; set; }
        public int pp { get; set; }
        public int pp_4k { get; set; }
        public int pp_7k { get; set; }
        public int pp_ap { get; set; }
        public int pp_rx { get; set; }
        public int rank { get; set; }
        public int? rank_ap { get; set; }
        public int? rank_rx { get; set; }
        public long ranked_score { get; set; }
        public int replays_watched { get; set; }
        public int s_count { get; set; }
        public int sh_count { get; set; }
        public long total_hits { get; set; }
        public long total_score { get; set; }
        public int x_count { get; set; }
        public int xh_count { get; set; }
    }
}
