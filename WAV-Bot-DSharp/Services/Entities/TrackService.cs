using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using WAV_Bot_DSharp.Services.Structures;

using WAV_Osu_NetApi;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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

        GatariApi api = new GatariApi();
        DateTime last_score = DateTime.Now - TimeSpan.FromHours(2);

        public TrackService(DiscordClient client, ILogger logger)
        {
            timer = new Timer(10000);
            timer.Elapsed += Check;
            timer.Start();

            this.client = client;
            this.logger = logger;

            gatariRecentChannel = client.GetChannelAsync(800124240908648469).Result;
            logger.Info($"Tracker gatari online! got channel: {gatariRecentChannel.Name}");
        }

        private void Check(object sender, ElapsedEventArgs e)
        {
            logger.Info("Checking scores...");
            CheckRecentGatari();
        }

        private async void CheckRecentGatari()
        {
            List<WAV_Osu_NetApi.Models.Gatari.Score> new_scores = new List<WAV_Osu_NetApi.Models.Gatari.Score>();
            List<WAV_Osu_NetApi.Models.Gatari.Score> available_scores = api.GetUserRecentScores(21129, true, 3);

            //Console.WriteLine(available_scores.Last().time);

            DateTime latest_score = last_score;
            foreach (var score in available_scores)
                if (score.time > last_score)
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
                    logger.Info("Found one! Sending to channel...");
                    DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();
                    await client.SendMessageAsync(gatariRecentChannel,
                        embed: discordEmbed.AddField("New score: ", $"▸ Title: {score.beatmap.song_name}\n▸ {score.ranking}, {score.accuracy}%, {score.pp}, {score.count_300}, {score.count_100}, {score.count_50}, {score.count_miss}")
                                           .Build());
                }

                last_score = latest_score;
            }
        }
    }
}
