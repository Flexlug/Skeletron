using System;
using System.Collections.Generic;
using System.Text;

namespace OsuNET_Api.Models.Bancho
{
    public class UserAccountHistory
    {
        public int id { get; set; }
        public string type { get; set; }
        public DateTime timestamp { get; set; }
        public int length { get; set; }
    }
}
