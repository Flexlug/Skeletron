using System;
using System.Linq;
using System.Collections.Generic;

using WAV_Bot_DSharp.Services.Models;

using Raven.Client;
using Raven.Client.Documents.Session;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Utils;
using Raven.Client.Documents;

namespace WAV_Raven_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = DocumentStoreProvider.Store;

            //List<WAVMember> members = new List<WAVMember>();
            //Random rnd = new Random();
            //for (int i = 0; i < 1500; i++)
            //{
            //    WAVMember member = new WAVMember((ulong)Math.Abs(rnd.Next()));
            //    WAVMemberOsuProfileInfo profileInfo = new WAVMemberOsuProfileInfo(rnd.Next(), WAV_Osu_NetApi.Models.OsuServer.Bancho);
            //    member.CompitionInfo.AvgPP = rnd.Next(100, 300);
            //    member.CompitionInfo.ProvidedScore = true;

            //    member.OsuServers.Add(profileInfo);

            //    members.Add(member);
            //}

            //using (IDocumentSession session = store.OpenSession())
            //{
            //    foreach (var member in members)
            //        session.Store(member);

            //    session.SaveChanges();
            //}

            //using (IDocumentSession session = store.OpenSession())
            //{
            //    List<WAVMember> members = session.Query<WAVMember>().ToList();

            //    foreach(var member in members)
            //        session.Delete(member); 

            //    session.SaveChanges();
            //}

            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                List<WAVMember> members = session.Query<WAVMember>().Include(x => x.CompitionInfo).Where(x => x.CompitionInfo.ProvidedScore).ToList();
                Console.WriteLine(members.Count);
            }

            Console.WriteLine("Done");
        }
    }
}
