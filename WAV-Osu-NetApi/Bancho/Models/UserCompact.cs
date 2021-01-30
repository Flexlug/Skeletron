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