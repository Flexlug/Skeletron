//using System;
//using System.Text;
//using System.Linq;
//using System.Timers;
//using System.Threading.Tasks;
//using System.Collections.Generic;

//using DSharpPlus;
//using DSharpPlus.Entities;
//using DSharpPlus.CommandsNext;
//using DSharpPlus.CommandsNext.Attributes;

//using WAV_Osu_NetApi;
//using WAV_Osu_NetApi.Gatari.Models;
//using WAV_Osu_NetApi.Bancho.Models;

//using WAV_Bot_DSharp.Threading;
//using WAV_Bot_DSharp.Services.Interfaces;
//using WAV_Bot_DSharp.Services.Models;

//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//using WAV_Bot_DSharp.Converters;

//namespace WAV_Bot_DSharp.Services.Entities
//{
//    /// <summary>
//    /// Сервис, запускающий через определенный интервал запланированные задачи
//    /// </summary>
//    public class TrackService : ITrackService
//    {
//        private ILogger<TrackService> logger;

//        ITrackedUsersDBService trackedUsers;

//        private Timer timer;
//        private OsuUtils utils;
//        private OsuEmoji emoji;
//        private DiscordClient client;
//        private DiscordChannel gatariRecentChannel;
//        private DiscordChannel banchoRecentChannel;

//        private Random rnd = new Random();

//        GatariApi gapi = new GatariApi();
//        BanchoApi bapi = null;


//        private const int TIMER_INTERVAL = 10000;

//        public TrackService(DiscordClient client, ILogger<TrackService> logger, ITrackedUsersDBService trackedUsers, OsuUtils utils, OsuEmoji emoji, BanchoApi bapi)
//        {
//            timer = new Timer(TIMER_INTERVAL);
//            timer.Elapsed += Check;
//            timer.Start();

//            this.utils = utils;
//            this.emoji = emoji;
//            this.client = client;
//            this.logger = logger;
//            this.bapi = bapi;
//            this.trackedUsers = trackedUsers;

//            gatariRecentChannel = client.GetChannelAsync(800124240908648469).Result;
//            banchoRecentChannel = client.GetChannelAsync(815949279566888961).Result;

//            logger.LogDebug($"Gatari tracker channel: {gatariRecentChannel.Name}");
//            logger.LogDebug($"Bancho tracker channel: {banchoRecentChannel.Name}");
//            logger.LogDebug($"Timer interval: {TIMER_INTERVAL} ms");

//            logger.LogInformation("TrackService started");
//        }

//        private void Check(object sender, ElapsedEventArgs e)
//        {
//            //logger.Debug("Checking scores...");
//            CheckRecentGatari();
//            CheckRecentBancho();
//        }

//        private async void CheckRecentGatari()
//        {
//            WAVMember user = await trackedUsers.NextGatariUserAsync();

//            if (user is null)
//                return;

//            GUser guser = null;
//            if (!gapi.TryGetUser((int)user.GatariId, ref guser))
//            {
//                logger.LogWarning($"Couldn't find user {user.GatariId} on Gatari, but the record exists in database!");
//                return;
//            }

//            logger.LogDebug($"Got gatari user {guser.username} id {user.GatariId.ToString()}");

//            List<GScore> new_scores = new List<GScore>();
//            List<GScore> available_scores = gapi.GetUserRecentScores((int)user.GatariId, 0, 3, true);
//            List<GScore> available_mania_scores = gapi.GetUserRecentScores((int)user.GatariId, 3, 3, true);

//            available_scores.AddRange(available_mania_scores);

//            if (available_scores is null || available_scores.Count == 0)
//            {
//                logger.LogDebug($"No recent scores");
//                return;
//            }

//            DateTime? latest_score = user.GatariRecentLastAt;

//            DateTime latest_score_avaliable_time = available_scores.Select(x => x.time)
//                                               .OrderByDescending(x => x)
//                                               .First() + TimeSpan.FromSeconds(10);

//            logger.LogDebug($"Gatari user {guser.username}: latest tracked: {latest_score_avaliable_time} latest known: {latest_score}");

//            if (latest_score is null)
//            {
//                latest_score = latest_score_avaliable_time;
//                await trackedUsers.UpdateBanchoRecentTimeAsync(user.Id, latest_score);
//            }

//            foreach (var score in available_scores)
//                if (score.time > latest_score)
//                {
//                    new_scores.Add(score);
//                    if (latest_score < score.time)
//                        latest_score = score.time;
//                }

//            if (new_scores.Count != 0)
//            {
//                foreach (var score in new_scores)
//                {
//                    logger.LogDebug($"Found new score for {guser.username}");

//                    await trackedUsers.UpdateGatariRecentTimeAsync(user.Id, latest_score_avaliable_time);
//                    TimeSpan mapLen = TimeSpan.FromSeconds(score.beatmap.hit_length);

//                    Beatmap bb = bapi.GetBeatmap(score.beatmap.beatmap_id);

//                    bool shouldPost = true,
//                         trackingFailed = false;
//                    double percentage = 0;

//                    if (score.ranking.Equals("F"))
//                    {
//                        trackingFailed = true;
//                        percentage = utils.FailedScoreProgress(score, bb);
//                        if (percentage < 0.90 || score.mods.HasFlag(WAV_Osu_NetApi.Bancho.Models.Enums.Mods.NoFail) || mapLen < TimeSpan.FromMinutes(3))
//                        {
//                            shouldPost = false;
//                        }
//                    }


//                    if (shouldPost)
//                    {
//                        DiscordEmbed embed = utils.GatariScoreToEmbed(score, guser, mapLen);

//                        string msgContent = null;

//                        if (trackingFailed)
//                            msgContent = $"Press {emoji.RankingEmoji("F")} Completed {percentage * 100:##0.00}%";

//                        await client.SendMessageAsync(gatariRecentChannel,
//                            embed: embed,
//                            content: msgContent);
//                    }
//                }
//            }
//        }

//        private async void CheckRecentBancho()
//        {
//            WAVMember user = await trackedUsers.NextBanchoUserAsync();

//            if (user is null)
//                return;

//            User buser = null;
//            if (!bapi.TryGetUser((int)user.BanchoId, ref buser))
//            {
//                logger.LogWarning($"Couldn't find user {user.BanchoId} on Bancho, but the record exists in database!");
//                return;
//            }

//            logger.LogDebug($"Got bancho user {buser.username} id {user.BanchoId}");

//            List<Score> new_scores = new List<Score>();
//            List<Score> available_scores = bapi.GetUserRecentScores((int)user.BanchoId, true, 0, 3);
//            List<Score> available_mania_scores = bapi.GetUserRecentScores((int)user.BanchoId, true, 3, 3);

//            available_scores.AddRange(available_mania_scores);

//            if (available_scores is null || available_scores.Count == 0)
//            {
//                logger.LogDebug($"No recent scores");
//                return;
//            }

//            DateTime? latest_score = user.BanchoRecentLastAt;

//            DateTime latest_score_avaliable_time = available_scores.Select(x => x.created_at)
//                                               .OrderByDescending(x => x)
//                                               .First() + TimeSpan.FromSeconds(10);

//            logger.LogDebug($"Bancho user {buser.username}: latest tracked: {latest_score_avaliable_time} latest known: {latest_score}");

//            if (latest_score is null)
//            {
//                latest_score = latest_score_avaliable_time;
//                await trackedUsers.UpdateBanchoRecentTimeAsync(user.Id, latest_score);
//            }

//            foreach (var score in available_scores)
//                if (score.created_at > latest_score)
//                {
//                    new_scores.Add(score);
//                    if (latest_score < score.created_at)
//                        latest_score = score.created_at;
//                }

//            if (new_scores.Count != 0)
//            {
//                foreach (var score in new_scores)
//                {
//                    logger.LogDebug($"Found new score for {buser.username}");

//                    await trackedUsers.UpdateBanchoRecentTimeAsync(user.Id, latest_score_avaliable_time);
//                    TimeSpan mapLen = TimeSpan.FromSeconds(score.beatmap.total_length);

//                    // For max_combo info
//                    score.beatmap = bapi.GetBeatmap(score.beatmap.id);

//                    bool shouldPost = true,
//                         trackingFailed = false;
//                    double percentage = 0;

//                    if (score.rank.Equals("F"))
//                    {
//                        trackingFailed = true;
//                        percentage = utils.FailedScoreProgress(score);

//                        if (percentage < 0.90 || score.mods.Contains("NF") || mapLen <= TimeSpan.FromMinutes(3))
//                        {
//                            shouldPost = false;
//                        }
//                    }

//                    if (shouldPost)
//                    {
//                        DiscordEmbed embed = utils.BanchoScoreToEmbed(score, buser, mapLen);

//                        string msgContent = null;

//                        if (trackingFailed)
//                            msgContent = $"Press {emoji.RankingEmoji("F")} Completed {percentage * 100:##0.00}%";

//                        await client.SendMessageAsync(banchoRecentChannel,
//                            embed: embed,
//                            content: msgContent);
//                    }
//                }
//            }
//        }

//        public Task AddGatariTrackRecentAsync(GUser u) => trackedUsers.AddGatariTrackRecentAsync(u);
//        public Task<bool> RemoveGagariTrackRecentAsync(GUser u) => trackedUsers.RemoveGatariTrackRecentAsync(u);
//        public Task<bool> RemoveBanchoTrackRecentAsync(int u) => trackedUsers.RemoveBanchoTrackRecentAsync(u);
//        public Task AddBanchoTrackRecentAsync(int u) => trackedUsers.AddBanchoTrackRecentAsync(u);
//    }
//}
