using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using OsuParsers.Replays;
using OsuParsers.Decoders;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Converters;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Bancho.Models;
using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.Commands
{
    public class OsuCommands : SkBaseCommandModule
    {
        private ILogger<OsuCommands> logger;

        private DiscordChannel wavScoresChannel;
        private DiscordGuild guild;

        private WebClient webClient;
        private OsuUtils utils;
        private OsuEmoji emoji;

        private BanchoApi api;
        private GatariApi gapi;

        private readonly ulong WAV_UID = 708860200341471264;

        public OsuCommands(ILogger<OsuCommands> logger, DiscordClient client, OsuUtils utils, BanchoApi api, GatariApi gapi, OsuEmoji emoji)
        {
            ModuleName = "Osu commands";

            this.logger = logger;
            this.wavScoresChannel = client.GetChannelAsync(829466881353711647).Result;
            this.webClient = new WebClient();

            this.guild = client.GetGuildAsync(WAV_UID).Result;
            this.utils = utils;
            this.api = api;
            this.gapi = gapi;
            this.emoji = emoji;

            logger.LogInformation("OsuCommands loaded");

            client.MessageCreated += Client_MessageCreated;
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            Tuple<int, int> BMSandBMid = utils.GetIdsFromBanchoString(e.Message.Content);

            if (!(BMSandBMid is null) && (e.Channel.Name.Contains("-osu") || e.Channel.Name.Contains("map-offer")))
            {
                int bms_id = BMSandBMid.Item1,
                    bm_id = BMSandBMid.Item2;

                Beatmap bm = api.GetBeatmap(bm_id);
                Beatmapset bms = api.GetBeatmapset(bms_id);

                // Contruct message
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

                TimeSpan mapLen = TimeSpan.FromSeconds(bm.total_length);

                DiscordEmoji banchoRankEmoji = emoji.RankStatusEmoji(bm.ranked);
                DiscordEmoji diffEmoji = emoji.DiffEmoji(bm.difficulty_rating);

                // Check gatari
                GBeatmap gBeatmap = gapi.TryRetrieveBeatmap(bm.id);

                StringBuilder embedMsg = new StringBuilder();
                embedMsg.AppendLine($"{diffEmoji}  **__[{bm.version}]__**\n▸**Difficulty**: {bm.difficulty_rating}★\n▸**CS**: {bm.cs} ▸**HP**: {bm.drain} ▸**AR**: {bm.ar}\n\nBancho: {banchoRankEmoji} : [link](https://osu.ppy.sh/beatmapsets/{bms.id}#osu/{bm.id})\nLast updated: {bm.last_updated}");
                if (!(gBeatmap is null))
                {
                    DiscordEmoji gatariRankEmoji = emoji.RankStatusEmoji(gBeatmap.ranked);
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

                await e.Message.RespondAsync(embed: embedBuilder.Build());
            }
        }

        [Command("submit"), RequireDirectMessage]
        public async Task SubmitScore(CommandContext commandContext)
        {
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Id);

            logger.LogInformation($"DM {msg.Author}: {msg.Content} : {msg.Attachments.Count}");

            if (Settings.KOSTYL.IgnoreDMList.Contains(msg.Author.Id))
            {
                await commandContext.RespondAsync("Извините, но вы были внесены в черный список бота.");
                return;
            }

            if (msg.Attachments.Count == 0)
            {
                await commandContext.RespondAsync("Вы не прикрепили к сообщению никаких файлов.");
                return;
            }

            if (msg.Attachments.Count > 1)
            {
                await commandContext.RespondAsync("К сообщению можно прикрепить только один файл.");
                return;
            }

            DiscordAttachment attachment = msg.Attachments.First();

            if (!attachment.FileName.EndsWith("osr"))
            {
                await commandContext.RespondAsync("Файл не является реплеем.");
                return;
            }

            Replay replay = null;

            try
            {
                string fileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
                webClient.DownloadFile(attachment.Url, $"downloads/{fileName}");

                replay = ReplayDecoder.Decode($"downloads/{fileName}");
            }

            catch (Exception e)
            {
                logger.LogCritical(e, "Exception while parsing score");
            }

            DiscordMember member = await guild.GetMemberAsync(msg.Author.Id);
            string category = member.Roles.Select(x => x.Name)
                                          .Where((x) =>
                                          {
                                              foreach (var xx in (new string[] { "beginner", "alpha", "beta", "gamma", "delta", "epsilon" }))
                                                  if (x.Contains(xx))
                                                      return true;
                                              return false;
                                          })
                                          .FirstOrDefault();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Osu nickname: `{replay.PlayerName}`");
            sb.AppendLine($"Discord nickname: `{msg.Author.Username}`");
            sb.AppendLine($"Score: `{replay.ReplayScore:N0}`"); // Format: 123456789 -> 123 456 789
            sb.AppendLine($"Category: `{category ?? "No category"}`");
            sb.AppendLine($"Mods: `{utils.ModsToString((WAV_Osu_NetApi.Bancho.Models.Enums.Mods)replay.Mods)}`");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithAuthor(msg.Author.Username, iconUrl: msg.Author.AvatarUrl)
                                                                 .WithTitle($"Added replay {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}")
                                                                 .WithUrl(attachment.Url)
                                                                 .WithDescription(sb.ToString())
                                                                 .AddField("OSR Link:", attachment.Url)
                                                                 .AddField("File name:", $"`{attachment.FileName}`")
                                                                 .WithTimestamp(DateTime.Now);

            await wavScoresChannel.SendMessageAsync(embed: embed);
            await commandContext.RespondAsync("Ваш скор был отправлен на рассмотрение. Спасибо за участие!");
        }
    }
}
