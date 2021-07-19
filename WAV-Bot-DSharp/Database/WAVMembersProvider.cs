using System;
using System.Linq;

using DSharpPlus;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;

using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Database.Interfaces;

using WAV_Osu_NetApi.Models;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Database
{
    public class WAVMembersProvider : IWAVMembersProvider
    {
        private IDocumentStore store;

        private ILogger<WAVCompitProvider> logger;

        public WAVMembersProvider(DiscordClient client,
                                  ILogger<WAVCompitProvider> logger)
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
        public WAVMember GetMember(string uid)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                if (member is null)
                {
                    member = new WAVMember(uid);
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
        public void AddOsuServerInfo(string uid, WAVMemberOsuProfileInfo profile)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                if (member is null)
                    throw new NullReferenceException("No such object in DB");

                WAVMemberOsuProfileInfo serverInfo = member.OsuServers.FirstOrDefault(x => x.Server == profile.Server);
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
        public WAVMemberOsuProfileInfo GetOsuProfileInfo(string uid, OsuServer server)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .Include(x => x.CompitionProfile)
                                          .FirstOrDefault(x => x.DiscordUID == uid);

                return member.OsuServers?.FirstOrDefault(x => x.Server == server);
            }
        }
    }
}
