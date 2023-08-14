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
        private Regex groupExportLink { get; set; }
        private Regex groupNormalLink { get; set; }

        public VkRegex()
        {
            groupExportLink = new Regex(@"(?<!\\)https:\/\/vk.com\/wall(-?\d+)_(\d+)");
            groupNormalLink = new Regex(@"(?<!\\)https:\/\/vk.com\/.*w=wall(-?\d+)_(\d+)");
        }

        public string TryGetGroupPostIdFromExportUrl(string msg)
        {
            Match match = groupExportLink.Match(msg);

            if (match is null || match.Groups.Count != 3)
                return null;

            return $"{match.Groups[1].Value}_{match.Groups[2].Value}";
        }

        public string TryGetGroupPostIdFromRegularUrl(string msg)
        {
            Match match = groupNormalLink.Match(msg);

            if (match is null || match.Groups.Count != 3)
                return null;

            return $"{match.Groups[1].Value}_{match.Groups[2].Value}";
        }
    }
}
