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
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;

using WAV_Bot_DSharp.Services.Interfaces;

using WAV_Osu_Recognizer;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Bancho.Models;
using WAV_Bot_DSharp.Configurations;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Реализация сервиса, который будет отслеживать скриншоты и скоры из osu!
    /// </summary>
    public class OsuService : IOsuService
    {
        private DiscordEmoji[] _pollEmojiCache;
        private DiscordClient client;
        private ILogger logger;

        private WebClient webClient;

        private Recognizer recognizer;

        private BanchoApi api;

        public OsuService(DiscordClient client, Settings settings, ILogger logger)
        {
            this.client = client;
            this.logger = logger;

            recognizer = new Recognizer();
            webClient = new WebClient();

            api = new BanchoApi(settings.ClientId, settings.Secret);

            logger.Debug("Osu service started");
            ConfigureFilesInterceptor(client);
        }

        private void ConfigureFilesInterceptor(DiscordClient client)
        {
            client.MessageCreated += Client_OnMessageCreated;
        }

        private async Task Client_OnMessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            logger.Debug("Client_OnMessageCreated invoked");
            IReadOnlyCollection<DiscordAttachment> attachments = e.Message.Attachments;

            // Skip messages with no attachments
            if (attachments.Count == 0)
            {
                logger.Debug("Client_OnMessageCreated skipped");
                return;
            }

            logger.Debug($"Attachments count: {attachments.Count}");

            foreach (DiscordAttachment attachment in attachments)
                if (attachment.Width != 0 && attachment.Height != 0)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state) { ExecuteMessageTrack(e.Message, attachment); }));
                    break;
                }
        }

        /// <summary>
        /// Начинает отслеживание реакций сообщения с картинкой.
        /// </summary>
        /// <param name="message">Отслеживаемое изображение</param>
        /// <param name="attachment">Картинка</param>
        private void ExecuteMessageTrack(DiscordMessage message, DiscordAttachment attachment)
        {
            var interactivity = client.GetInteractivity();
            if (_pollEmojiCache == null)
            {
                _pollEmojiCache = new[] {
                        DiscordEmoji.FromName(client, ":grey_question:")
                    };
            }

            TimeSpan duration = TimeSpan.FromSeconds(5);

            // DoPollAsync adds automatically emojis out from an emoji array to a special message and waits for the "duration" of time to calculate results.
            var pollResult = interactivity.DoPollAsync(message, _pollEmojiCache, PollBehaviour.DeleteEmojis, duration);
            var questions = pollResult.Result[0].Total;

            if (questions > 0)
            {
                message.CreateReactionAsync(DiscordEmoji.FromName(client, ":white_check_mark:"));
                var res = DownloadAndRecognizeImage(attachment);

                if (res == null)
                {
                    message.DeleteAllReactionsAsync();
                    message.CreateReactionAsync(DiscordEmoji.FromName(client, ":x:"));
                    return;
                }

                Beatmapset bms = res.Item1;
                Beatmap bm = res.Item2;

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
                
                TimeSpan mapLen = TimeSpan.FromSeconds(bm.total_length);

                embedBuilder.WithTitle($"{bms.artist} – {bms.title} by {bms.creator}");
                embedBuilder.WithUrl(bm.url);
                embedBuilder.AddField($"Length: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)}, BPM: {bm.bpm}", 
                                      $"[{bm.version}]\n▸Difficulty: {bm.difficulty_rating}★\n▸AR: {bm.ar} ▸CS: {bm.cs}",
                                      true);
                embedBuilder.WithThumbnail(bms.covers.List2x);
                embedBuilder.WithFooter(bms.tags);

                message.RespondAsync(embed: embedBuilder.Build());
            }
            else
            {
                message.DeleteAllReactionsAsync();
            }
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

            string recedText = recognizer.RecognizeTopText(image).Split('\n').First();
            List<Beatmapset> bmsl = api.Search(recedText, WAV_Osu_NetApi.Bancho.QuerryParams.MapType.Any);

            if (bmsl == null || bmsl.Count == 0)
                return null;

            int indexStart = recedText.IndexOfAny(new char[] { '[' });
            if (indexStart == -1)
                return null;

            string diffName = recedText.Substring(indexStart);

            logger.Info($"Recognized map: {recedText} diffName: {diffName}");

            Beatmapset bms = bmsl?.First();
            if (bms == null)
                return null;

            Tuple<Beatmap, double> bmd = bms.beatmaps.Select(x => Tuple.Create(x, WAV_Osu_Recognizer.StringComparer.Compare(x.version, diffName)))
                                                     .OrderByDescending(x => x.Item2)
                                                     .FirstOrDefault();

            return Tuple.Create(bms, bmd.Item1);
        }
    }
}