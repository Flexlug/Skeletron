using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;

using WAV_Bot_DSharp.Utils;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Database.Interfaces;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Database
{
    public class WAVCompitProvider : IWAVCompitProvider
    {
        private IDocumentStore store;

        private BanchoApi api;
        private GatariApi gapi;

        public WAVCompitProvider(BanchoApi api,
                                 GatariApi gapi)
        {
            this.store = DocumentStoreProvider.Store;

            this.api = api;
            this.gapi = gapi;
        }

        public WAVMemberCompitProfile GetCompitProfile(ulong uid)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                return member.CompitionInfo;
            }
        }

        public List<CompitScore> GetUserScores(ulong id)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                List<CompitScore> scores = session.Query<CompitScore>()
                                                 .Where(x => x.Player == id)
                                                 .ToList();

                return scores;
            }
        }

        public void ResetScores()
        {
            using (IDocumentSession session = store.OpenSession()
            {
                List<CompitScore> scores = session.Query<CompitScore>()
                                                 .ToList();

                foreach (var score in scores)
                    session.Delete(score);

                session.SaveChanges();
            }
        }

        public void SubmitScore(CompitScore score)
        {
            using (IDocumentSession session = store.OpenSession()
            {
                session.Store(score);
                session.SaveChanges();
            }
        }

        public CompitInfo GetCompitionInfo()
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                return session.Query<CompitInfo>().FirstOrDefault();
            }
        }

        public void SetCompitionInfo(CompitInfo info)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                CompitInfo oldInfo = session.Query<CompitInfo>().FirstOrDefault();

                if (oldInfo is null)
                    return;

                session.Delete(oldInfo);
                session.Store(info);

                session.SaveChanges();
            }
        }
    }
}
