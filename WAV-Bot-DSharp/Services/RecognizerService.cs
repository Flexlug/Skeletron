using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Exceptions;

using WAV_Osu_Recognizer;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Models.Bancho;
using WAV_Osu_NetApi.Models.Gatari;

using OneOf;
using DSharpPlus.EventArgs;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Реализация сервиса, который будет отслеживать скриншоты и скоры из osu!
    /// </summary>
    public class RecognizerService : IRecognizerService
    {
        private DiscordClient client;
        private WebClient webClient;
        private Recognizer recognizer;

        private BanchoApi api;
        private GatariApi gapi;

        private Dictionary<int, DateTime> ignoreList;
        private OsuEmoji emoji;
        private OsuEmbed utils;
        private OsuRegex regex;
        private BackgroundQueue queue;

        private DiscordChannel debugChannel;

        DiscordEmoji eyesEmoji;

        private ILogger<RecognizerService> logger;
        private IShedulerService sheduler;

        public RecognizerService(DiscordClient client, 
                                 Settings settings, 
                                 ILogger<RecognizerService> logger, 
                                 OsuEmoji emoji, 
                                 OsuEmbed utils,
                                 OsuRegex regex,
                                 IShedulerService sheduler)
        {
            this.client = client;
            this.logger = logger;
            this.utils = utils;
            this.sheduler = sheduler;

            this.emoji = emoji;
            this.regex = regex;

            this.debugChannel = client.GetChannelAsync(823835078298828841).Result;

            recognizer = new Recognizer();
            webClient = new WebClient();

            ignoreList = new Dictionary<int, DateTime>();

            api = new BanchoApi(settings.ClientId, settings.Secret);
            gapi = new GatariApi();

            queue = new BackgroundQueue();

            eyesEmoji = DiscordEmoji.FromName(client, ":eyes:");

            logger.LogInformation("RecognizerService loaded");
            ConfigureFilesInterceptor(client);
        }

        private void ConfigureFilesInterceptor(DiscordClient client)
        {
            client.MessageCreated += Client_OnMessageCreated;
        }

        private async Task Client_OnMessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            IReadOnlyCollection<DiscordAttachment> attachments = e.Message.Attachments;

            // Skip messages with no attachments
            if (attachments.Count == 0)
                return;

            logger.LogDebug($"Detected attachments. Count: {attachments.Count}");

            if (string.IsNullOrEmpty(e.Channel.Name))
            {
                logger.LogDebug($"Is dm");
                return;
            }

            if (!((e.Channel.Name?.Contains("-osu") ?? true) || 
                  (e.Channel.Name?.Contains("bot-debug") ?? true)))
            {
                logger.LogDebug($"Not osu channel");
                return;
            }

            foreach (DiscordAttachment attachment in attachments)
            {
                if (!(attachment.Width >= 750 && attachment.Height >= 550))
                    continue;

                string fileName = attachment.FileName.ToLower();

                // Ignore videofiles
                if ((fileName.EndsWith(".mp4") ||
                     fileName.EndsWith(".avi") ||
                     fileName.EndsWith(".mkv") ||
                     fileName.EndsWith(".m4v") ||
                     fileName.EndsWith(".webm") ||
                     fileName.EndsWith(".mov") ||
                     fileName.EndsWith(".mts") ||
                     fileName.EndsWith(".flv") ||
                     fileName.EndsWith(".3gp") ||
                     fileName.EndsWith(".m2ts") ||
                     fileName.EndsWith(".mpg") ||
                     fileName.EndsWith(".tga")))
                    continue;

                ThreadPool.QueueUserWorkItem(new WaitCallback(async delegate (object state)
                {
                    await ExecuteMessageTrack(e.Message, attachment);
                }));
            }
        }

        private bool check(MessageReactionAddEventArgs args, DiscordMessage message)
        {
            logger.LogInformation($"emoji: {args.Emoji}, id equals: {args.Message.Id == message.Id}, isBot: {args.Message.Author.IsBot}");
            return args.Emoji == eyesEmoji && 
                   args.Message.Id == message.Id && 
                   !args.User.IsBot && 
                   args.Message.Author.Username == message.Author.Username;
        }


        /// <summary>
        /// Начинает отслеживание реакций сообщения с картинкой.
        /// </summary>
        /// <param name="message">Отслеживаемое изображение</param>
        /// <param name="attachment">Картинка</param>
        private async Task ExecuteMessageTrack(DiscordMessage message, DiscordAttachment attachment)
        {
            await message.CreateReactionAsync(eyesEmoji);

            var interactivity = client.GetInteractivity();
            var pollres = await interactivity.WaitForReactionAsync(args => check(args, message),
                                                                           TimeSpan.FromSeconds(10));

            if (pollres.TimedOut)
            {
                logger.LogDebug("Raction wait timed out");
                await message.DeleteAllReactionsAsync();
                return;
            }

            logger.LogInformation($"Triggered recognition by {pollres.Result.User.Username}");
            logger.LogInformation($"Beatmap detect attempt");

            string[] recedText = null;
            string searchQuerry = string.Empty;

            var res = await queue.QueueTask(() => DownloadAndRecognizeImage(attachment, ref recedText, ref searchQuerry));

            Beatmapset banchoBeatmapset = null;
            Beatmap banchoBeatmap = null;

            Beatmapset firstBanchoBeatmapset = null;
            Beatmap firstBanchoBeatmap = null;

            // returned exception
            if (res.IsT2)
            {
                await message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(client, 800151438553776178));
                return;
            }

            GBeatmap gBeatmap;
            DiscordEmbed embed = null; 
            DiscordMessage msg = null;
            DiscordEmbedBuilder embedBuilder = null;
            var buttons = new List<DiscordButtonComponent>(new[]
                                                           {
                                                               new DiscordButtonComponent(ButtonStyle.Primary, $"wrong-{message.Id}", "Не та карта")
                                                           });

            // got only beatmapset
            if (res.IsT1)
            {
                banchoBeatmapset = res.AsT1;

                // Contruct message
                embedBuilder = new DiscordEmbedBuilder();


                if (banchoBeatmapset.beatmaps.Count > 1)
                {
                    List<DSharpPlus.Interactivity.Page> pages = new List<DSharpPlus.Interactivity.Page>();

                    foreach (var bm in banchoBeatmapset.beatmaps)
                    {
                        pages.Add(new DSharpPlus.Interactivity.Page("", new DiscordEmbedBuilder(utils.BeatmapToEmbed(bm, banchoBeatmapset, null, true))));
                    }

                    await interactivity.SendPaginatedMessageAsync(message.Channel,
                                                                  message.Author,
                                                                  pages,
                                                                  
                                                                  behaviour: DSharpPlus.Interactivity.Enums.PaginationBehaviour.WrapAround,
                                                                  deletion: DSharpPlus.Interactivity.Enums.ButtonPaginationBehavior.DeleteButtons);
                }
                else
                {
                    banchoBeatmap = banchoBeatmapset.beatmaps.Last();
                    msg = await message.RespondAsync(new DiscordMessageBuilder()
                                       .WithEmbed(utils.BeatmapToEmbed(banchoBeatmap, banchoBeatmapset, null, true))
                                       .AddComponents(buttons));
                }
            }

            // got both
            if (res.IsT0)
            {
                banchoBeatmapset = res.AsT0.Item1;
                banchoBeatmap = res.AsT0.Item2;

                // Ignore beatmap for several minutes
                foreach (var kvp in ignoreList)
                    if (DateTime.Now - kvp.Value > TimeSpan.FromMinutes(10))
                        ignoreList.Remove(kvp.Key);

                if (ignoreList.ContainsKey(banchoBeatmap.id))
                {
                    logger.LogInformation($"Beatmap is in ignore list {banchoBeatmap.id}");
                    return;
                }

                ignoreList.Add(banchoBeatmap.id, DateTime.Now);

                // Check gatari
                gBeatmap = gapi.TryGetBeatmap(banchoBeatmap.id);

                embed = utils.BeatmapToEmbed(banchoBeatmap, banchoBeatmapset, gBeatmap);

                msg = await message.RespondAsync(new DiscordMessageBuilder()
                    .WithEmbed(embed)
                    .AddComponents(buttons));
            }

            firstBanchoBeatmap = banchoBeatmap;
            firstBanchoBeatmapset = banchoBeatmapset;

            var resp = await interactivity.WaitForButtonAsync(msg, message.Author, TimeSpan.FromMinutes(1));
            if (resp.TimedOut)
            {
                logger.LogInformation($"Beatmap correction timed out - {banchoBeatmap.id}");
                await msg.ModifyAsync(new DiscordMessageBuilder()
                         .WithEmbed(embed));
                return;
            }
            logger.LogInformation($"Waiting for correction - {banchoBeatmap.id} by {resp.Result.User.Mention}");

            await resp.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                         .WithContent("Ответьте на это сообщение ссылкой на правильную карту (bancho url)"));

            var msg_res = await interactivity.WaitForMessageAsync((new_msg) => 
                                                                            new_msg.ReferencedMessage?.Id == msg.Id &&
                                                                            new_msg.Author.Id == message.Author.Id, 
                                                                  TimeSpan.FromMinutes(1));
            if (msg_res.TimedOut)
            {
                logger.LogInformation($"Correction timed out - {banchoBeatmap.id} by {msg_res.Result.Author.Mention}");
                await msg.ModifyAsync(new DiscordMessageBuilder()
                         .WithEmbed(embed));
                return;
            }

            var correct_bm = regex.GetBMandBMSIdFromBanchoUrl(msg_res.Result.Content);
            if (correct_bm is null)
            {
                logger.LogInformation($"Coudn't verify link - {banchoBeatmap.id} by {msg_res.Result.Author.Mention}");
                await msg_res.Result.RespondAsync("Ссылка не распознана");
                await msg.ModifyAsync(new DiscordMessageBuilder()
                .WithEmbed(embed));
                return;
            }
            logger.LogInformation($"Applying changes - {banchoBeatmap.id} by {msg_res.Result.Author.Mention}");

            banchoBeatmapset = api.GetBeatmapset(correct_bm.Item1);
            banchoBeatmap = api.GetBeatmap(correct_bm.Item2);
            // Contruct message
            embedBuilder = new DiscordEmbedBuilder();

            // Check gatari
            gBeatmap = gapi.TryGetBeatmap(banchoBeatmap.id);

            embed = utils.BeatmapToEmbed(banchoBeatmap, banchoBeatmapset, gBeatmap);

            await msg.ModifyAsync("", embed: embed);

            await debugChannel.SendMessageAsync("<@286893444448780290>", new DiscordEmbedBuilder()
                .WithDescription($"Search querry: `{searchQuerry}`\nRecognized text:\n```{string.Join('\n', recedText)}```")
                .WithTitle("Recognition error report")
                .WithImageUrl(message.Attachments.FirstOrDefault()?.Url ?? @"https://cdn.discordapp.com/icons/708860200341471264/a_9c50e0ada346ccf6528efc140bb4c4b1.gif?size=1024")
                .AddField("Message url: ", message.JumpLink.ToString())
                .AddField("Author: ", message.Author.Mention)
                .AddField("Wrong url: ", firstBanchoBeatmap.url)
                .AddField("Correct url: ", banchoBeatmap.url)
                .Build()
                );
        }

        /// <summary>
        /// Начинает распознавание картинки
        /// </summary>
        /// <param name="attachment">Картинка</param>
        private OneOf<Tuple<Beatmapset, Beatmap>, Beatmapset, BeatmapsetNotFoundException> DownloadAndRecognizeImage(DiscordAttachment attachment, ref string[] recedTextOut, ref string searchQuerry)
        {
            string webFileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
            webClient.DownloadFile(attachment.Url, $"downloads/{webFileName}");

            string fileName = $"downloads/{webFileName}";
            Image image = Image.FromFile(fileName);

            string[] rawrecedText = recognizer.RecognizeTopText(image).Split('\n');

            foreach (string s in rawrecedText)
                logger.LogInformation(s);

            // Searching for first non-empty or almost string
            string recedText = string.Empty;

            foreach (string s in rawrecedText)
                if (!string.IsNullOrWhiteSpace(s) && s.Length > 10)
                {
                    recedText = s;
                    break;
                }

            if (string.IsNullOrWhiteSpace(recedText))
                return new BeatmapsetNotFoundException();

            logger.LogDebug($"Recognized text: {recedText}");

            recedTextOut = rawrecedText;

            // Cut artist
            int indexStart = recedText.IndexOf('-');
            if (indexStart == -1)
                indexStart = recedText.IndexOf('—');

            if (indexStart != -1)
            {
                logger.LogDebug("Cutting artist");
                recedText = recedText.Substring(indexStart).TrimStart(new char[] { ' ', '-', '—' });
            }

            searchQuerry = recedText;

            logger.LogDebug($"Searching for: {recedText}");
            List<Beatmapset> bmsl = api.Search(recedText, MapType.Any);


            // Get map diff
            string diffName = string.Empty;
            string mapName = string.Empty;

            indexStart = recedText.IndexOf('[');
            if (indexStart == -1)
            {
                logger.LogInformation($"Coulnd't get map difficulty");

                mapName = recedText;
            }
            else
            {
                diffName = recedText.Substring(indexStart).TrimStart('[').TrimEnd(']');
                mapName = recedText.Substring(0, recedText.IndexOf('['));

                logger.LogDebug($"diffName: {diffName}");
            }

            if (bmsl == null || bmsl.Count == 0)
            {
                logger.LogInformation($"Api search return null or empty List");
                return new BeatmapsetNotFoundException();
            }

            string mapper = string.Empty;
            
            mapper = rawrecedText.Select(x => x)
                                 .FirstOrDefault(x =>
                                 {
                                     x = x.ToLowerInvariant();
                                     return x.Contains("mapped") || x.Contains("beatmap");
                                 });

            Beatmapset bms = null;
            if (!string.IsNullOrEmpty(mapper))
            {
                mapper = mapper?.Substring(10);
                logger.LogDebug($"Got mapper: {mapper}. Comparing...");

                List<Tuple<Beatmapset, double>> bsm = bmsl.Select(x => Tuple.Create(x, 
                                                                                    WAV_Osu_Recognizer.RecStringComparer.Compare(x.creator, mapper) +
                                                                                    WAV_Osu_Recognizer.RecStringComparer.Compare(x.title, mapName)))
                                                                  .OrderByDescending(x => x.Item2)
                                                                  .ToList();


                foreach (var b in bsm)
                    logger.LogDebug($"{b.Item1.creator} {b.Item1.title}: {b.Item2}");

                if (bsm.All(x => x.Item2 < 0.2))
                    return new BeatmapsetNotFoundException();

                if (bsm == null || bsm.Count == 0)
                    bms = bmsl.FirstOrDefault();
                else
                    bms = bsm.First().Item1;
            }
            else
            {
                logger.LogInformation($"Couldn't get mapper");

                List<Tuple<Beatmapset, double>> bsm = bmsl.Select(x => Tuple.Create(x,
                                                                    WAV_Osu_Recognizer.RecStringComparer.Compare(x.title, mapName)))
                                                  .OrderByDescending(x => x.Item2)
                                                  .ToList();


                foreach (var b in bsm)
                    logger.LogDebug($"{b.Item1.creator} {b.Item1.title}: {b.Item2}");

                if (bsm == null || bsm.Count == 0)
                    bms = bmsl.FirstOrDefault();
                else
                    bms = bsm.First().Item1;
            }


            if (bms == null)
            {
                logger.LogInformation($"No matching beatmapsets");
                return new BeatmapsetNotFoundException(); 
            }
            else
            {
                logger.LogDebug($"Beatmapsets count: {bmsl.Count}");
            }

            if (!string.IsNullOrEmpty(diffName))
            {
                List<Tuple<Beatmap, double>> bmds = bms.beatmaps.Select(x => Tuple.Create(x, WAV_Osu_Recognizer.RecStringComparer.Compare(x.version, diffName)))
                                                         .OrderByDescending(x => x.Item2)
                                                         .ToList();

                logger.LogDebug("Comparing beatmap versions:");
                foreach (var k in bmds)
                    logger.LogDebug($"{k.Item1.version} {k.Item2}");


                var result = Tuple.Create(bms, bmds.First().Item1);
                logger.LogInformation($"Success. bms_id: {result.Item1.id}, bm_id: {result.Item2.id}");
                return result;
            }
            else
            {
                var result = bms;
                logger.LogInformation($"Success. bms_id: {result.id}");

                return result;
            }
        }
    }
}