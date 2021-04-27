using System;
using System.Collections.Generic;

namespace WAV_Osu_NetApi.Bancho.Models
{
    public class UserCompact
    {
        public string avatar_url { get; set; }
        public string country_code { get; set; }
        public string default_group { get; set; }
        public int id { get; set; }
        public bool is_active { get; set; }
        public bool is_bot { get; set; }
        public bool is_online { get; set; }
        public bool is_supporter { get; set; }
        public DateTime? last_visit { get; set; }
        public bool pm_friends_only { get; set; }
        public string profile_colour { get; set; }
        public string username { get; set; }

        public int favourite_beatmapset_count { get; set; }
        public int follower_count { get; set; }
        public int graveyard_beatmapset_count { get; set; }
        public int loved_beatmapset_count { get; set; }
        public List<UserMonthlyPlaycount> monthly_playcounts { get; set; }
        public Page page { get; set; }
        public List<string> previous_usernames { get; set; }
        public int ranked_and_approved_beatmapset_count { get; set; }
        public List<ReplaysWatchedCount> replays_watched_counts { get; set; }
        public int scores_best_count { get; set; }
        public int scores_first_count { get; set; }
        public int scores_recent_count { get; set; }
        public UserStatistics statistics { get; set; }
        public int support_level { get; set; }
        public int unranked_beatmapset_count { get; set; }
        public List<UserAchievement> user_achievements { get; set; }
        public UserRankHistory rank_history { get; set; }

        public Country country { get; set; }
        public Cover cover { get; set; }
        public bool is_admin { get; set; }
        public bool is_bng { get; set; }
        public bool is_full_bn { get; set; }
        public bool is_gmt { get; set; }
        public bool is_limited_bn { get; set; }
        public bool is_moderator { get; set; }
        public bool is_nat { get; set; }
        public bool is_restricted { get; set; }
        public bool is_silenced { get; set; }
    }
}