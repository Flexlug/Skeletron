using System;
using System.Collections.Generic;
using System.Text;

namespace OsuNET_Api.Models.Bancho
{
    public class UserBadge
    {
        public DateTime awarded_at { get; set; }
        public string description { get; set; }
        public string image_url { get; set; }
        public string url { get; set; }
    }
}
