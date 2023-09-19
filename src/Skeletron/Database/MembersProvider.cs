using System;
using System.Linq;

using DSharpPlus;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;

using Skeletron.Database.Models;
using Skeletron.Database.Interfaces;

using OsuNET_Api.Models;

using Microsoft.Extensions.Logging;

namespace Skeletron.Database
{
    public class MembersProvider : IMembersProvider
    {
        private IDocumentStore store;

        private ILogger<CompitProvider> logger;

        private int iter = 0;

        public MembersProvider(DiscordClient client,
                                  ILogger<CompitProvider> logger)
        {
            this.store = DocumentStoreProvider.Store;

            this.logger = logger;
            logger.LogInformation("WAVMembersProvider loaded");
        }

        /// <summary>
        /// Получить информацию об участнике WAV
        /// </summary>
        /// <param name="uid">Discord uid</param>
        /// <returns></returns>
        public WAVMembers GetMember(string uid)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMembers member = session.Query<WAVMembers>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                if (member is null)
                {
                    member = new WAVMembers(uid);
                    session.Store(member);
                    session.SaveChanges();
                }

                return member;
            }
        }

        /// <summary>
        /// Добавить или обновить данные о сервере, на котором играет участник
        /// </summary>
        /// <param name="uid">Uid участника</param>
        /// <param name="server">Название сервера</param>
        /// <param name="id">ID пользователя на сервере</param>
        public void AddOsuServerInfo(string uid, OsuProfileInfo profile)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMembers member = session.Query<WAVMembers>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                if (member is null)
                    throw new NullReferenceException("No such object in DB");

                OsuProfileInfo serverInfo = member.OsuServers.FirstOrDefault(x => x.Server == profile.Server);
                if (serverInfo is not null)
                {
                    serverInfo.OsuId = profile.OsuId;
                    serverInfo.OsuNickname = profile.OsuNickname;
                }
                else
                {
                    member.OsuServers.Add(profile);
                }

                session.SaveChanges();
            }
        }

        /// <summary>
        /// Получить информацию об osu! профиле участника WAV
        /// </summary>
        /// <param name="uid">Discord id участника WAV</param>
        /// <param name="server">Название сервера</param>
        /// <returns></returns>
        public OsuProfileInfo GetOsuProfileInfo(string uid, OsuServer server)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMembers member = session.Query<WAVMembers>()
                                          .Include(x => x.OsuServers)
                                          .Include(x => x.CompitionProfile)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                return member.OsuServers?.FirstOrDefault(x => x.Server == server);
            }
        }

        public WAVMembers Next()
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                int count = session.Query<WAVMembers>().Count();

                if (iter >= count)
                    iter = 0;

                var res = session.Query<WAVMembers>()
                                 .Skip(iter)
                                 .Take(1)
                                 .FirstOrDefault();

                iter++;

                return res;
            }
        }
    }
}
