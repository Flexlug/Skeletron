using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Osu_NetApi.Models.Bancho
{
    public class Beatmapset : BeatmapsetCompact
    {
        public Availability availability { get; set; }
        public float bpm { get; set; }
        public bool can_be_hyped { get; set; }
        public bool discussion_enabled { get; set; }
        public bool discussion_locked { get; set; }
        public Hype hype { get; set; }
        public bool is_scoreable { get; set; }
        public DateTime last_updated { get; set; }
        public string legacy_thread_url { get; set; }
        public Nominations nominations { get; set; }
        public int ranked { get; set; }
        public DateTime? ranked_date { get; set; }
        public string source { get; set; }
        public bool storyboard { get; set; }
        public DateTime? submitted_date { get; set; }
        public string tags { get; set; }
    }
}
