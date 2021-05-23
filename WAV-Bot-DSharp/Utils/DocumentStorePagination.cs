using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace WAV_Bot_DSharp.Utils
{
    public static class DocumentStorePagination
    {
        public const int PAGE_SIZE = 50;

        public static int GetPageCount<T>(this IRavenQueryable<T> queryable)
        {
            QueryStatistics stats;
            queryable.Statistics(out stats).Take(0).ToArray(); //Без перечисления статистика работать не будет.

            var result = stats.TotalResults / PAGE_SIZE;

            if (stats.TotalResults % PAGE_SIZE > 0) // Округляем вверх
            {
                result++;
            }

            return result;
        }

        public static IEnumerable<T> GetPage<T>(this IRavenQueryable<T> queryable, int page)
        {
            return queryable
            .Skip(page * PAGE_SIZE)
                       .Take(PAGE_SIZE)
                       .ToArray();
        }
    }
}
