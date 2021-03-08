using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using WAV_Bot_DSharp.Databases.Interfaces;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Services.Structures;
using WAV_Bot_DSharp.Threading;

using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.Databases.Entities
{
    public class TrackedUsersDbService : ITrackedUsersDbService
    {
        private int gatariUserIterator = 0;
        private int banchoUserIterator = 0;

        private BackgroundQueue queue;
        private TrackedUserContext trackedUsersDb;

        private ILogger<TrackedUsersDbService> logger;

        public TrackedUsersDbService(TrackedUserContext trackedUsers, ILogger<TrackedUsersDbService> logger)
        {
            this.trackedUsersDb = trackedUsers;
            this.logger = logger;

            queue = new BackgroundQueue();
        }


        private void AddGatariTrackRecent(GUser u)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.GatariId == u.id);

                    if (user is null)
                    {
                        user = new TrackedUser()
                        {
                            BanchoId = null,
                            BanchoTrackRecent = false,
                            BanchoRecentLastAt = null,
                            BanchoTopLastAt = null,
                            BanchoTrackTop = false,
                            GatariId = u.id,
                            GatariTrackRecent = true,
                            GatariRecentLastAt = null,
                            GatariTrackTop = false,
                            GatariTopLastAt = null
                        };

                        trackedUsersDb.TrackedUsers.Add(user);
                    }
                    else
                    {
                        user.GatariTrackRecent = true;
                        user.GatariRecentLastAt = null;
                    }

                    trackedUsersDb.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error on AddTrackRecent {e.Message}\n{e.StackTrace}");
            }
        }
        private bool RemoveGatariTrackRecent(GUser u)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.GatariId == u.id);

                    if (user is null)
                        return false;

                    user.GatariTrackRecent = false;
                    user.GatariRecentLastAt = null;
                    trackedUsersDb.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"Error on RemoveTrackRecent {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private bool RemoveBanchoTrackRecent(int u)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.GatariId == u);

                    if (user is null)
                        return false;

                    user.GatariTrackRecent = false;
                    user.GatariRecentLastAt = null;
                    trackedUsersDb.SaveChanges();

                    transaction.Commit();
                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"Error on RemoveTrackRecent {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private void AddBanchoTrackRecent(int u)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.BanchoId == u);

                    if (user is null)
                    {
                        user = new TrackedUser()
                        {
                            BanchoId = u,
                            BanchoTrackRecent = true,
                            BanchoRecentLastAt = null,
                            BanchoTopLastAt = null,
                            BanchoTrackTop = false,

                            GatariId = null,
                            GatariTrackRecent = false,
                            GatariRecentLastAt = null,
                            GatariTrackTop = false,
                            GatariTopLastAt = null
                        };

                        trackedUsersDb.TrackedUsers.Add(user);
                    }
                    else
                    {
                        user.BanchoTrackRecent = true;
                        user.GatariRecentLastAt = null;
                    }

                    trackedUsersDb.SaveChanges();
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error on AddTrackRecent {e.Message}\n{e.StackTrace}");
            }
        }

        private TrackedUser NextBanchoUser()
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    List<TrackedUser> users = trackedUsersDb.TrackedUsers.Select(x => x)
                                                                  .Where(x => x.BanchoId != null && x.BanchoTrackRecent)
                                                                  .AsNoTracking()
                                                                  .ToList();

                    if (users.Count == 0)
                        return null;

                    if (banchoUserIterator >= users.Count)
                    {
                        banchoUserIterator = 0;
                    }

                    transaction.Commit();
                    return users[banchoUserIterator++];
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error on NextBanchoUser {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private TrackedUser NextGatariUser()
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    List<TrackedUser> users = trackedUsersDb.TrackedUsers.Select(x => x)
                                                                  .Where(x => x.GatariId != null && x.GatariTrackRecent)
                                                                  .AsNoTracking()
                                                                  .ToList();

                    if (users.Count == 0)
                        return null;

                    if (gatariUserIterator >= users.Count)
                    {
                        gatariUserIterator = 0;
                    }

                    transaction.Commit();
                    return users[gatariUserIterator++];
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error on NextGatariUser {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private void UpdateBanchoRecentTime(ulong id, DateTime? dateTime)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.Id == id);

                    user.BanchoRecentLastAt = dateTime;

                    trackedUsersDb.SaveChanges();

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error on UpdateBanchoRecentTime {e.Message}\n{e.StackTrace}");
            }
        }

        private void UpdateGatariRecentTime(ulong id, DateTime? dateTime)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.Id == id);

                    user.GatariRecentLastAt = dateTime;

                    trackedUsersDb.SaveChanges();

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error on UpdateGatariRecentTime {e.Message}\n{e.StackTrace}");
            }
        }

        #region Async abuse

        public Task<TrackedUser> NextBanchoUserAsync() => queue.QueueTask(() => NextBanchoUser());
        public Task<TrackedUser> NextGatariUserAsync() => queue.QueueTask(() => NextGatariUser());
        public Task UpdateBanchoRecentTimeAsync(ulong id, DateTime? dateTime) => queue.QueueTask(() => UpdateBanchoRecentTime(id, dateTime));
        public Task UpdateGatariRecentTimeAsync(ulong id, DateTime? dateTime) => queue.QueueTask(() => UpdateGatariRecentTime(id, dateTime));
        public Task AddGatariTrackRecentAsync(GUser u) => queue.QueueTask(() => AddGatariTrackRecent(u));
        public Task<bool> RemoveGatariTrackRecentAsync(GUser u) => queue.QueueTask(() => RemoveGatariTrackRecent(u));
        public Task<bool> RemoveBanchoTrackRecentAsync(int u) => queue.QueueTask(() => RemoveBanchoTrackRecent(u));
        public Task AddBanchoTrackRecentAsync(int u) => queue.QueueTask(() => AddBanchoTrackRecent(u));

        #endregion
    }
}
