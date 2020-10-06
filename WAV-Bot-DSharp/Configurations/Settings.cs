﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Bot_DSharp.Configurations
{
    public class Settings
    {
        public static readonly IList<string> DefaultPrefixes = new List<string>() { "." };

        public string Token { get; set; }
        public IList<string> Prefixes { get; set; }

        public Settings() : this("", DefaultPrefixes) { }

        public Settings(string token, IList<string> prefixes)
        {
            Token = token;
            Prefixes = prefixes;
        }
    }
}
