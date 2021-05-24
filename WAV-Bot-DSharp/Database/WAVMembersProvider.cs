using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

using WAV_Bot_DSharp.Utils;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Database.Interfaces;

using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Database
{
    public class WAVMembersProvider : IWAVMembersProvider
    {
        private IDocumentStore store;

        public WAVMembersProvider(DiscordClient client)
        {
            this.store = DocumentStoreProvider.Store;
        }

        /// <summary>
        /// Получить информацию об участнике WAV
        /// </summary>
        /// <param name="uid">Discord uid</param>
        /// <returns></returns>
        public WAVMember GetMember(ulong uid)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

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
        public void AddOsuServerInfo(ulong uid, OsuServer server, int id)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                if (member is null)
                    throw new NullReferenceException("No such object in DB");

                WAVMemberOsuProfileInfo serverInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);
                if (serverInfo is not null)
                {
                    serverInfo.Id = id;
                }
                else
                {
                    member.OsuServers.Add(new WAVMemberOsuProfileInfo(id, server));
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
        public WAVMemberOsuProfileInfo GetOsuProfileInfo(ulong uid, OsuServer server)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                return member.OsuServers?.FirstOrDefault(x => x.Server == server);
            }
        }
    }
}
