using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Gatari.Models
{
    public class GUser
    {
        public string abbr { get; set; }
        public int? clanid { get; set; }
        public string country { get; set; }
        public int custom_hue { get; set; }
        public int favourite_mode { get; set; }
        public int followers_count { get; set; }
        public int id { get; set; }
        public bool is_online { get; set; }
        public long latest_activity { get; set; }
        public int play_style { get; set; }
        public int privileges { get; set; }
        public long registered_on { get; set; }
        public string username { get; set; }
        public string username_aka { get; set; }
    }
}
