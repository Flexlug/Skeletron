using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skeletron.Converters
{
    internal class VkRegex
    {
        private Regex groupLink { get; set; }

        public VkRegex()
        {
            groupLink = new Regex(@"https:\/\/vk.com\/wall-(\d+)_(\d+)");
        }

        public string TryGetGroupPostId(string msg)
        {
            Match match = groupLink.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            return $"-{match.Groups[0].Value}_{match.Groups[1].Value}";
        }
    }
}
