using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Osu_NetApi.Bancho.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models;
using WAV_Osu_NetApi.Bancho.Models;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Converters
{
    /// <summary>
    /// Class with Osu utils
    /// </summary>
    public class OsuUtils
    {
        private OsuEmoji osuEmoji { get; set; }

        private Regex banchoBMandBMSUrl { get; set; }
        private Regex banchoUserId { get; set; }
        private Regex gatariUserId { get; set; }
        private Regex gatariBMSUrl { get; set; }
        private Regex gatariBMUrl { get; set; }

        private ILogger logger { get; set; }

        public OsuUtils(OsuEmoji emoji, ILogger<OsuUtils> logger)
        {
            this.osuEmoji = emoji;
            this.banchoBMandBMSUrl = new Regex(@"http[s]?:\/\/osu.ppy.sh\/beatmapsets\/([0-9]*)#osu\/([0-9]*)");
            this.gatariBMSUrl = new Regex(@"http[s]?:\/\/osu.gatari.pw\/s\/([0-9]*)");
            this.gatariBMUrl = new Regex(@"http[s]?:\/\/osu.gatari.pw\/b\/([0-9]*)");
            this.banchoUserId = new Regex(@"http[s]?:\/\/osu.ppy.sh\/users\/([0-9]*)");
            this.gatariUserId = new Regex(@"http[s]?:\/\/osu.gatari.pw\/u\/([0-9]*)");

            this.logger = logger;
        }

        /// <summary>
        /// Get beatmapset and beatmap id from bancho url
        /// </summary>
        /// <param name="msg">Message, which contains url</param>
        /// <returns>Tuple, where first element is beatmapset id and second element - beatmap id</returns>
        public Tuple<int, int> GetBMandBMSIdFromBanchoUrl(string msg)
        {
            Match match = banchoBMandBMSUrl.Match(msg);

            if (match is null || match.Groups.Count != 3)
                return null;

            int bms_id, bm_id;

            if (int.TryParse(match.Groups[1].Value, out bms_id) && int.TryParse(match.Groups[2].Value, out bm_id))
                return Tuple.Create(bms_id, bm_id);

            return null;
        }

        /// <summary>
        /// Get beatmapset id from gatari url
        /// </summary>
        /// <param name="msg">Message which contains url</param>
        /// <returns>Id of beatmapset</returns>
        public int? GetBMSIdFromGatariUrl(string msg)
        {
            Match match = gatariBMSUrl.Match(msg);
            
            if (match is null || match.Groups.Count != 2)
                return null;

            int bms_id;

            if (int.TryParse(match.Groups[1].Value, out bms_id))
                return bms_id;

            return null;
        }

        /// <summary>
        /// Get beatmapset and beatmap id from gatari url
        /// </summary>
        /// <param name="msg">Message, which contains url</param>
        /// <returns>Tuple, where first element is beatmapset id and second element - beatmap id</returns>
        public int? GetBMIdFromGatariUrl(string msg)
        {
            Match match = gatariBMUrl.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int bm_id;

            if (int.TryParse(match.Groups[1].Value, out bm_id))
                return bm_id;

            return null;
        }

        /// <summary>
        /// Get user id from bancho url
        /// </summary>
        /// <param name="msg">Message, which contains bancho url</param>
        /// <returns>User id</returns>
        public int? GetUserIdFromBanchoUrl(string msg)
        {
            Match match = banchoUserId.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int user_id;

            if (int.TryParse(match.Groups[1].Value, out user_id))
                return user_id;

            return null;
        }

        /// <summary>
        /// Get user id from gatari url
        /// </summary>
        /// <param name="msg">Message, which contains gatari url</param>
        /// <returns>User id</returns>
        public int? GetUserIdFromGatariUrl(string msg)
        {
            Match match = gatariUserId.Match(msg);

            if (match is null || match.Groups.Count != 2)
                return null;

            int user_id;

            if (int.TryParse(match.Groups[1].Value, out user_id))
                return user_id;

            return null;
        }

        /// <summary>
        /// Build embed from beatmap and beatmapset information
        /// </summary>
        /// <param name="bm">Beatmap object</param>
        /// <param name="bms">Beatmapset object</param>
        /// <param name="gBeatmap">Beatmap object from gatari (if exists)</param>
        /// <returns></returns>
        public DiscordEmbed BeatmapToEmbed(Beatmap bm, Beatmapset bms, GBeatmap gBeatmap = null)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            TimeSpan mapLen = TimeSpan.FromSeconds(bm.total_length);

            DiscordEmoji banchoRankEmoji = osuEmoji.RankStatusEmoji(bm.ranked);
            DiscordEmoji diffEmoji = osuEmoji.DiffEmoji(bm.difficulty_rating);
            
            StringBuilder embedMsg = new StringBuilder();
            embedMsg.AppendLine($"{diffEmoji}  **__[{bm.version}]__**\n▸**Difficulty**: {bm.difficulty_rating}★\n▸**CS**: {bm.cs} ▸**HP**: {bm.drain} ▸**AR**: {bm.ar} ▸**OD**: {bm.accuracy}\n\nBancho: {banchoRankEmoji} : [link](https://osu.ppy.sh/beatmapsets/{bms.id}#osu/{bm.id})\nLast updated: {bm.last_updated}");
            if (!(gBeatmap is null))
            {
                DiscordEmoji gatariRankEmoji = osuEmoji.RankStatusEmoji(gBeatmap.ranked);
                embedMsg.AppendLine($"\nGatari: {gatariRankEmoji} : [link](https://osu.gatari.pw/s/{gBeatmap.beatmapset_id}#osu/{gBeatmap.beatmap_id})\nLast updated: {(gBeatmap.ranking_data != 0 ? new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(gBeatmap.ranking_data).ToString() : "")}");
            }

            // Construct embed
            embedBuilder.WithTitle($"{banchoRankEmoji}  {bms.artist} – {bms.title} by {bms.creator}");
            embedBuilder.WithUrl(bm.url);
            embedBuilder.AddField($"Length: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)}, BPM: {bm.bpm}",
                                  embedMsg.ToString(),
                                  true);
            embedBuilder.WithThumbnail(bms.covers.List2x);
            embedBuilder.WithFooter(bms.tags);

            return embedBuilder.Build();
        }

        public DiscordEmbed UserToEmbed(User user, List<Score> scores = null)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"**Server:** bancho");
            sb.AppendLine($"**Rank:** `#{user.statistics.global_rank}` ({user.country_code} `#{user.statistics.country_rank}`)");
            sb.AppendLine($"**Level:** `{user.statistics.level.current}` + `{user.statistics.level.progress}%`");
            sb.AppendLine($"**PP:** `{user.statistics.pp} PP` **Acc**: `{user.statistics.hit_accuracy}%`");
            sb.AppendLine($"**Playcount:** `{user.statistics.play_count}` (`{(Math.Round((double)user.statistics.play_time / 3600))}` hrs)");
            sb.AppendLine($"**Ranks**: {osuEmoji.RankingEmoji("XH")}`{user.statistics.grade_counts.ssh}` {osuEmoji.RankingEmoji("X")}`{user.statistics.grade_counts.ss}` {osuEmoji.RankingEmoji("SH")}`{user.statistics.grade_counts.sh}` {osuEmoji.RankingEmoji("S")}`{user.statistics.grade_counts.s}` {osuEmoji.RankingEmoji("A")}`{user.statistics.grade_counts.a}`");
            sb.AppendLine($"**Playstyle:** {string.Join(", ", user.playstyle)}\n");
            sb.AppendLine("Top 5 scores:");

            if (scores != null && scores?.Count != 0)
            {
                double avg_pp = 0;
                for (int i = 0; i < scores.Count; i++)
                {
                    Score s = scores[i];

                    string mods = string.Join(' ', s.mods);
                    if (string.IsNullOrEmpty(mods))
                        mods = "NM";

                    sb.AppendLine($"{i + 1}: __{s.beatmapset.title} [{s.beatmap.version}]__ **{mods}** - {s.beatmap.difficulty_rating}★");
                    sb.AppendLine($"▸ {osuEmoji.RankingEmoji(s.rank)} ▸ `{s.pp} PP` ▸ **[{s.statistics.count_300}/{s.statistics.count_100}/{s.statistics.count_50}]**");

                    avg_pp += s.pp ?? 0;
                }
                sb.AppendLine($"\nAvg: `{Math.Round(avg_pp / 5, 2)} PP`");
            }
            embedBuilder.WithTitle(user.username)
                        .WithThumbnail(user.avatar_url)
                        .WithDescription(sb.ToString());

            return embedBuilder.Build(); ;
        }

        public DiscordEmbed UserToEmbed(GUser user, GStatistics stats, List<GScore> scores = null)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"**Server:** gatari");
            sb.AppendLine($"**Rank:** `{stats.rank}` ({user.country} `#{stats.country_rank}`)");
            sb.AppendLine($"**Level:** `{stats.level}` + `{stats.level_progress}%`");
            sb.AppendLine($"**PP:** `{stats.pp} PP` **Acc**: `{Math.Round(stats.avg_accuracy, 2)}%`");
            sb.AppendLine($"**Playcount:** `{stats.playcount}` (`{(Math.Round((double)stats.playtime / 3600))}` hrs)");
            sb.AppendLine($"**Ranks**: {osuEmoji.RankingEmoji("XH")}`{stats.xh_count}` {osuEmoji.RankingEmoji("X")}`{stats.x_count}` {osuEmoji.RankingEmoji("SH")}`{stats.sh_count}` {osuEmoji.RankingEmoji("S")}`{stats.s_count}` {osuEmoji.RankingEmoji("A")}`{stats.a_count}`\n");
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

                    sb.AppendLine($"{i + 1}: __{s.beatmap.song_name}__ **{mods}** - {s.beatmap.difficulty}★");
                    sb.AppendLine($"▸ {osuEmoji.RankingEmoji(s.ranking)} ▸ `{s.pp} PP` ▸ **[{s.count_300}/{s.count_100}/{s.count_50}]**");

                    avg_pp += s.pp ?? 0;
                }
                sb.AppendLine($"\nAvg: `{Math.Round(avg_pp / 5, 2)} PP`");
            }
            embedBuilder.WithTitle(user.username)
                        .WithThumbnail($"https://a.gatari.pw/{user.id}?{new Random().Next(1000, 9999)}")
                        .WithDescription(sb.ToString());

            return embedBuilder.Build(); ;
        }

        /// <summary>
        /// Translate osu mods to string
        /// </summary>
        /// <param name="mods">Osu mods</param>
        /// <returns></returns>
        public string ModsToString(Mods mods)
        {
            StringBuilder sb = new StringBuilder(20);

            if (mods is Mods.None)
                return " NM";

            if (mods.HasFlag(Mods.NoFail))
                sb.Append(" NF");

            if (mods.HasFlag(Mods.Easy))
                sb.Append(" EZ");

            if (mods.HasFlag(Mods.TouchDevice))
                sb.Append(" TD");

            if (mods.HasFlag(Mods.Hidden))
                sb.Append(" HD");

            if (mods.HasFlag(Mods.HardRock))
                sb.Append(" HR");

            if (mods.HasFlag(Mods.SuddenDeath))
                sb.Append(" SD");

            if (mods.HasFlag(Mods.DoubleTime))
                sb.Append(" DT");

            if (mods.HasFlag(Mods.Relax))
                sb.Append(" RX");

            if (mods.HasFlag(Mods.HalfTime))
                sb.Append(" HT");

            if (mods.HasFlag(Mods.Nightcore))
                sb.Append(" NC");

            if (mods.HasFlag(Mods.Flashlight))
                sb.Append(" FL");

            if (mods.HasFlag(Mods.Autoplay))
                sb.Append(" Auto");

            if (mods.HasFlag(Mods.Relax2))
                sb.Append(" AP");

            if (mods.HasFlag(Mods.Perfect))
                sb.Append(" PF");

            if (mods.HasFlag(Mods.Key1))
                sb.Append(" K1");

            if (mods.HasFlag(Mods.Key2))
                sb.Append(" K2");

            if (mods.HasFlag(Mods.Key3))
                sb.Append(" K3");

            if (mods.HasFlag(Mods.Key4))
                sb.Append(" K4");

            if (mods.HasFlag(Mods.Key5))
                sb.Append(" K5");

            if (mods.HasFlag(Mods.Key6))
                sb.Append(" K6");

            if (mods.HasFlag(Mods.Key7))
                sb.Append(" K7");

            if (mods.HasFlag(Mods.Key8))
                sb.Append(" K8");

            if (mods.HasFlag(Mods.Key9))
                sb.Append(" K9");

            if (mods.HasFlag(Mods.FadeIn))
                sb.Append(" FI");

            if (mods.HasFlag(Mods.Cinema))
                sb.Append(" Cinema");

            if (mods.HasFlag(Mods.Random))
                sb.Append(" Random");

            if (mods.HasFlag(Mods.Target))
                sb.Append(" Target Practice");

            if (mods.HasFlag(Mods.KeyCoop))
                sb.Append(" KeyCoop");

            if (mods.HasFlag(Mods.ScoreV2))
                sb.Append(" ScoreV2");

            if (mods.HasFlag(Mods.Mirror))
                sb.Append(" Mirror");

            return sb.ToString();
        }



        /// <summary>
        /// Get embed from gatari score and user information
        /// </summary>
        /// <param name="score">Gatari score</param>
        /// <param name="user">Gatari user</param>
        /// <param name="mapLen">Map's length</param>
        /// <returns></returns>
        public DiscordEmbed GatariScoreToEmbed(GScore score, GUser user, TimeSpan mapLen)
        {
            DiscordEmoji rankEmoji = osuEmoji.RankingEmoji(score.ranking);
            Random rnd = new Random();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            embed.WithAuthor(user.username, $"https://osu.gatari.pw/u/{user.id}", $"https://a.gatari.pw/{user.id}?{rnd.Next(1000, 9999)}")
                 .WithThumbnail($"https://b.ppy.sh/thumb/{score.beatmap.beatmapset_id}.jpg");


            StringBuilder embedMessage = new StringBuilder();
            embedMessage.AppendLine($"[{osuEmoji.RankStatusEmoji(score.beatmap.ranked)} {score.beatmap.song_name} by {score.beatmap.creator}](https://osu.gatari.pw/s/{score.beatmap.beatmapset_id}#osu/{score.beatmap.beatmap_id})");
            embedMessage.AppendLine($"▸ **Difficulty**: {score.beatmap.difficulty:##0.00}★ ▸ **Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)} ▸ **BPM**: {score.beatmap.bpm} ▸ **Mods**: {ModsToString(score.mods)}");
            embedMessage.AppendLine($"▸ {rankEmoji} ▸ **{score.accuracy:##0.00}%** ▸ **{$"{(double)score.pp:##0.00}"}** {osuEmoji.PPEmoji()} ▸ **{score.max_combo}x/{score.beatmap.fc}x**");


            // mania
            if (score.play_mode == 3)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_katu} {osuEmoji.Hit200Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!mania", embedMessage.ToString());
            }

            // ctb
            if (score.play_mode == 2)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_katu} {osuEmoji.Hit200Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!ctb", embedMessage.ToString());
            }

            // taiko
            if (score.play_mode == 1)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_katu} {osuEmoji.Hit200Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!taiko", embedMessage.ToString());
            }

            //std
            if (score.play_mode == 0)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.count_300} {osuEmoji.Hit300Emoji()}, {score.count_100} {osuEmoji.Hit100Emoji()}, {score.count_50} {osuEmoji.Hit50Emoji()}, {score.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!standard", embedMessage.ToString());
            }

            embed.WithFooter($"Played at: {score.time}");

            return embed.Build();
        }

        public DiscordEmbed BanchoScoreToEmbed(Score score, User user, TimeSpan mapLen)
        {
            DiscordEmoji rankEmoji = osuEmoji.RankingEmoji(score.rank);
            Random rnd = new Random();

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

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
                embed.AddField($"New recent score osu!mania", embedMessage.ToString());
            }

            // ctb
            if (score.mode_int == 2)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_katu} {osuEmoji.Hit200Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!ctb", embedMessage.ToString());
            }

            // taiko
            if (score.mode_int == 1)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_katu} {osuEmoji.Hit200Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!taiko", embedMessage.ToString());
            }

            //std
            if (score.mode_int == 0)
            {
                embedMessage.AppendLine($"▸ {score.score} [{score.statistics.count_300} {osuEmoji.Hit300Emoji()}, {score.statistics.count_100} {osuEmoji.Hit100Emoji()}, {score.statistics.count_50} {osuEmoji.Hit50Emoji()}, {score.statistics.count_miss} {osuEmoji.MissEmoji()}]");
                embed.AddField($"New recent score osu!standard", embedMessage.ToString());
            }

            embed.WithFooter($"Played at: {score.created_at}");

            return embed.Build();
        }

        /// <summary>
        /// Check, if beatmap is closed on setted percetange
        /// </summary>
        /// <param name="score">Bancho score</param>
        /// <param name="percentage">Required percentage</param>
        /// <returns></returns>
        public double FailedScoreProgress(Score score)
        {
            int total_hits = score.statistics.count_50 + score.statistics.count_100 + score.statistics.count_300 + score.statistics.count_miss;
            int expected_hits = score.beatmap.count_circles + score.beatmap.count_sliders + score.beatmap.count_spinners;

            double progress = (double)total_hits / expected_hits;

            logger.LogDebug($"Failed progress: {progress}");

            return progress;
        }

        /// <summary>
        /// Check, if beatmap is closed on setted percetange
        /// </summary>
        /// <param name="score">Gatari score</param>
        /// <param name="bm">Beatmap</param>
        /// <param name="percentage">Required percentage</param>
        /// <returns></returns>
        public double FailedScoreProgress(GScore score, Beatmap bm)
        {
            int total_hits = score.count_50 + score.count_100 + score.count_300 + score.count_miss;
            int expected_hits = bm.count_circles + bm.count_sliders + bm.count_spinners;

            double progress = (double)total_hits / expected_hits;

            logger.LogDebug($"Failed progress: {progress}");

            return progress;
        }

    }
}

