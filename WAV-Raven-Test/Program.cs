using System;
using System.Linq;
using System.Collections.Generic;

using WAV_Bot_DSharp.Services.Models;

using Raven.Client;
using Raven.Client.Documents.Session;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Utils;

namespace WAV_Raven_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = DocumentStoreProvider.Store;

            //List<WAVMember> members = new List<WAVMember>();
            //Random rnd = new Random();
            //for (int i = 0; i < 500; i++)
            //{
            //    WAVMember member = new WAVMember((ulong)Math.Abs(rnd.Next()));
            //    WAVMemberOsuProfileInfo profileInfo = new WAVMemberOsuProfileInfo(rnd.Next(), "bancho");
            //    member.CompitionInfo.AvgPP = rnd.Next(100, 300);
            //    member.CompitionInfo.ProvidedScore = true;

            //    members.Add(member);
            //}

            //using (IDocumentSession session = store.OpenSession())
            //{
            //    foreach(var member in members)
            //        session.Store(member);

            //    session.SaveChanges();
            //}

            using (IDocumentSession session = store.OpenSession())
            {
                int pageCount = DocumentStorePagination.GetPageCount(session.Query<WAVMember>());

                for (int page = 0; page < pageCount; page++)
                    foreach (WAVMember member in DocumentStorePagination.GetPage(session.Query<WAVMember>(), page))
                        session.Delete(member);

                session.SaveChanges();
            }

            Console.WriteLine("Done");
        }
    }
}
