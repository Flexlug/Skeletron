using System;
using System.Text;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Osu_NetApi.Bancho.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models.Enums;
using WAV_Osu_NetApi.Gatari.Models;
using WAV_Osu_NetApi.Bancho.Models;

namespace WAV_Bot_DSharp.Converters
{
    public class OsuUtils
    {
        private DiscordClient client { get; set; }
        private OsuEmoji osuEmoji { get; set; }

        public OsuUtils(DiscordClient client, OsuEmoji emoji)
        {
            this.client = client;
            this.osuEmoji = emoji;
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
            embedMessage.AppendLine($"[{score.beatmap.song_name}](https://osu.gatari.pw/s/{score.beatmap.beatmapset_id}#osu/{score.beatmap.beatmap_id})\n▸ **Difficulty**: {score.beatmap.difficulty:##0.00}★ ▸ **Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)} ▸ **BPM**: {score.beatmap.bpm} ▸ **Mods**: {ModsToString(score.mods)}");
            embedMessage.AppendLine($"▸ {rankEmoji} ▸ **{score.accuracy:##0.00}%** ▸ **{score.pp}** {osuEmoji.PPEmoji()} ▸ **{score.max_combo}x/{score.beatmap.fc}x**");

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

            embed.WithAuthor(user.username, $"https://osu.ppy.sh/users{user.id}", $"https://a.ppy.sh/{user.id}?{rnd.Next(1000, 9999)}")
                 .WithThumbnail($"https://b.ppy.sh/thumb/{score.beatmap.beatmapset_id}.jpg");


            StringBuilder embedMessage = new StringBuilder();
            embedMessage.AppendLine($"[{score.beatmapset.artist} - {score.beatmapset.title} [{score.beatmap.version}]](https://osu.ppy.sh/beatmapsets/{score.beatmapset.id}#osu/{score.beatmap.id})\n▸ **Difficulty**: {score.beatmap.difficulty_rating:##0.00}★ ▸ **Length**: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)} ▸ **BPM**: {score.beatmap.bpm} ▸ **Mods**: {string.Join(" ", score.mods)}");
            embedMessage.AppendLine($"▸ {rankEmoji} ▸ **{score.accuracy:##0.00}%** ▸ **{score.pp}** {osuEmoji.PPEmoji()} ▸ **{score.max_combo}x/{score.max_combo}x**");

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

    }
}

