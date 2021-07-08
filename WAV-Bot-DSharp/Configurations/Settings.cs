using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Bot_DSharp.Configurations
{
    public class Settings
    {
        public static readonly IList<string> DefaultPrefixes = new List<string>();

        // Discord credential
        public string Token { get; set; }
        public IList<string> Prefixes { get; set; }

        // Bancho credentials
        public int ClientId { get; set; }
        public string Secret { get; set; }

        // Other credentials
        public string GoogleClientID { get; set; }
        public string GoogleClientSecret { get; set; }
        public string GoogleKey { get; set; }
        public string SearchKey { get; set; }

        public Settings() : this("", DefaultPrefixes) { }

        public Settings(string token, IList<string> prefixes)
        {
            Token = token;
            Prefixes = prefixes;
        }
    }
}
