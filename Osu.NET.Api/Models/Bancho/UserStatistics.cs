using System;
using System.Collections.Generic;
using System.Text;

namespace OsuNET_Api.Models.Bancho
{
    public class UserStatistics
    {
        public UserLevel level { get; set; }
        public int global_rank { get; set; }
        public double pp { get; set; }
        public long ranked_score { get; set; }
        public double hit_accuracy { get; set; }
        public int play_count { get; set; }
        public int play_time { get; set; }
        public long total_score { get; set; }
        public long total_hits { get; set; }
        public int maximum_combo { get; set; }
        public int replays_watched_by_others { get; set; }
        public bool is_ranked { get; set; }
        public GradeCounts grade_counts { get; set; }
        public int country_rank { get; set; }
        public UserRank rank { get; set; }
    }
}
