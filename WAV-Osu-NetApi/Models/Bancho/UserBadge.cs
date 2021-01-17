using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models.Bancho
{
    public class UserBadge
    {
        public DateTime awarded_at { get; set; }
        public string description { get; set; }
        public string image_url { get; set; }
        public string url { get; set; }
    }
}
