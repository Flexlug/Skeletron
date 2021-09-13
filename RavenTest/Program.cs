using System;
using System.Linq;
using System.Collections.Generic;

using Skeletron.Services.Models;

using Raven.Client;
using Raven.Client.Documents.Session;
using Skeletron.Database.Models;
using Skeletron.Utils;
using Raven.Client.Documents;
using Skeletron.Services;

namespace WAV_Raven_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = DocumentStoreProvider.Store;

            Random rnd = new Random();

            List<WAVMembers> allMembers = null;

            using (var session = store.OpenSession())
            {
                var wavMembers = session.Query<WAVMember>().ToList();

                allMembers = wavMembers.Select(x => new ServerMember(x.DiscordUID)
                {
                    ActivityPoints = x.ActivityPoints,
                    CompitionProfile = x.CompitionProfile,
                    DiscordUID = x.DiscordUID,
                    LastActivity = x.LastActivity,
                    OsuServers = x.OsuServers
                }).ToList();

                session.Store(allMembers);
                session.SaveChanges();
            }

            Console.WriteLine("Done");
        }
    }
}
