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

using WAV_Bot_DSharp.Services.Models;
using WAV_Bot_DSharp.Utils;

namespace WAV_Bot_DSharp.Services
{
    public class WAVMembersProvider
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

                return member;
            }
        }

        /// <summary>
        /// Получить информацию об участии данного пользователя в конкурсах WAV
        /// </summary>
        /// <param name="uid">Discord id</param>
        /// <returns></returns>
        public WAVMemberCompitInfo GetCompitInfo(ulong uid)
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
        public void SetCompitInfo(ulong uid)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                member.CompitionInfo.ProvidedScore = true;
            }
        }

        /// <summary>
        /// Сбросить всю информацию об участии каждого человека в конкурсе
        /// </summary>
        public void ResetAllCompitInfop()
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
        /// Добавить участника в БД
        /// </summary>
        /// <param name="uid">Discord id участника, добавляемого в БД</param>
        public void CreateMember(ulong uid)
        {
            WAVMember member = new WAVMember()
            {
                Uid = uid,
                CompitionInfo = new WAVMemberCompitInfo(),
                LastActivity = DateTime.Now,
                OsuServers = new List<WAVMemberOsuProfileInfo>()
            };

            using (IDocumentSession session = store.OpenSession())
            {
                session.Store(member);
                session.SaveChanges();
            }
        }

        /// <summary>
        /// Добавить или обновить данные о сервере, на котором играет участник
        /// </summary>
        /// <param name="uid">Uid участника</param>
        /// <param name=""></param>
        public void AddOsuServerInfo(ulong uid, string server, int id)
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
                    member.OsuServers.Add(new WAVMemberOsuProfileInfo()
                    {
                        Server = server,
                        Id = id,
                        BestLast = DateTime.Now,
                        RecentLast = DateTime.Now
                    });
                }
            }
        }

        /// <summary>
        /// Получить информацию об osu! профиле участника WAV
        /// </summary>
        /// <param name="uid">Discord id участника WAV</param>
        /// <param name="server">Название сервера</param>
        /// <returns></returns>
        public WAVMemberOsuProfileInfo GetOsuProfileInfo(ulong uid, string server)
        {
            using (IDocumentSession session = store.OpenSession(new SessionOptions() { NoTracking = true }))
            {
                WAVMember member = session.Query<WAVMember>()
                                          .Include(x => x.OsuServers)
                                          .FirstOrDefault(x => x.Uid == uid);

                return member.OsuServers.FirstOrDefault(x => x.Server == server);
            }
        }
    }
}
