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

        private DiscordRole beginnerRole;
        private DiscordRole alphaRole;
        private DiscordRole betaRole;
        private DiscordRole gammaRole;
        private DiscordRole deltaRole;
        private DiscordRole epsilonRole;

        private DiscordGuild guild;
        private DiscordClient client;

        private readonly ulong WAV_UID = 708860200341471264;

        public WAVMembersProvider(DiscordClient client)
        {
            this.store = DocumentStoreProvider.Store;

            this.client = client;
            this.guild = client.GetGuildAsync(WAV_UID).Result;

            this.beginnerRole = guild.GetRole(831262333208756255);
            this.alphaRole = guild.GetRole(831262447502360686);
            this.betaRole = guild.GetRole(831262485910781953);
            this.gammaRole = guild.GetRole(831262538317430844);
            this.deltaRole = guild.GetRole(831262333208756255);
            this.epsilonRole = guild.GetRole(831262333208756255);
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
                    session.Store(new WAVMember(uid));
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
