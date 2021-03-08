using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Services.Structures;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Databases.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Реализация сервиса, который отслеживает активность пользователей
    /// </summary>
    public class ActivityService : IActivityService
    {
        public static readonly int PAGE_SIZE = 10;

        private readonly TimeSpan MAX_AFK_TIME = TimeSpan.FromDays(30);
        private readonly ulong WAV_UID = 708860200341471264;

        BackgroundQueue queue;

        private int objectsCount;

        DiscordClient client;
        DiscordGuild guild;
        UsersContext usersDb;
        //UsersContext usersDb
        //{
        //    get
        //    {
        //        return provider.GetService(typeof(UsersContext)) as UsersContext;
        //    }
        //}

        ILogger<ActivityService> logger;

        public ActivityService(UsersContext users, DiscordClient client, ILogger<ActivityService> logger)
        {
            this.client = client;
            this.usersDb = users;
            this.logger = logger;

            guild = client.GetGuildAsync(WAV_UID).Result;
            queue = new BackgroundQueue();

            using (var transaction = usersDb.Database.BeginTransaction())
            {
                objectsCount = usersDb.Users.Count();
                transaction.Commit();
            }

            ConfigureEvents(client);

            logger.LogInformation("ActivityService loaded");
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
            logger.LogInformation($"UserUpdate: {user.Username}: {reason}");
            await ManualUpdateToPresentAsync(user.Id);
        }

        /// <summary>
        /// Добавить пользователя в список отслеживания
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        /// <returns>Если true, то в БД были изменения, иначе false</returns>
        public bool AddUser(ulong user)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    UserInfo userInfo = usersDb.Users.FirstOrDefault(x => x.Uid == user);

                    if (userInfo == null)
                    {
                        usersDb.Add(new UserInfo()
                        {
                            LastActivity = DateTime.Now,
                            Uid = user
                        });
                        usersDb.SaveChanges();

                        objectsCount++;
                        transaction.Commit();
                        return true;
                    }

                    transaction.Commit();
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in AddUser method");
                return false;
            }
        }

        /// <summary>
        /// Возвращает коллекцию пользователей, которые 
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <returns>Список AFK пользователей</returns>
        public List<UserInfo> GetAFKUsers(int page)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    List<UserInfo> users = usersDb.Users.ToList();

                    users = users.Where(x => DateTime.Now - x.LastActivity > MAX_AFK_TIME)
                                 .OrderBy(x => x.LastActivity)
                                 .ToList();

                    transaction.Commit();
                    return users;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in GetAFKUSers");
                return null;
            }
        }

        /// <summary>
        /// Обновить список пользователей и добавить недостающих в базу данных
        /// </summary>
        /// <returns>Количество добавленных пользователей</returns>
        public int UpdateCurrentUsers(IReadOnlyDictionary<ulong, DiscordMember> allUsers)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    List<UserInfo> newUsers = new List<UserInfo>();
                    foreach (DiscordUser user in allUsers.Values)
                    {
                        UserInfo us = usersDb.Users.FirstOrDefault(x => x.Uid == user.Id);

                        // проверяем, есть ли юзер в бд
                        if (us == null ? true : false)
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
                        usersDb.Users.AddRange(newUsers);
                        usersDb.SaveChanges();
                        objectsCount += newUsers.Count;
                    }

                    transaction.Commit();

                    return newUsers.Count;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in UpdateCurrentUsers");
                return 0;
            }
        }

        /// <summary>
        /// Удалить лишние записи в базе данных
        /// </summary>
        /// <returns>Количество удалённых записей</returns>
        public int ExcludeAbsentUsers(IReadOnlyDictionary<ulong, DiscordMember> currentMembers)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    List<UserInfo> absentUsers = new List<UserInfo>();
                    List<UserInfo> existingUsers = usersDb.Users.ToList();

                    foreach (UserInfo user in existingUsers)
                        if (!currentMembers.ContainsKey(user.Uid))
                        {
                            logger.LogInformation($"Going to delete {user.Uid}");
                            absentUsers.Add(user);
                        }

                    if (absentUsers.Count != 0)
                    {
                        usersDb.Users.RemoveRange(absentUsers);
                        objectsCount -= absentUsers.Count;
                        usersDb.SaveChanges();
                    }


                    transaction.Commit();
                    return absentUsers.Count;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in ExcludeAbsentUsers");
                return 0;
            }
        }

        /// <summary>
        /// Записывает новое время активности пользователя
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        public void ManualUpdateToPresent(ulong user)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    UserInfo userInfo = usersDb.Users.First(x => x.Uid == user);
                    userInfo.LastActivity = DateTime.Now;
                    usersDb.SaveChanges();

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in ManualUpdateToPresent");
            }
        }

        /// <summary>
        /// Вручную обновить ингформацию об активности пользователя. Последняя активность будет выставлена на заданное дату и время
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <param name="dateTime">Дата и время, на которое необходимо обновить активность</param>
        /// <returns></returns>
        public void ManualUpdate(ulong user, DateTime dateTime)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    UserInfo userInfo = usersDb.Users.First(x => x.Uid == user);
                    userInfo.LastActivity = dateTime;

                    usersDb.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in ManualUpdate");
            }
        }

        /// <summary>
        /// Получить информацию об активности пользователей в пагинированной панели.
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <returns>Страницу с информацией о пользователях</returns>
        public List<UserInfo> ViewActivityInfo(int page)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    List<UserInfo> users = usersDb.Users.OrderBy(x => x.LastActivity)
                                          .Skip((page - 1) * PAGE_SIZE)
                                          .Take(PAGE_SIZE)
                                          .AsNoTracking()
                                          .ToList();

                    transaction.Commit();
                    return users;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in ViewActivityInfo");
                return null;
            }
        }

        /// <summary>
        /// Получить информацию об активности пользователя
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <returns>Дату последней активности пользователя</returns>
        public UserInfo GetUser(ulong user)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    UserInfo userInfo = usersDb.Users.AsNoTracking()
                                                     .First(x => x.Uid == user);

                    transaction.Commit();
                    return userInfo;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error in GetUser");
                return null;
            }
        }

        /// <summary>
        /// Удаляет пользователя из списка отслеживания
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        public void RemoveUser(ulong user)
        {
            try
            {
                using (var transaction = usersDb.Database.BeginTransaction())
                {
                    UserInfo info = usersDb.Users.First(x => x.Uid == user);
                    usersDb.Remove(info);

                    objectsCount--;

                    usersDb.SaveChanges();
                    transaction.Commit();
                }
            }

            catch (Exception e)
            {
                logger.LogError(e, "Error in RemoveUser");
            }
        }

        /// <summary>
        /// Получить общее количество страниц
        /// </summary>
        /// <returns>Количество страниц в базе данных</returns>
        public async Task<int> GetTotalPagesAsync() => objectsCount / PAGE_SIZE + 1;

        public Task<int> UpdateCurrentUsersAsync(IReadOnlyDictionary<ulong, DiscordMember> allUsers) => queue.QueueTask(() => UpdateCurrentUsers(allUsers));
        public Task<int> ExcludeAbsentUsersAsync(IReadOnlyDictionary<ulong, DiscordMember> allUsers) => queue.QueueTask(() => ExcludeAbsentUsers(allUsers));
        public Task<List<UserInfo>> ViewActivityInfoAsync(int page) => queue.QueueTask(() => ViewActivityInfo(page));
        public Task<List<UserInfo>> GetAFKUsersAsync(int page) => queue.QueueTask(() => GetAFKUsers(page));
        public Task RemoveUserAsync(ulong user) => queue.QueueTask(() => RemoveUser(user));
        public Task<bool> AddUserAsync(ulong user) => queue.QueueTask(() => AddUser(user));
        public Task<UserInfo> GetUserAsync(ulong user) => queue.QueueTask(() => GetUser(user));
        public Task ManualUpdateToPresentAsync(ulong user) => queue.QueueTask(() => ManualUpdateToPresent(user));
        public Task ManualUpdateAsync(ulong user, DateTime dateTime) => queue.QueueTask(() => ManualUpdate(user, dateTime));
    }
}