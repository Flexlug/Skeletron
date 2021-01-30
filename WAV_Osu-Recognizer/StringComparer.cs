using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace WAV_Osu_Recognizer
{
    public static class StringComparer
    {
        /// <summary>
        /// Compares two strings and returns their difference in %
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Difference in %</returns>
        public static double Compare(string s1, string s2)
        {
            // string char 
            char[] sc1 = s1.ToArray(),
                   sc2 = s2.ToArray();

            int maxLength = Math.Max(s1.Length, s2.Length);

            // string char dictionary (char/count)
            Dictionary<char, int> scd1 = new Dictionary<char, int>(),
                                  scd2 = new Dictionary<char, int>();

            foreach (char c in sc1.Distinct())
                scd1.Add(c, sc1.Count(x => x == c));

            foreach (char c in sc2.Distinct())
                scd2.Add(c, sc2.Count(x => x == c));

            double equality = 0;
            foreach(var kvp in scd1)
            {
                if (!scd2.ContainsKey(kvp.Key))
                    continue;

                double max = Math.Max(scd1[kvp.Key], scd2[kvp.Key]);
                double min = Math.Min(scd1[kvp.Key], scd2[kvp.Key]);

                equality += min / max * (max / maxLength);
            }

            return equality;
        }
    }
}
