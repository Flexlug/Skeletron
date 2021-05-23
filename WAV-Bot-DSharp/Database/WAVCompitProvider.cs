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

        /// <summary>
        /// Получить информацию об участии данного пользователя в конкурсах WAV
        /// </summary>
        /// <param name="uid">Discord id</param>
        /// <returns></returns>
        public WAVMemberCompitInfo GetParticipationInfo(ulong uid)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                return member.CompitionInfo ?? throw new NullReferenceException($"Couldn't get CompitionInfo from user {uid}");
            }
        }

        /// <summary>
        /// Указать, что участник принял участие в конкурсе WAV
        /// </summary>
        /// <param name="uid">Discord id участника</param>
        public void SetMemberParticipated(ulong uid)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                member.CompitionInfo.ProvidedScore = true;

                session.SaveChanges();
            }
        }


        /// <summary>
        /// Сбросить всю информацию об участии каждого человека в конкурсе
        /// </summary>
        public void ResetAllCompitInfo()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                int pageCount = DocumentStorePagination.GetPageCount(session.Query<WAVMember>());

                for (int page = 0; page < pageCount; page++)
                    foreach (WAVMember member in DocumentStorePagination.GetPage(session.Query<WAVMember>(), page))
                        member.CompitionInfo.ProvidedScore = false;

                session.SaveChanges();
            }
        }

        /// <summary>
        /// Зарегистрировать участника как участника конкурса. 
        /// </summary>
        /// <param name="server">Название сервера, для которого нужно пересчитать скоры</param>
        public double RecountMember(ulong uid, OsuServer server)
        {
            WAVMember member = null;

            using (IDocumentSession session = store.OpenSession())
            {
                member = session.Query<WAVMember>()
                                    .Include(x => x.OsuServers)
                                    .Where(x => x.Uid == uid)
                                    .FirstOrDefault();
            }

            if (member is null)
                throw new NullReferenceException("Couldnt find such member in database.");

            WAVMemberOsuProfileInfo osuProfile = member.OsuServers.FirstOrDefault(x => x.Server == server);

            if (osuProfile is null)
                return -1;

            throw new NotImplementedException();
        }
    }
}
