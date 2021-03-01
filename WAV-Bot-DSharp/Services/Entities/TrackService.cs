using System;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Gatari.Models;
using WAV_Osu_NetApi.Bancho.Models;

using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Services.Structures;

using Microsoft.EntityFrameworkCore;

using NLog;
using WAV_Bot_DSharp.Converters;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Сервис, запускающий через определенный интервал запланированные задачи
    /// </summary>
    public class TrackService : ITrackService
    {
        private ILogger logger;

        private Timer timer;
        private OsuUtils utils;
        private DiscordClient client;
        private DiscordChannel gatariRecentChannel;
        private int gatariUserIterator = 0;
        private int banchoUserIterator = 0;

        private BackgroundQueue queue;

        private Random rnd = new Random();

        private TrackedUserContext trackedUsersDb;

        GatariApi gapi = new GatariApi();

        public TrackService(DiscordClient client, ILogger logger, TrackedUserContext trackedUsers, OsuUtils utils)
        {
            timer = new Timer(6000);
            timer.Elapsed += Check;
            timer.Start();

            queue = new BackgroundQueue();

            this.utils = utils;
            this.client = client;
            this.logger = logger;
            this.trackedUsersDb = trackedUsers;

            gatariRecentChannel = client.GetChannelAsync(800124240908648469).Result;
            logger.Info($"Gatari tracker online! got channel: {gatariRecentChannel.Name}");
        }

        private void Check(object sender, ElapsedEventArgs e)
        {
            //logger.Debug("Checking scores...");
            CheckRecentGatari();
        }

        private async void CheckRecentGatari()
        {
            TrackedUser user = NextGatariUser(ref gatariUserIterator);
            if (user is null)
                return;

            //logger.Debug(user.GatariId);

            GUser guser = null;
            if (!gapi.TryGetUser((int)user.GatariId, ref guser))
            {
                logger.Warn($"Couldn't find user {user.GatariId} on Gatari, but the record exists in database!");
                return;
            }

            //logger.Debug(guser.username);

            gatariUserIterator++;

            List<GScore> new_scores = new List<GScore>();
            List<GScore> available_scores = gapi.GetUserRecentScores((int)user.GatariId, 0, 3, false);
            List<GScore> available_mania_scores = gapi.GetUserRecentScores((int)user.GatariId, 3, 3, false);

            available_scores.AddRange(available_mania_scores);

            DateTime? latest_score = user.GatariRecentLastAt;

            DateTime latest_score_avaliable_time = available_scores.Select(x => x.time)
                                               .OrderByDescending(x => x)
                                               .First() + TimeSpan.FromSeconds(10);

            //logger.Debug($"{latest_score_avaliable_time} : {latest_score}");

            if (latest_score is null)
            {
                latest_score = latest_score_avaliable_time;
                UpdateGatariRecentTime(user.Id, latest_score);
            }

            foreach (var score in available_scores)
                if (score.time > latest_score)
                {
                    new_scores.Add(score);
                    if (latest_score < score.time)
                        latest_score = score.time;
                }

            if (new_scores.Count != 0)
            {
                Console.WriteLine();
                foreach (var score in new_scores)
                {
                    TimeSpan mapLen = TimeSpan.FromSeconds(score.beatmap.hit_length);

                    DiscordEmbed embed = utils.GatariScoreToEmbed(score, guser, mapLen);

                    UpdateGatariRecentTime(user.Id, latest_score_avaliable_time);

                    await client.SendMessageAsync(gatariRecentChannel,
                        embed: embed);
                }
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
                logger.Error($"Error on UpdateGatariRecentTime {e.Message}\n{e.StackTrace}");
            }
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
            catch(Exception e)
            {
                logger.Error($"Error on AddTrackRecent {e.Message}\n{e.StackTrace}");
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
                logger.Error($"Error on RemoveTrackRecent {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        private TrackedUser NextGatariUser(ref int iter)
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

                    if (iter >= users.Count) 
                    {
                        iter = 0;
                    }

                    transaction.Commit();
                    return users[iter];
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error on NextGatariUser {e.Message}\n{e.StackTrace}");
                return null;
            }
        }

        private TrackedUser NextBanchoUser(ref int iter)
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

                    if (iter >= users.Count)
                    {
                        iter = 0;
                    }

                    transaction.Commit();
                    return users[iter];
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error on NextBanchoUser {e.Message}\n{e.StackTrace}");
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
                logger.Error($"Error on UpdateBanchoRecentTime {e.Message}\n{e.StackTrace}");
            }
        }

        private void AddBanchoTrackRecent(User u)
        {
            try
            {
                using (var transaction = trackedUsersDb.Database.BeginTransaction())
                {
                    TrackedUser user = trackedUsersDb.TrackedUsers.FirstOrDefault(x => x.BanchoId == u.id);

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
                logger.Error($"Error on AddTrackRecent {e.Message}\n{e.StackTrace}");
            }
        }

        private bool RemoveBanchoTrackRecent(User u)
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
                logger.Error($"Error on RemoveTrackRecent {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public Task AddGatariTrackRecentAsync(GUser u) => queue.QueueTask(() => AddGatariTrackRecent(u));
        public Task<bool> RemoveGagariTrackRecentAsync(GUser u) => queue.QueueTask(() => RemoveGatariTrackRecent(u));


        public Task<bool> RemoveBanchoTrackRecentAsync(User u)
        {
            throw new NotImplementedException();
        }

        public Task AddBanchoTrackRecentAsync(User u)
        {
            throw new NotImplementedException();
        }
    }
}
