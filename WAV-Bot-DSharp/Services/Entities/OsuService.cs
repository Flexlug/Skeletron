﻿using System;
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
using WAV_Bot_DSharp.Threading;

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

        private BackgroundQueue queue;

        public OsuService(DiscordClient client, Settings settings, ILogger logger)
        {
            this.client = client;
            this.logger = logger;

            recognizer = new Recognizer();
            webClient = new WebClient();

            api = new BanchoApi(settings.ClientId, settings.Secret);

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
            var interactivity = client.GetInteractivity();
            if (_pollEmojiCache == null)
            {
                _pollEmojiCache = new[] {
                        DiscordEmoji.FromName(client, ":grey_question:")
                    };
            }

            TimeSpan duration = TimeSpan.FromSeconds(10);

            // DoPollAsync adds automatically emojis out from an emoji array to a special message and waits for the "duration" of time to calculate results.
            var pollResult = interactivity.DoPollAsync(message, _pollEmojiCache, PollBehaviour.DeleteEmojis, duration);
            var questions = pollResult.Result[0].Total;

            if (questions > 0)
            {
                //message.CreateReactionAsync(DiscordEmoji.FromName(client, ":white_check_mark:"));
                var res = await queue.QueueTask(() => DownloadAndRecognizeImage(attachment));

                if (res == null)
                {
                    //message.DeleteAllReactionsAsync();
                    await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":x:"));
                    return;
                }

                Beatmapset bms = res.Item1;
                Beatmap bm = res.Item2;

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
                
                TimeSpan mapLen = TimeSpan.FromSeconds(bm.total_length);

                DiscordEmoji rankEmoji = null;
                switch (bm.ranked) {
                    // ranked
                    case 1:
                        rankEmoji = DiscordEmoji.FromGuildEmote(client, 805362757934383105);
                        break;

                    // qualified
                    case 3:
                        rankEmoji = DiscordEmoji.FromGuildEmote(client, 805364968593686549);
                        break;

                    // loved
                    case 4:
                        rankEmoji = DiscordEmoji.FromGuildEmote(client, 805366123902009356);
                        break;

                    // other
                    default:
                        rankEmoji = DiscordEmoji.FromGuildEmote(client, 805368650529767444);
                        break;
                }

                DiscordEmoji diffEmoji = null;
                // Easy
                if (bm.difficulty_rating <= 1)
                    diffEmoji = DiscordEmoji.FromGuildEmote(client, 805376602824900648);
                else
                {
                    // Normal
                    if (bm.difficulty_rating <= 2.7)
                        diffEmoji = DiscordEmoji.FromGuildEmote(client, 805372074050322442);
                    else
                    {
                        // Hard
                        if (bm.difficulty_rating <= 4)
                            diffEmoji = DiscordEmoji.FromGuildEmote(client, 805375515593670686);
                        else
                        {
                            // Insane
                            if (bm.difficulty_rating <= 5.2)
                                diffEmoji = DiscordEmoji.FromGuildEmote(client, 805375873276575745);
                            else
                            {
                                // Expert
                                if (bm.difficulty_rating <= 6.3)
                                    diffEmoji = DiscordEmoji.FromGuildEmote(client, 805377293449953330);
                                else
                                {
                                    diffEmoji = DiscordEmoji.FromGuildEmote(client, 805377677661569065);
                                }
                            }
                        }
                    }
                }


                embedBuilder.WithTitle($"{rankEmoji}  {bms.artist} – {bms.title} by {bms.creator}");
                embedBuilder.WithUrl(bm.url);
                embedBuilder.AddField($"Length: {mapLen.Minutes}:{string.Format("{0:00}", mapLen.Seconds)}, BPM: {bm.bpm}", 
                                      $"{diffEmoji}  [{bm.version}]\n▸Difficulty: {bm.difficulty_rating}★\n▸AR: {bm.ar} ▸CS: {bm.cs}",
                                      true);
                embedBuilder.WithThumbnail(bms.covers.List2x);
                embedBuilder.WithFooter(bms.tags);

                await message.RespondAsync(embed: embedBuilder.Build());
            }
            else
            {
                //message.DeleteAllReactionsAsync();
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