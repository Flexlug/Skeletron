using System;
using System.Linq;
using System.Collections.Generic;

using WAV_Bot_DSharp.Services.Models;

using Raven.Client;
using Raven.Client.Documents.Session;

namespace WAV_Raven_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = DocumentStoreProvider.Store;

            WAVMember membesr = new WAVMember()
            {
                Uid = 708860200341471264,
                ActivityPoints = 4,
                LastActivity = DateTime.Now,
                OsuServers = new List<WAVMemberOsuServerInfo>()
                {
                    new WAVMemberOsuServerInfo()
                    {
                        BestLast = DateTime.Now,
                        Id = 4,
                        RecentLast = DateTime.Now,
                        Server = "bancho",
                        TrackBest = true,
                        TrackRecent = true
                    }
                }
            };

            using (IDocumentSession session = store.OpenSession())
            {
                WAVMember member = session.Query<WAVMember>()
                                          .FirstOrDefault(x => x.Uid == 708860200341471264);

                Console.ReadKey();
            }

            Console.WriteLine("Done");
        }
    }
}
