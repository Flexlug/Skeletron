using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using NLog;

using DSharpPlus;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Реализация сервиса, который отслеживает активность пользователей
    /// </summary>
    public class ActivityService : IActivityService
    {
        private readonly TimeSpan MAX_AFK_TIME = TimeSpan.FromDays(30);
        private readonly ulong WAV_UID = 708860200341471264;
        private readonly int PAGE_SIZE = 10;

        IServiceProvider provider;

        private int objectsCount;

        DiscordGuild guild;
        //UsersContext usersDb;
        UsersContext usersDb
        {
            get
            {
                return provider.GetService(typeof(UsersContext)) as UsersContext;
            }
        }

        ILogger logger;
        
        public ActivityService(IServiceProvider provider, DiscordClient client, ILogger logger)
        {
            this.provider = provider;
            this.logger = logger;
            guild = client.GetGuildAsync(WAV_UID).Result;

            usersDb.Database.BeginTransaction();
            objectsCount = usersDb.Database.ExecuteSqlCommand("SELECT COUNT(*) FROM Users");

            ConfigureEvents(client);
        }

        /// <summary>
        /// Добавить отслеживание активности по ивентам
        /// </summary>
        /// <param name="client"></param>
        private void ConfigureEvents(DiscordClient client)
        {
            client.InviteCreated += Client_OnInviteCreated;
            client.MessageCreated += Client_OnMessageCreated;
            client.MessageDeleted += Client_OnMessageDeleted;
            client.MessageReactionAdded += Client_OnMessageReactionAdded;
            client.MessageReactionRemoved += Client_OnMessageReactionRemoved;
            client.MessageUpdated += Client_OnMessageUpdated;
            client.TypingStarted += Client_OnTypingStarted;
            client.VoiceStateUpdated += Client_OnVoiceStateUpdated;
        }

        private async Task Client_OnVoiceStateUpdated(DiscordClient sender, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs e) => await RequestUpdateUser(e.User, "Voice channel interaction");
        private async Task Client_OnTypingStarted(DiscordClient sender, DSharpPlus.EventArgs.TypingStartEventArgs e) => await RequestUpdateUser(e.User, "Typing message");
        private async Task Client_OnMessageUpdated(DiscordClient sender, DSharpPlus.EventArgs.MessageUpdateEventArgs e) => await RequestUpdateUser(e.Author, "Updated message");
        private async Task Client_OnMessageReactionRemoved(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e) => await RequestUpdateUser(e.User, "Removed reaction");
        private async Task Client_OnMessageReactionAdded(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e) => await RequestUpdateUser(e.User, "Added reaction");
        private async Task Client_OnMessageDeleted(DiscordClient sender, DSharpPlus.EventArgs.MessageDeleteEventArgs e) => await RequestUpdateUser(e.Message.Author, "Message deleted");
        private async Task Client_OnMessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e) => await RequestUpdateUser(e.Author, "Message created");
        private async Task Client_OnInviteCreated(DiscordClient sender, DSharpPlus.EventArgs.InviteCreateEventArgs e) => await RequestUpdateUser(e.Invite.Inviter, "Invite created");

        /// <summary>
        /// Обновить пользователя в базе данных
        /// </summary>
        /// <param name="user"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task RequestUpdateUser(DiscordUser user, string reason) 
        {
            logger.Info($"UserUpdate: {user.Username}: {reason}");
            await ManualUpdateToPresent(user.Id);
        }

        /// <summary>
        /// Добавить пользователя в список отслеживания
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        /// <returns>Если true, то в БД были изменения, иначе false</returns>
        public async Task<bool> AddUser(ulong user)
        {
            UserInfo userInfo = await usersDb.Users.FirstOrDefaultAsync(x => x.Uid == user);

            if (userInfo == null)
            {
                await usersDb.AddAsync(new UserInfo()
                {
                    LastActivity = DateTime.Now,
                    Uid = user
                });
                await usersDb.SaveChangesAsync();

                objectsCount++;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает коллекцию пользователей, которые 
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <returns>Список AFK пользователей</returns>
        public async Task<List<UserInfo>> GetAFKUsers(int page)
        {
            return await usersDb.Users.Select(x => x)
                                      .Where(x => DateTime.Now - x.LastActivity > MAX_AFK_TIME)
                                      .OrderBy(x => x.LastActivity)
                                      .AsNoTracking()
                                      .ToListAsync();
        }

        /// <summary>
        /// Обновить список пользователей и добавить недостающих в базу данных
        /// </summary>
        /// <returns>Количество добавленных пользователей</returns>
        public async Task<int> UpdateCurrentUsers()
        {
            IReadOnlyCollection<DiscordUser> allUsers = await guild.GetAllMembersAsync();

            List<UserInfo> newUsers = new List<UserInfo>();

            foreach(DiscordUser user in allUsers)
            {
                // проверяем, есть ли юзер в бд
                if (await usersDb.Users.FirstOrDefaultAsync(x => x.Uid == user.Id) == null ? true : false)
                {
                    newUsers.Add(new UserInfo()
                    {
                        LastActivity = DateTime.Now,
                        Uid = user.Id
                    });
                }
            }

            if (newUsers.Count != 0)
            {
                await usersDb.AddRangeAsync(newUsers);
                await usersDb.SaveChangesAsync();
                objectsCount += newUsers.Count;
            }

            return newUsers.Count;
        }
        /// <summary>
        /// Удалить лишние записи в базе данных
        /// </summary>
        /// <returns>Количество удалённых записей</returns>
        public async Task<int> ExcludeAbsentUsers()
        {
            List<UserInfo> absentUsers = new List<UserInfo>();

            foreach(UserInfo user in usersDb.Users)
                if (await guild.GetMemberAsync(user.Uid) == null ? true : false)
                    usersDb.Add(user);

            if (absentUsers.Count != 0)
            {
                usersDb.Users.RemoveRange(absentUsers);
                objectsCount -= absentUsers.Count;
                await usersDb.SaveChangesAsync();
            }

            return absentUsers.Count;
        }

        /// <summary>
        /// Записывает новое время активности пользователя
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        public async Task ManualUpdateToPresent(ulong user)
        {
            UserInfo userInfo = await usersDb.Users.FirstAsync(x => x.Uid == user);
            userInfo.LastActivity = DateTime.Now;

            Task.WaitAll();
            await usersDb.SaveChangesAsync();
        }

        /// <summary>
        /// Вручную обновить ингформацию об активности пользователя. Последняя активность будет выставлена на заданное дату и время
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <param name="dateTime">Дата и время, на которое необходимо обновить активность</param>
        /// <returns></returns>
        public async Task ManualUpdate(ulong user, DateTime dateTime)
        {
            UserInfo userInfo = await usersDb.Users.FirstAsync(x => x.Uid == user);
            userInfo.LastActivity = dateTime;

            await usersDb.SaveChangesAsync();
        }

        /// <summary>
        /// Получить информацию об активности пользователей в пагинированной панели.
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <returns>Страницу с информацией о пользователях</returns>
        public async Task<List<UserInfo>> ViewActivityInfo(int page)
        {
            return await usersDb.Users.OrderBy(x => x.LastActivity)
                                      .Skip((page - 1) * PAGE_SIZE)
                                      .Take(PAGE_SIZE)
                                      .AsNoTracking()
                                      .ToListAsync();
        }
        
        /// <summary>
        /// Получить информацию об активности пользователя
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <returns>Дату последней активности пользователя</returns>
        public async Task<UserInfo> GetUser(ulong user)
        {
            return await usersDb.Users.AsNoTracking()
                                      .FirstAsync(x => x.Uid == user);
        }

        /// <summary>
        /// Удаляет пользователя из списка отслеживания
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        public async Task RemoveUser(ulong user)
        {
            UserInfo info = await usersDb.Users.FirstAsync(x => x.Uid == user);
            usersDb.Remove(info);

            objectsCount--;

            await usersDb.SaveChangesAsync();
        }

        /// <summary>
        /// Получить общее количество страниц
        /// </summary>
        /// <returns>Количество страниц в базе данных</returns>
        public async Task<int> GetTotalPages() => objectsCount / PAGE_SIZE + 1;
    }
}
