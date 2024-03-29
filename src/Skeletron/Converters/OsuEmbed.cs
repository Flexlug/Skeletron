﻿using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using DSharpPlus.Entities;

using OsuNET_Api.Models.Bancho;
using OsuNET_Api.Models.Gatari;

using Microsoft.Extensions.Logging;

namespace Skeletron.Converters
{
    /// <summary>
    /// Class with Osu utils
    /// </summary>
    public class OsuEmbed
    {
        private OsuEmoji osuEmoji;
        private OsuEnums osuEnums;

        private ILogger<OsuEmbed> logger;

        //private DiscordRole beginnerRole { get; set; }
        //private DiscordRole alphaRole { get; set; }
        //private DiscordRole betaRole { get; set; }
        //private DiscordRole gammaRole { get; set; }
        //private DiscordRole deltaRole { get; set; }
        //private DiscordRole epsilonRole { get; set; }

        public OsuEmbed(OsuEmoji emoji, OsuEnums enums, ILogger<OsuEmbed> logger)
        {
            this.osuEmoji = emoji;
            this.osuEnums = enums;

            //this.wav_guild = guild;

            //this.beginnerRole = guild.GetRole(915129427040538644);
            //this.alphaRole = guild.GetRole(915129468174106644);
            //this.betaRole = guild.GetRole(915129493532852274);
            //this.gammaRole = guild.GetRole(915129516639264778);
            //this.deltaRole = guild.GetRole(915129540945281044);
            //this.epsilonRole = guild.GetRole(915129563477053440);

            this.logger = logger;

            logger.LogInformation("OsuEmbed loaded");
        }

        /// <summary>
        /// Build embed from beatmap and beatmapset information
        /// </summary>
        /// <param name="bm">Beatmap object</param>
        /// <param name="bms">Beatmapset object</param>
        /// <param name="gBeatmap">Beatmap object from gatari (if exists)</param>
        /// <returns></returns>
        public DiscordEmbed BeatmapToEmbed(Beatmap bm, Beatmapset bms, GBeatmap gBeatmap = null, bool recognizerWarn = false)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            TimeSpan mapLen = TimeSpan.FromSeconds(bm.total_length);

            DiscordEmoji banchoRankEmoji = osuEmoji.RankStatusEmoji(bm.ranked);
            DiscordEmoji diffEmoji = osuEmoji.DiffEmoji(bm.difficulty_rating);

            StringBuilder embedMsg = new StringBuilder();

            embedBuilder.WithTitle($"{banchoRankEmoji}  {bms.artist} – {bms.title} by {bms.creator}");
            embedBuilder.WithUrl(bm.url);

            
            embedMsg.AppendLine($"{diffEmoji}  **__[{bm.version}]__**");
            embedMsg.AppendLine($"▶️ : {bms.play_count} ❤️: {bms.favourite_count}");
            embedMsg.AppendLine($"▸**Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)}, **BPM**: {bm.bpm}");
            embedMsg.AppendLine($"▸**Difficulty**: {bm.difficulty_rating}★");
            embedMsg.AppendLine($"▸**CS**: {bm.cs} ▸**HP**: {bm.drain} ▸**AR**: {bm.ar} ▸**OD**: {bm.accuracy}");
            
            embedMsg.AppendLine();
            embedMsg.Append($"Bancho: {banchoRankEmoji} : [link](https://osu.ppy.sh/beatmapsets/{bms.id}#osu/{bm.id})\nLast updated: {bm.last_updated}\n");

            if (!(gBeatmap is null))
            {
                DiscordEmoji gatariRankEmoji = osuEmoji.RankStatusEmoji(gBeatmap.ranked);
                embedMsg.AppendLine($"\nGatari: {gatariRankEmoji} : [link](https://osu.gatari.pw/s/{gBeatmap.beatmapset_id}#osu/{gBeatmap.beatmap_id})\nLast updated: {(gBeatmap.ranking_data != 0 ? new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(gBeatmap.ranking_data).ToString() : "")}");
            }

            embedMsg.AppendLine();

            if (recognizerWarn)
            {
                embedMsg.AppendLine();
                embedMsg.AppendLine($"⚠ Difficulty recognition warning ⚠");
            }

            embedBuilder.WithDescription(embedMsg.ToString());
            embedBuilder.WithThumbnail(bms.covers.List2x);

            return embedBuilder.Build();
        }

        /// <summary>
        /// Получить Discord embed на основе bancho профиля
        /// </summary>
        /// <param name="user">Bancho профиль</param>
        /// <param name="scores">Скоры</param>
        /// <returns></returns>
        public DiscordEmbed UserToEmbed(User user, List<Score> scores = null)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            StringBuilder sb = new StringBuilder();

            if (user.is_supporter)
            {
                sb.Append("**Supporter:** ");
                sb.AppendLine(string.Concat(Enumerable.Repeat($"{osuEmoji.RankStatusEmoji(RankStatus.Loved)}️", user.support_level)));
            }

            if (user.title is not null)
            {
                sb.AppendLine($"**Title:** __**{user.title}**__");
            }

            if (user.groups is not null && user.groups.Count != 0)
            { 
                sb.AppendLine($"**Groups:** {string.Join(", ", user.groups.Select(x => $"__**{x.short_name}**__"))}");
            }

            if (user.statistics.global_rank is null || user.statistics.country_rank is null)
            {
                sb.AppendLine($"**Rank:** `-` ({user.country_code} `-`)");
            }
            else
            {
                sb.AppendLine($"**Rank:** `#{user.statistics.global_rank}` ({user.country_code} `#{user.statistics.country_rank}`)");
            }

            sb.AppendLine($"**Level:** `{user.statistics.level.current}` + `{user.statistics.level.progress}%`");
            sb.AppendLine($"**PP:** `{user.statistics.pp} PP` **Acc**: `{user.statistics.hit_accuracy:f2}%`");
            sb.AppendLine($"**Playcount:** `{user.statistics.play_count}` (`{(Math.Round((double)user.statistics.play_time / 3600))}` hrs)");
            sb.AppendLine($"**Ranks**: {osuEmoji.RankingEmoji("XH")}`{user.statistics.grade_counts.ssh}` {osuEmoji.RankingEmoji("X")}`{user.statistics.grade_counts.ss}` {osuEmoji.RankingEmoji("SH")}`{user.statistics.grade_counts.sh}` {osuEmoji.RankingEmoji("S")}`{user.statistics.grade_counts.s}` {osuEmoji.RankingEmoji("A")}`{user.statistics.grade_counts.a}`");
            sb.AppendLine($"**Playstyle:** {(user.playstyle is null ? string.Empty : string.Join(", ", user.playstyle))}\n");
            sb.AppendLine($"**Server:** bancho");

            if (scores != null && scores?.Count != 0)
            {
                sb.AppendLine($"Top {scores.Count} scores:");

                double avg_pp = 0;
                for (int i = 0; i < scores.Count; i++)
                {
                    Score s = scores[i];

                    string mods = string.Join(' ', s.mods);
                    if (string.IsNullOrEmpty(mods))
                        mods = "NM";

                    sb.AppendLine($"{i + 1}: __[{s.beatmapset.title} [{s.beatmap.version}]]({s.beatmap.url})__ **{mods}** - {s.beatmap.difficulty_rating:f2}★");
                    sb.AppendLine($"▸ {osuEmoji.RankingEmoji(s.rank)} ▸ `{s.pp:f2} PP` ▸ **[{s.statistics.count_300}/{s.statistics.count_100}/{s.statistics.count_50}]**");

                    avg_pp += s.pp ?? 0;
                }
                sb.AppendLine($"\nAvg: `{Math.Round(avg_pp / scores.Count, 2)} PP`");
            }

            embedBuilder.WithTitle(user.username)
                        .WithUrl($"https://osu.ppy.sh/users/{user.id}")
                        .WithThumbnail(user.avatar_url)
                        .WithDescription(sb.ToString());

            return embedBuilder.Build(); ;
        }


        //public DiscordEmbed ScoresToLeaderBoard(CompitInfo compitInfo,
        //                                        List<CompitScore> beginnerScores = null,
        //                                        List<CompitScore> alphaScores = null,
        //                                        List<CompitScore> betaScores = null,
        //                                        List<CompitScore> gammaScores = null,
        //                                        List<CompitScore> deltaScores = null,
        //                                        List<CompitScore> epsilonScores = null)
        //{
        //    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

        //    embedBuilder.WithTitle("W.w.W participant list")
        //                .WithFooter($"Last update: {DateTime.Now}");

        //    StringBuilder descrSb = new StringBuilder();


        //    descrSb.AppendLine($"{beginnerRole?.Mention}\n[{compitInfo.BeginnerMap.beatmapset.artist} {compitInfo.BeginnerMap.beatmapset.title} [{compitInfo.BeginnerMap.version}] by {compitInfo.BeginnerMap.beatmapset.creator}]({compitInfo.BeginnerMap.url})");
        //    if (beginnerScores is not null && beginnerScores.Count != 0)
        //    {
        //        foreach (var score in beginnerScores.OrderByDescending(x => x.Score))
        //            descrSb.AppendLine($"`{score.DiscordNickname}`");
        //        descrSb.AppendLine();
        //    }
        //    else
        //    {
        //        descrSb.AppendLine($"-\n");
        //    }

        //    descrSb.AppendLine($"{alphaRole?.Mention}\n[{compitInfo.AlphaMap.beatmapset.artist} {compitInfo.AlphaMap.beatmapset.title} [{compitInfo.AlphaMap.version}] by {compitInfo.AlphaMap.beatmapset.creator}]({compitInfo.AlphaMap.url})");
        //    if (alphaScores is not null && alphaScores.Count != 0)
        //    {
        //        foreach (var score in alphaScores.OrderByDescending(x => x.Nickname))
        //            descrSb.AppendLine($"`{score.DiscordNickname}`");
        //        descrSb.AppendLine();
        //    }
        //    else
        //    {
        //        descrSb.AppendLine($"-\n");
        //    }

        //    descrSb.AppendLine($"{betaRole?.Mention}\n[{compitInfo.BetaMap.beatmapset.artist} {compitInfo.BetaMap.beatmapset.title} [{compitInfo.BetaMap.version}] by {compitInfo.BetaMap.beatmapset.creator}]({compitInfo.BetaMap.url})");
        //    if (betaScores is not null && betaScores.Count != 0)
        //    {
        //        foreach (var score in betaScores.OrderByDescending(x => x.Nickname))
        //            descrSb.AppendLine($"`{score.DiscordNickname}`");
        //        descrSb.AppendLine();
        //    }
        //    else
        //    {
        //        descrSb.AppendLine($"-\n");
        //    }

        //    descrSb.AppendLine($"{gammaRole?.Mention}\n[{compitInfo.GammaMap.beatmapset.artist} {compitInfo.GammaMap.beatmapset.title} [{compitInfo.GammaMap.version}] by {compitInfo.GammaMap.beatmapset.creator}]({compitInfo.GammaMap.url})");
        //    if (gammaScores is not null && gammaScores.Count != 0)
        //    {
        //        foreach (var score in gammaScores.OrderByDescending(x => x.Nickname))
        //            descrSb.AppendLine($"`{score.DiscordNickname}`");
        //        descrSb.AppendLine();
        //    }
        //    else
        //    {
        //        descrSb.AppendLine($"-\n");
        //    }

        //    descrSb.AppendLine($"{deltaRole?.Mention}\n[{compitInfo.DeltaMap.beatmapset.artist} {compitInfo.DeltaMap.beatmapset.title} [{compitInfo.DeltaMap.version}] by {compitInfo.DeltaMap.beatmapset.creator}]({compitInfo.DeltaMap.url})");
        //    if (deltaScores is not null && deltaScores.Count != 0)
        //    {
        //        foreach (var score in deltaScores.OrderByDescending(x => x.Nickname))
        //            descrSb.AppendLine($"`{score.DiscordNickname}`");
        //        descrSb.AppendLine();
        //    }
        //    else
        //    {
        //        descrSb.AppendLine($"-\n");
        //    }

        //    descrSb.AppendLine($"{epsilonRole?.Mention}\n[{compitInfo.EpsilonMap.beatmapset.artist} {compitInfo.EpsilonMap.beatmapset.title} [{compitInfo.EpsilonMap.version}] by {compitInfo.EpsilonMap.beatmapset.creator}]({compitInfo.EpsilonMap.url})");
        //    if (epsilonScores is not null && epsilonScores.Count != 0)
        //    {
        //        foreach (var score in epsilonScores.OrderByDescending(x => x.Nickname))
        //            descrSb.AppendLine($"`{score.DiscordNickname}`");
        //        descrSb.AppendLine();
        //    }
        //    else
        //    {
        //        descrSb.AppendLine($"-\n");
        //    }

        //    embedBuilder.WithDescription(descrSb.ToString());

        //    return embedBuilder.Build();
        //}

        //public DiscordEmbed CompitInfoToEmbed(CompitInfo compitInfo)
        //{
        //    DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

        //    string beginner = string.Empty,
        //           alpha = string.Empty,
        //           beta = string.Empty,
        //           gamma = string.Empty,
        //           delta = string.Empty,
        //           epsilon = string.Empty;

        //    if (compitInfo.BeginnerMap is null)
        //        beginner = "Нет";
        //    else
        //        beginner = $"{compitInfo.BeginnerMap.beatmapset.artist} - {compitInfo.BeginnerMap.beatmapset.title}\n{compitInfo.BeginnerMap.url}";

        //    if (compitInfo.AlphaMap is null)
        //        alpha = "Нет";
        //    else
        //        alpha = $"{compitInfo.AlphaMap.beatmapset.artist} - {compitInfo.AlphaMap.beatmapset.title}\n{compitInfo.AlphaMap.url}";

        //    if (compitInfo.BetaMap is null)
        //        beta = "Нет";
        //    else
        //        beta = $"{compitInfo.BetaMap.beatmapset.artist} - {compitInfo.BetaMap.beatmapset.title}\n{compitInfo.BetaMap.url}";

        //    if (compitInfo.GammaMap is null)
        //        gamma = "Нет";
        //    else
        //        gamma = $"{compitInfo.GammaMap.beatmapset.artist} - {compitInfo.GammaMap.beatmapset.title}\n{compitInfo.GammaMap.url}";

        //    if (compitInfo.DeltaMap is null)
        //        delta = "Нет";
        //    else
        //        delta = $"{compitInfo.DeltaMap.beatmapset.artist} - {compitInfo.DeltaMap.beatmapset.title}\n{compitInfo.DeltaMap.url}";

        //    if (compitInfo.EpsilonMap is null)
        //        epsilon = "Нет";
        //    else
        //        epsilon = $"{compitInfo.EpsilonMap.beatmapset.artist} - {compitInfo.EpsilonMap.beatmapset.title}\n{compitInfo.EpsilonMap.url}";

        //    embed.WithTitle("W.w.W status")
        //         .AddField("Запущен", compitInfo.IsRunning ? "Да" : "Нет")
        //         .AddField("Дата начала", compitInfo.StartDate?.ToString() ?? "Нет")
        //         .AddField("Дата завершения", compitInfo.Deadline?.ToString() ?? "Нет")
        //         .AddField("Канал для лидерборда", string.IsNullOrEmpty(compitInfo.LeaderboardChannelUID) ?
        //                                                                "Нет" :
        //                                                                wav_guild.GetChannel(ulong.Parse(compitInfo.LeaderboardChannelUID)).Name)
        //         .AddField("Канал для скоров", string.IsNullOrEmpty(compitInfo.ScoresChannelUID) ?
        //                                                            "Нет" :
        //                                                            wav_guild.GetChannel(ulong.Parse(compitInfo.ScoresChannelUID)).Name)
        //         .AddField("Лидерборд", string.IsNullOrEmpty(compitInfo.LeaderboardMessageUID) ? "Нет" : compitInfo.LeaderboardMessageUID)
        //         .AddField("Карта Beginner", beginner)
        //         .AddField("Карта Alpha", alpha)
        //         .AddField("Карта Beta", beta)
        //         .AddField("Карта Gamma", gamma)
        //         .AddField("Карта Delta", delta)
        //         .AddField("Карта Epsilon", epsilon);

        //    return embed.Build();
        //}

        /// <summary>
        /// Получить Discord embed на основе gatari профиля
        /// </summary>
        /// <param name = "user" > Gatari профиль</param>
        /// <param name = "scores" > Скоры </ param >
        /// < returns ></ returns >
        public DiscordEmbed UserToEmbed(GUser user, GStatistics stats, List<GScore> scores = null)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            StringBuilder sb = new StringBuilder(); 

            sb.AppendLine($"**Rank:** `{stats.rank}` ({user.country} `#{stats.country_rank}`)");
            sb.AppendLine($"**Level:** `{stats.level}` + `{stats.level_progress}%`");
            sb.AppendLine($"**PP:** `{stats.pp} PP` **Acc**: `{Math.Round(stats.avg_accuracy, 2)}%`");
            sb.AppendLine($"**Playcount:** `{stats.playcount}` (`{(Math.Round((double)stats.playtime / 3600))}` hrs)");
            sb.AppendLine($"**Ranks**: {osuEmoji.RankingEmoji("XH")}`{stats.xh_count}` {osuEmoji.RankingEmoji("X")}`{stats.x_count}` {osuEmoji.RankingEmoji("SH")}`{stats.sh_count}` {osuEmoji.RankingEmoji("S")}`{stats.s_count}` {osuEmoji.RankingEmoji("A")}`{stats.a_count}`\n");
            sb.AppendLine($"**Server:** gatari");
            //sb.AppendLine($"**Playstyle:** {user.play string.Join(", ", user.playstyle)}\n");
            sb.AppendLine("Top 5 scores:");

            if (scores != null && scores?.Count != 0)
            {
                double avg_pp = 0;
                for (int i = 0; i < scores.Count; i++)
                {
                    GScore s = scores[i];

                    string mods = string.Join(' ', s.mods);
                    if (string.IsNullOrEmpty(mods))
                        mods = "NM";

                    sb.AppendLine($"{i + 1}: __[{s.beatmap.song_name}](https://osu.gatari.pw/b/{s.beatmap.beatmap_id})__ **{mods}** - {s.beatmap.difficulty}★");
                    sb.AppendLine($"▸ {osuEmoji.RankingEmoji(s.ranking)} ▸ `{s.pp} PP` ▸ **[{s.count_300}/{s.count_100}/{s.count_50}]**");

                    avg_pp += s.pp ?? 0;
                }
                sb.AppendLine($"\nAvg: `{Math.Round(avg_pp / 5, 2)} PP`");
            }
            embedBuilder.WithTitle(user.username)
                        .WithUrl($"https://osu.gatari.pw/u/{user.id}")
                        .WithThumbnail($"https://a.gatari.pw/{user.id}?{new Random().Next(1000, 9999)}")
                        .WithDescription(sb.ToString());

            return embedBuilder.Build(); ;
        }


        /// <summary>
        /// Get embed from gatari score and user information
        /// </summary>
        /// <param name="score">Gatari score</param>
        /// <param name="user">Gatari user</param>
        /// <param name="mapLen">Map's length</param>
        /// <returns></returns>
        public DiscordEmbed GatariScoreToEmbed(GScore score, GUser user)
        {
            DiscordEmoji rankEmoji = osuEmoji.RankingEmoji(score.ranking);
            Random rnd = new Random();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            embed.WithAuthor(user.username, $"https://osu.gatari.pw/u/{user.id}", $"https://a.gatari.pw/{user.id}?{rnd.Next(1000, 9999)}")
                 .WithThumbnail($"https://b.ppy.sh/thumb/{score.beatmap.beatmapset_id}.jpg");

            TimeSpan mapLen = TimeSpan.FromSeconds(score.beatmap.hit_length);

            StringBuilder embedMessage = new StringBuilder();
            embedMessage.AppendLine($"[{osuEmoji.RankStatusEmoji(score.beatmap.ranked)} {score.beatmap.song_name} by {score.beatmap.creator}](https://osu.gatari.pw/s/{score.beatmap.beatmapset_id}#osu/{score.beatmap.beatmap_id})");
            embedMessage.AppendLine($"▸ **Difficulty**: {score.beatmap.difficulty:##0.00}★ ▸ **Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)} ▸ **BPM**: {score.beatmap.bpm} ▸ **Mods**: {osuEnums.ModsToString(score.mods)}");
            embedMessage.AppendLine($"▸ {rankEmoji} ▸ **{score.accuracy:##0.00}%** ▸ **{$"{(double)score.pp:##0.00}"}** {osuEmoji.PPEmoji()} ▸ **{score.max_combo}x/{score.beatmap.fc}x**");


            // mania
            if (score.play_mode == 3)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_katu} {osuEmoji.Hit200Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in osu!mania", embedMessage.ToString());
            }

            // ctb
            if (score.play_mode == 2)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_katu} {osuEmoji.Hit200Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in score osu!ctb", embedMessage.ToString());
            }

            // taiko
            if (score.play_mode == 1)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_katu} {osuEmoji.Hit200Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in osu!taiko", embedMessage.ToString());
            }

            //std
            if (score.play_mode == 0)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in osu!standard", embedMessage.ToString());
            }

            embed.WithFooter($"Played at: {score.time}");

            return embed.Build();
        }

        /// <summary>
        /// Получить Discord embed на основе скора пользователя
        /// </summary>
        /// <param name="score">Скор с Bancho</param>
        /// <param name="user">Пользователь, поставивший данный скор</param>
        /// <returns></returns>
        public DiscordEmbed BanchoScoreToEmbed(Score score, User user)
        {
            DiscordEmoji rankEmoji = osuEmoji.RankingEmoji(score.rank);
            Random rnd = new Random();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            TimeSpan mapLen = TimeSpan.FromSeconds(score.beatmap.hit_length);

            embed.WithAuthor(user.username, $"https://osu.ppy.sh/users/{user.id}", $"https://a.ppy.sh/{user.id}?{rnd.Next(1000, 9999)}")
                 .WithThumbnail($"https://b.ppy.sh/thumb/{score.beatmap.beatmapset_id}.jpg");


            StringBuilder embedMessage = new StringBuilder();
            embedMessage.AppendLine($"[{osuEmoji.RankStatusEmoji(score.beatmap.ranked)} {score.beatmapset.artist} - {score.beatmapset.title} [{score.beatmap.version}] by {score.beatmapset.creator}](https://osu.ppy.sh/beatmapsets/{score.beatmapset.id}#osu/{score.beatmap.id})");
            embedMessage.AppendLine($"▸ **Difficulty**: {score.beatmap.difficulty_rating:##0.00}★ ▸ **Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)} ▸ **BPM**: {score.beatmap.bpm} ▸ **Mods**: {((score.mods is null || score.mods.Count == 0) ? "NM" : string.Join(" ", score.mods))}");
            embedMessage.AppendLine($"▸ {rankEmoji} ▸ **{score.accuracy * 100:##0.00}%** ▸ ** {(score.pp is null ? "-" : $"{(double)score.pp:##0.00}")} ** {osuEmoji.PPEmoji()} ▸ **{score.max_combo}x/{score.beatmap.max_combo}x**");

            // mania
            if (score.mode_int == 3)    
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_katu} {osuEmoji.Hit200Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in osu!mania", embedMessage.ToString());
            }

            // ctb
            if (score.mode_int == 2)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_katu} {osuEmoji.Hit200Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in osu!ctb", embedMessage.ToString());
            }

            // taiko
            if (score.mode_int == 1)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_katu} {osuEmoji.Hit200Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in score osu!taiko", embedMessage.ToString());
            }

            //std
            if (score.mode_int == 0)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"Score in osu!standard", embedMessage.ToString());
            }

            embed.WithFooter($"Played at: {score.created_at}");

            return embed.Build();
        }

        /// <summary>
        /// Проверяет, насколько далеко была пройдена карта
        /// </summary>
        /// <param name="score">Скор с Bancho</param>
        /// <returns></returns>
        public double FailedScoreProgress(Score score)
        {
            int total_hits = score.statistics.count_50 + score.statistics.count_100 + score.statistics.count_300 + score.statistics.count_miss;
            int expected_hits = score.beatmap.count_circles + score.beatmap.count_sliders + score.beatmap.count_spinners;

            double progress = (double)total_hits / expected_hits;

            return progress;
        }

        /// <summary>
        /// Проверяет, насколько далеко была пройдена карта
        /// </summary>
        /// <param name="score">Скор с Gatari</param>
        /// <param name="bm">Карта, на которой был поставлен скор</param>
        /// <returns></returns>
        public double FailedScoreProgress(GScore score, Beatmap bm)
        {
            int total_hits = score.count_50 + score.count_100 + score.count_300 + score.count_miss;
            int expected_hits = bm.count_circles + bm.count_sliders + bm.count_spinners;

            double progress = (double)total_hits / expected_hits;

            return progress;
        }

    }
}

