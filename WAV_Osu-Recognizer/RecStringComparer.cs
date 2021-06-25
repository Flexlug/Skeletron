using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace WAV_Osu_Recognizer
{
    public static class RecStringComparer
    {
        /// <summary>
        /// Compares two strings and returns their difference in %
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Difference in %</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/33665871/fuzzy-matches-of-strings-with-percentage-in-c-sharp
        /// </remarks>
        public static Double Compare(String left, String right)
        {
            return DictionaryPercentage(WordsToCounts(left), WordsToCounts(right));
        }

        public static Dictionary<String, int> WordsToCounts(String value)
        {
            if (String.IsNullOrEmpty(value))
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            return value
              .Split(' ', '\r', '\n', '\t')
              .Select(item => item.Trim(',', '.', '?', '!', ':', ';', '"'))
              .Where(item => !String.IsNullOrEmpty(item))
              .GroupBy(item => item, StringComparer.OrdinalIgnoreCase)
              .ToDictionary(chunk => chunk.Key,
                            chunk => chunk.Count(),
                            StringComparer.OrdinalIgnoreCase);
        }

        public static Double DictionaryPercentage(
          IDictionary<String, int> left,
          IDictionary<String, int> right)
        {

            if (left is null)
                if (right is null)
                    return 1.0;
                else
                    return 0.0;
            else if (right is null)
                return 0.0;

            int all = left.Sum(pair => pair.Value);

            if (all <= 0)
                return 0.0;

            double found = 0.0;

            foreach (var pair in left)
            {
                int count;

                if (!right.TryGetValue(pair.Key, out count))
                    count = 0;

                found += count < pair.Value ? count : pair.Value;
            }

            int leftLen = left.Select(x => x.Value).Sum();
            int rightLen = right.Select(x => x.Value).Sum();

            int maxLen = Math.Max(leftLen, rightLen);
            int minLen = Math.Min(leftLen, rightLen);

            return found / all * (minLen / maxLen);
        }

        //public static double Compare(string s1, string s2)
        //{
        //    // string char 
        //    char[] sc1 = s1.ToArray(),
        //           sc2 = s2.ToArray();

        //    int maxLength = Math.Max(s1.Length, s2.Length);

        //    // string char dictionary (char/count)
        //    Dictionary<char, int> scd1 = new Dictionary<char, int>(),
        //                          scd2 = new Dictionary<char, int>();

        //    foreach (char c in sc1.Distinct())
        //        scd1.Add(c, sc1.Count(x => x == c));

        //    foreach (char c in sc2.Distinct())
        //        scd2.Add(c, sc2.Count(x => x == c));

        //    double equality = 0;
        //    foreach(var kvp in scd1)
        //    {
        //        if (!scd2.ContainsKey(kvp.Key))
        //            continue;

        //        double max = Math.Max(scd1[kvp.Key], scd2[kvp.Key]);
        //        double min = Math.Min(scd1[kvp.Key], scd2[kvp.Key]);

        //        equality += min / max * (max / maxLength);
        //    }
        //    return equality;
        //}
    }
}
