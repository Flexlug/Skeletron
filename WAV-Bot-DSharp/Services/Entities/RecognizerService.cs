using System;
using System.Net;
using System.Linq;
using System.Text;
using System.DrawingCore;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using NLog;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Services.Interfaces;

using WAV_Osu_Recognizer;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Bancho.Models;
using WAV_Osu_NetApi.Gatari.Models;
using WAV_Osu_NetApi.Gatari.Models.Enums;
using WAV_Bot_DSharp.Converters;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Реализация сервиса, который будет отслеживать скриншоты и скоры из osu!
    /// </summary>
    public class RecognizerService : IRecognizerService
    {
        private DiscordClient client;
        private ILogger logger;

        private WebClient webClient;

        private Recognizer recognizer;

        private BanchoApi api;
        private GatariApi gapi;

        private Dictionary<int, DateTime> ignoreList;

        private OsuEmoji emoji;

        private BackgroundQueue queue;

        public RecognizerService(DiscordClient client, Settings settings, ILogger logger, OsuEmoji emoji)
        {
            this.client = client;
            this.logger = logger;

            this.emoji = emoji;

            recognizer = new Recognizer();
            webClient = new WebClient();

            ignoreList = new Dictionary<int, DateTime>();

            api = new BanchoApi(settings.ClientId, settings.Secret);
            gapi = new GatariApi();

            queue = new BackgroundQueue();

            logger.Debug("Osu service started");
            ConfigureFilesInterceptor(client);
        }

        private void ConfigureFilesInterceptor(DiscordClient client)
        {
            client.MessageCreated += Client_OnMessageCreated;
        }

        private async Task Client_OnMessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            //logger.Debug("Client_OnMessageCreated invoked");
            IReadOnlyCollection<DiscordAttachment> attachments = e.Message.Attachments;

            // Skip messages with no attachments
            if (attachments.Count == 0)
            {
                //logger.Debug("Client_OnMessageCreated skipped");
                return;
            }

            logger.Debug($"Detected attachments. Count: {attachments.Count}");

            foreach (DiscordAttachment attachment in attachments)
                if (attachment.Width > 800 && attachment.Height > 600)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(async delegate(object state) 
                    { 
                        await ExecuteMessageTrack(e.Message, attachment); 
                    }));

                    break;
                }
        }

        /// <summary>
        /// Начинает отслеживание реакций сообщения с картинкой.
        /// </summary>
        /// <param name="message">Отслеживаемое изображение</param>
        /// <param name="attachment">Картинка</param>
        private async Task ExecuteMessageTrack(DiscordMessage message, DiscordAttachment attachment)
        {
            var res = await queue.QueueTask(() => DownloadAndRecognizeImage(attachment));

            if (res == null)
            {
                return;
            }

            Beatmapset banchoBeatmapset = res.Item1;
            Beatmap banchoBeatmap = res.Item2;

            // Ignore beatmap for several minutes
            foreach (var kvp in ignoreList)
                if (DateTime.Now - kvp.Value > TimeSpan.FromSeconds(20))
                    ignoreList.Remove(kvp.Key);

            if (ignoreList.ContainsKey(banchoBeatmap.id))
            {
                logger.Info($"Beatmap is in ignore list {banchoBeatmap.id}");
                return;
            }

            ignoreList.Add(banchoBeatmap.id, DateTime.Now);

            // Contruct message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            TimeSpan mapLen = TimeSpan.FromSeconds(banchoBeatmap.total_length);

            DiscordEmoji banchoRankEmoji = emoji.RankStatusEmoji(banchoBeatmap.ranked);
            DiscordEmoji diffEmoji = emoji.DiffEmoji(banchoBeatmap.difficulty_rating);

            // Check gatari
            GBeatmap gBeatmap = gapi.TryRetrieveBeatmap(banchoBeatmap.id);
            
            StringBuilder embedMsg = new StringBuilder();
            embedMsg.AppendLine($"{diffEmoji}  **__[{banchoBeatmap.version}]__**\n▸**Difficulty**: {banchoBeatmap.difficulty_rating}★\n▸**CS**: {banchoBeatmap.cs} ▸**HP**: {banchoBeatmap.drain} ▸**AR**: {banchoBeatmap.ar}\n\nBancho: {banchoRankEmoji} : [link](https://osu.ppy.sh/beatmapsets/{banchoBeatmapset.id}#osu/{banchoBeatmap.id})\nLast updated: {banchoBeatmap.last_updated}");
            if (!(gBeatmap is null))
            {
                DiscordEmoji gatariRankEmoji = emoji.RankStatusEmoji(gBeatmap.ranked);
                embedMsg.AppendLine($"\nGatari: {gatariRankEmoji} : [link](https://osu.gatari.pw/s/{gBeatmap.beatmapset_id}#osu/{gBeatmap.beatmap_id})\nLast updated: {(gBeatmap.ranking_data != 0 ? new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(gBeatmap.ranking_data).ToString() : "")}");
            }

            // Construct embed
            embedBuilder.WithTitle($"{banchoRankEmoji}  {banchoBeatmapset.artist} – {banchoBeatmapset.title} by {banchoBeatmapset.creator}");
            embedBuilder.WithUrl(banchoBeatmap.url);
            embedBuilder.AddField($"Length: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)}, BPM: {banchoBeatmap.bpm}",
                                  embedMsg.ToString(),
                                  true);
            embedBuilder.WithThumbnail(banchoBeatmapset.covers.List2x);
            embedBuilder.WithFooter(banchoBeatmapset.tags);

            await message.RespondAsync(embed: embedBuilder.Build());
        }

        /// <summary>
        /// Начинает распознавание картинки
        /// </summary>
        /// <param name="attachment">Картинка</param>
        private Tuple<Beatmapset, Beatmap> DownloadAndRecognizeImage(DiscordAttachment attachment)
        {
            string fileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
            webClient.DownloadFile(attachment.Url, $"downloads/{fileName}");

            Image image = Image.FromFile($"downloads/{fileName}");

            string[] rawrecedText = recognizer.RecognizeTopText(image).Split('\n');

            foreach (string s in rawrecedText)
                logger.Debug(s);

            // Searching for first non-empty string
            string recedText = string.Empty;

            foreach (string s in rawrecedText)
                if (!string.IsNullOrWhiteSpace(s))
                {
                    recedText = s;
                    break;
                }

            logger.Debug($"Recognized text: {recedText}");

            // Cut artist
            int indexStart = recedText.IndexOf('-');
            if (indexStart != -1)
            {
                logger.Debug("Cutting artist");
                recedText = recedText.Substring(indexStart).TrimStart(new char[] { ' ', '-' });
            }
            logger.Debug($"Searching for: {recedText}");
            List<Beatmapset> bmsl = api.Search(recedText, WAV_Osu_NetApi.Bancho.QuerryParams.MapType.Any);


            // Get map diff
            indexStart = recedText.IndexOf('[');
            if (indexStart == -1)
                return null;

            string diffName = recedText.Substring(indexStart);
            logger.Debug($"diffName: {diffName}");

            if (bmsl == null || bmsl.Count == 0)
            {
                logger.Debug($"Api search return null or empty List");
                return null;
            }

            string mapper = string.Empty;
            //foreach(string s in rawrecedText)
            //{
            //    string sLow = s.ToLower();
                
            //    if (sLow.Contains("beatmap") || sLow.Contains("mapped")) 
            //    {
            //        logger.Debug("found mapper");
            //        mapper = s;
            //        break;
            //    }
            //    logger.Debug($"Skipping {sLow}");
            //}
            mapper = rawrecedText.Select(x => x)
                                 .Where(x =>
                                 {
                                     x = x.ToLowerInvariant();
                                     return x.Contains("mapped") || x.Contains("beatmap");
                                 })
                                 .FirstOrDefault();


            Beatmapset bms = null;
            if (!string.IsNullOrEmpty(mapper))
            {
                mapper = mapper?.Substring(10);
                logger.Debug($"Got mapper: {mapper}. Comparing...");
                List<Tuple<Beatmapset, double>> bsm = bmsl.Select(x => Tuple.Create(x, WAV_Osu_Recognizer.StringComparer.Compare(x.creator, mapper)))
                                                           .OrderByDescending(x => x.Item2)
                                                           .ToList();


                foreach (var b in bsm)
                    logger.Debug($"{b.Item1.creator}: {b.Item2}");

                if (bsm == null || bsm.Count == 0)
                    bms = bmsl.FirstOrDefault();
                else
                    bms = bsm.First().Item1;
            }
            else
            {
                bms = bmsl.FirstOrDefault();
            }


            if (bms == null)
            {
                logger.Debug($"No beatmapsets");
                return null; 
            }
            else
            {
                logger.Debug($"Beatmapsets count: {bmsl.Count}");
            }


            List<Tuple<Beatmap, double>> bmds = bms.beatmaps.Select(x => Tuple.Create(x, WAV_Osu_Recognizer.StringComparer.Compare(x.version, diffName)))
                                                     .OrderByDescending(x => x.Item2)
                                                     .ToList();
            logger.Debug("Comparing beatmap versions:");
            foreach (var k in bmds)
                logger.Debug($"{k.Item1.version} {k.Item2}");



            return Tuple.Create(bms, bmds.First().Item1);
        }
    }
}