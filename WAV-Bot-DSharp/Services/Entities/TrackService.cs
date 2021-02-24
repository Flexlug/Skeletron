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

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Сервис, запускающий через определенный интервал запланированные задачи
    /// </summary>
    public class TrackService : ITrackService
    {
        private ILogger logger;

        private Timer timer;
        private DiscordClient client;
        private DiscordChannel gatariRecentChannel;
        private int userIterator = 0;

        private BackgroundQueue queue;

        private TrackedUserContext trackedUsersDb;

        GatariApi gapi = new GatariApi();

        public TrackService(DiscordClient client, ILogger logger, TrackedUserContext trackedUsers)
        {
            timer = new Timer(10000);
            timer.Elapsed += Check;
            timer.Start();

            queue = new BackgroundQueue();

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
            TrackedUser user = NextGatariUser(ref userIterator);
            if (user is null)
                return;

            logger.Trace(user.GatariId);

            GUser guser = null;
            if (!gapi.TryGetUser((int)user.GatariId, ref guser))
            {
                logger.Warn($"Couldn't find user {user.GatariId} on Gatari, but the record exists in database!");
                return;
            }

            logger.Trace(guser.username);

            userIterator++;

            List<GScore> new_scores = new List<GScore>();
            List<GScore> available_scores = gapi.GetUserRecentScores((int)user.GatariId, 0, 3, false);
            List<GScore> available_mania_scores = gapi.GetUserRecentScores((int)user.GatariId, 3, 3, false);

            available_scores.AddRange(available_mania_scores);

            DateTime? latest_score = user.GatariRecentLastAt;

            DateTime latest_score_avaliable_time = available_scores.Select(x => x.time)
                                               .OrderByDescending(x => x)
                                               .First() + TimeSpan.FromSeconds(10);

            logger.Trace($"{latest_score_avaliable_time} : {latest_score}");

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

                    logger.Trace("Found one! Sending to channel...");
                    DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();
                    discordEmbed.WithAuthor(guser.username, $"https://osu.gatari.pw/u/{guser.id}", $"https://a.gatari.pw/{guser.id}");
                    //discordEmbed.WithThumbnail($"https://assets.ppy.sh/beatmaps/{score.beatmap.beatmapset_id}/covers/list@2x.jpg");
                    discordEmbed.WithThumbnail($"https://b.ppy.sh/thumb/{score.beatmap.beatmapset_id}.jpg");

                    DiscordEmoji rankEmoji = Converters.OsuUtils.RankingEmoji(score.ranking, client);

                    StringBuilder embedMessage = new StringBuilder();
                    embedMessage.AppendLine($"[{score.beatmap.song_name}](https://osu.gatari.pw/s/{score.beatmap.beatmapset_id}#osu/{score.beatmap.beatmap_id})\n▸ **Difficulty**: {score.beatmap.difficulty:##0.00}★ ▸ **Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)} ▸ **BPM**: {score.beatmap.bpm} ▸ **Mods**: {Converters.OsuUtils.ModsToString(score.mods)}");
                    embedMessage.AppendLine($"▸ {rankEmoji} ▸ **{score.accuracy:##0.00}%** ▸ **{score.pp}** {Converters.OsuUtils.PPEmoji(client)} ▸ **{score.max_combo}x/{score.beatmap.fc}x**");
                    
                    //std
                    if (score.play_mode == 0)
                    {
                        embedMessage.AppendLine($"▸ [{score.count_300} {Converters.OsuUtils.Hit300Emoji(client)}, {score.count_100} {Converters.OsuUtils.Hit100Emoji(client)}, {score.count_50} {Converters.OsuUtils.Hit50Emoji(client)}, {score.count_miss} {Converters.OsuUtils.MissEmoji(client)}]");
                        discordEmbed.AddField($"New recent score osu!standard", embedMessage.ToString());
                    }

                    if (score.play_mode == 3)
                    {
                        embedMessage.AppendLine($"▸ [{score.count_300} {Converters.OsuUtils.Hit300Emoji(client)}, {score.count_katu} {Converters.OsuUtils.Hit200Emoji(client)}, {score.count_100} {Converters.OsuUtils.Hit100Emoji(client)}, {score.count_50} {Converters.OsuUtils.Hit50Emoji(client)}, {score.count_miss} {Converters.OsuUtils.MissEmoji(client)}]");
                        discordEmbed.AddField($"New recent score osu!mania", embedMessage.ToString());
                    }

                    discordEmbed.WithFooter($"Played at: {score.time}");

                    UpdateGatariRecentTime(user.Id, latest_score_avaliable_time);

                    await client.SendMessageAsync(gatariRecentChannel,
                        embed: discordEmbed.Build());
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

        private void AddTrackRecent(GUser u)
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
                            GatariRecentLastAt = DateTime.Now,
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

        private bool RemoveTrackRecent(GUser u)
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

        public Task AddTrackRecentAsync(GUser u) => queue.QueueTask(() => AddTrackRecent(u));
        public Task<bool> RemoveTrackRecentAsync(GUser u) => queue.QueueTask(() => RemoveTrackRecent(u));


        public Task<bool> RemoveTrackRecentAsync(User u)
        {
            throw new NotImplementedException();
        }

        public Task AddTrackRecentAsync(User u)
        {
            throw new NotImplementedException();
        }
    }
}
