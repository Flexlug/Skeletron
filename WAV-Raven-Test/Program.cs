using System;
using System.Linq;
using System.Collections.Generic;

using WAV_Bot_DSharp.Services.Models;

using Raven.Client;
using Raven.Client.Documents.Session;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Utils;
using Raven.Client.Documents;
using WAV_Bot_DSharp.Services;

namespace WAV_Raven_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = DocumentStoreProvider.Store;

            Random rnd = new Random();

            List<CompitScore> allScores = new List<CompitScore>();

            for (int cat = 0; cat <= 6; cat++) {
                if (cat == 2)
                    continue;
                for (int i = 0; i < rnd.Next(5, 10); i++)
                    allScores.Add(new CompitScore()
                    {
                        Category = (CompitCategories)cat,
                        DiscordUID = rnd.Next().ToString(),
                        Nickname = rnd.Next().ToString(),
                        Score = rnd.Next(1, 100000),
                        ScoreUrl = "sample_string"
                    });
            }

            SheetGenerator generator = new SheetGenerator();
            var file = generator.CompitScoresToFile(allScores);

            Console.WriteLine("Done");
        }
    }
}
