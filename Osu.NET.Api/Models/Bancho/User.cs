using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models.Bancho
{
    public class User : UserCompact
    {
        public string cover_url { get; set; }
        public string discord { get; set; }
        public bool has_supported { get; set; }
        public string interests { get; set; }
        public DateTime join_date { get; set; }
        public Kudosu kudosu { get; set; }
        public string location { get; set; }
        public double max_blocks { get; set; }
        public double max_friends { get; set; }
        public string occupation { get; set; }
        public string playmode { get; set; }
        public List<string> playstyle { get; set; }
        public double post_count { get; set; }
        public List<string> profile_order { get; set; }
        public string skype { get; set; }
        public string title { get; set; }
        public string twitter { get; set; }
        public string website { get; set; }
    }
}
