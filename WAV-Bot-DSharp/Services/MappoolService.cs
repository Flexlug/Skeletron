using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Converters;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Models.Bancho;
using WAV_Osu_NetApi.Models.Gatari.Enums;

using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

using OsuParsers.Database.Objects;
using DSharpPlus;

namespace WAV_Bot_DSharp.Services
{
    public class MappoolService : IMappoolService
    {
        private ILogger<MappoolService> logger;

        private IMappoolProvider mappoolProvider;
        private IWAVCompitProvider compitProvider;

        private DiscordChannel announceChannel;
        private DiscordChannel debugChannel;

        private MappoolSpectateStatus spectateStatus;

        private DiscordClient client;

        private DiscordMessage beginnerMappoolAnnounceMsg = null;
        private DiscordMessage alphaMappoolAnnounceMsg = null;
        private DiscordMessage betaMappoolAnnounceMsg = null;
        private DiscordMessage gammaMappoolAnnounceMsg = null;
        private DiscordMessage deltaMappoolAnnounceMsg = null;
        private DiscordMessage epsilonMappoolAnnounceMsg = null;

        private bool spectatingEnabled = false;

        private OsuRegex regex;
        private OsuEmoji emoji;

        private BanchoApi bapi;
        private GatariApi gapi;

        public MappoolService(ILogger<MappoolService> logger,
                              IMappoolProvider mappoolProvider,
                              IWAVCompitProvider compitProvider,
                              DiscordClient client,
                              OsuRegex regex,
                              OsuEmoji emoji,
                              BanchoApi bapi,
                              GatariApi gapi)
        {
            this.logger = logger;

            this.client = client;

            this.mappoolProvider = mappoolProvider;
            this.compitProvider = compitProvider;

            this.regex = regex;
            this.emoji = emoji;

            this.bapi = bapi;
            this.gapi = gapi;

            debugChannel = client.GetChannelAsync(823835078298828841).Result;

            CheckMappoolSpectateStatus();
        }

        private void CheckMappoolSpectateStatus()
        {
            spectateStatus = mappoolProvider.GetMappoolStatus();

            // Если отслеживание не запущено, то вырубаем проверку
            if (!spectateStatus.SpectateStatus)
                return;

            // Получить канал для анонсов
            ulong spectateId = ulong.Parse(spectateStatus.AnnounceChannelId);
            ulong beginnerId = ulong.Parse(spectateStatus.BeginnerMessageId);
            ulong alphaId = ulong.Parse(spectateStatus.AlphaMessageId);
            ulong betaId = ulong.Parse(spectateStatus.BetaMessageId);
            ulong gammaId = ulong.Parse(spectateStatus.GammaMessageId);
            ulong deltaId = ulong.Parse(spectateStatus.DeltaMessageId);
            ulong epsilonId = ulong.Parse(spectateStatus.EpsilonMessageId);

            try
            {
                announceChannel = client.GetChannelAsync(spectateId).Result;
                beginnerMappoolAnnounceMsg = announceChannel.GetMessageAsync(beginnerId).Result;
                alphaMappoolAnnounceMsg = announceChannel.GetMessageAsync(alphaId).Result;
                betaMappoolAnnounceMsg = announceChannel.GetMessageAsync(betaId).Result;
                gammaMappoolAnnounceMsg = announceChannel.GetMessageAsync(gammaId).Result;
                deltaMappoolAnnounceMsg = announceChannel.GetMessageAsync(deltaId).Result;
                epsilonMappoolAnnounceMsg = announceChannel.GetMessageAsync(epsilonId).Result;
            }
            catch (Exception ex)
            {
                debugChannel.SendMessageAsync($"Ошибка при запуске отслеживания маппула: {ex.InnerException}\n{ex.Message}");
            }
        }

        public string AddAdminMap(CompitCategory cat, string url)
        {
            var ids = regex.GetBMandBMSIdFromBanchoUrl(url);
            var bBeatmap = bapi.GetBeatmap(ids.Item2);
            if (bBeatmap is null)
                return $"Не удалось получить карту с id {ids.Item2}";

            if (mappoolProvider.CheckMapOffered(bBeatmap.id, cat))
                return "Данную карту уже кто-то предложил.";

            mappoolProvider.MapAdd(new()
            {
                AdminMap = true,
                Beatmap = bBeatmap,
                BeatmapId = ids.Item2,
                Category = cat,
                SuggestedBy = "admin",
                Votes = new() { "default" }
            });

            return "done";
        }

        public string AddMap(string memberId, string url)
        {
            var compitProfile = compitProvider.GetCompitProfile(memberId);

            if (compitProvider is null)
                return "Вы не зарегистрированы в конкурсе.";

            var cat = compitProfile.Category;

            var ids = regex.GetBMandBMSIdFromBanchoUrl(url);
            var bBeatmap = bapi.GetBeatmap(ids.Item2);
            if (bBeatmap is null)
                return $"Не удалось получить карту с id {ids.Item2}";

            if (!(bBeatmap.ranked == RankStatus.Ranked ||
                bBeatmap.ranked == RankStatus.Qualified ||
                bBeatmap.ranked == RankStatus.Loved ||
                bBeatmap.ranked == RankStatus.Approved))
            {
                var gBeatmap = gapi.TryGetBeatmap(ids.Item2);

                if (!(gBeatmap.ranked == GRankStatus.Ranked ||
                    gBeatmap.ranked == GRankStatus.Qualified ||
                    gBeatmap.ranked == GRankStatus.Loved ||
                    gBeatmap.ranked == GRankStatus.Approved))
                    return "Карта не подходит из-за своего ранк статуса.";
            }

            if (mappoolProvider.CheckUserSubmitedAny(memberId) || mappoolProvider.CheckUserVoted(memberId))
                return "Вы уже предложили карту или проголосовали.";

            mappoolProvider.MapAdd(new()
            {
                AdminMap = false,
                Beatmap = bBeatmap,
                BeatmapId = ids.Item2,
                Category = cat,
                SuggestedBy = memberId,
                Votes = new() { memberId }
            });

            return "done";
        }

        public DiscordEmbed GetCategoryMappool(CompitCategory cat)
        {
            var maps = mappoolProvider.GetCategoryMaps(cat);

            StringBuilder str = new();
            if (maps is null || maps.Count == 0)
                str.AppendLine("Никто ещё не предложил карт для данной категории.");

            foreach (var map in maps)
                str.AppendLine(OfferedMapToString(map));

            return new DiscordEmbedBuilder()
                .WithTitle($"Предложка для категории {cat}")
                .WithDescription(str.ToString())
                .WithFooter($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}")
                .Build();
        }

        public DiscordEmbed GetCategoryMappool(DiscordMember user)
        {
            var compitProfile = compitProvider.GetCompitProfile(user.Id.ToString());

            if (compitProfile is null)
                return new DiscordEmbedBuilder()
                    .WithTitle("Ошибка")
                    .WithDescription("Вы не зарегистрированы в конкурсе.");

            var cat = compitProfile.Category;

            var maps = mappoolProvider.GetCategoryMaps(cat);

            StringBuilder str = new();
            if (maps is null || maps.Count == 0)
                str.AppendLine("Никто ещё не предложил карт для данной категории.");

            foreach (var map in maps)
                str.AppendLine(OfferedMapToString(map));

            return new DiscordEmbedBuilder()
                .WithTitle($"Предложка для категории {cat}")
                .WithDescription(str.ToString())
                .WithFooter($"{DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}")
                .Build();
        }

        public string RemoveMap(CompitCategory cat, int id)
        {
            if (mappoolProvider.CheckMapOffered(id, cat))
                mappoolProvider.MapRemove(cat, id);
            else
                return "Данной карты нет.";

            return "done";
        }

        public bool SpectatingEnabled() => spectatingEnabled;

        public void ResetMappool()
        {
            mappoolProvider.ResetMappool();
        }

        public async Task StartSpectating()
        {
            UpdateSpectateMessage(true);
        }

        public void StopSpectating()
        {
            throw new NotImplementedException();
        }

        public void UpdateSpectateMessage(bool sendNewMessages)
        {
            List<OfferedMap> beginnerMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beginner)
                                                        .OrderByDescending(x => x.Votes.Count)
                                                        .Take(3)
                                                        .ToList();

            List<OfferedMap> alphaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Alpha)
                                                              .OrderByDescending(x => x.Votes.Count)
                                                              .Take(3)
                                                              .ToList();

            List<OfferedMap> betaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beta)
                                                             .OrderByDescending(x => x.Votes.Count)
                                                             .Take(3)
                                                             .ToList();

            List<OfferedMap> gammaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Gamma)
                                                              .OrderByDescending(x => x.Votes.Count)
                                                              .Take(3)
                                                              .ToList();

            List<OfferedMap> deltaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Delta)
                                                              .OrderByDescending(x => x.Votes.Count)
                                                              .Take(3)
                                                              .ToList();

            List<OfferedMap> epsilonMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Epsilon)
                                                                .OrderByDescending(x => x.Votes.Count)
                                                                .Take(3)
                                                                .ToList();

            DiscordEmbed beginnerEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карт для Beginner")
                .WithDescription(string.Join('\n', beginnerMappoolTop.Select(x => OfferedMapToString(x))))
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithAuthor("Топ-3 карт для Alpha")
                .WithDescription(string.Join('\n', alphaMappoolTop.Select(x => OfferedMapToString(x))))
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithAuthor("Топ-3 карта для Beta")
                .WithDescription(string.Join('\n', betaMappoolTop.Select(x => OfferedMapToString(x))))
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithAuthor("Топ-3 карта для Gamma")
                .WithDescription(string.Join('\n', gammaMappoolTop.Select(x => OfferedMapToString(x))))
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithAuthor("Топ-3 карта для Delta")
                .WithDescription(string.Join('\n', deltaMappoolTop.Select(x => OfferedMapToString(x))))
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithAuthor("Топ-3 карта для Epsilon")
                .WithDescription(string.Join('\n', epsilonMappoolTop.Select(x => OfferedMapToString(x))))
                .Build();

            if (sendNewMessages)
            {
                beginnerMappoolAnnounceMsg = announceChannel.SendMessageAsync(beginnerEmbed).Result;
                alphaMappoolAnnounceMsg = announceChannel.SendMessageAsync(alphaEmbed).Result;
                betaMappoolAnnounceMsg = announceChannel.SendMessageAsync(betaEmbed).Result;
                gammaMappoolAnnounceMsg = announceChannel.SendMessageAsync(gammaEmbed).Result;
                deltaMappoolAnnounceMsg = announceChannel.SendMessageAsync(deltaEmbed).Result;
                epsilonMappoolAnnounceMsg = announceChannel.SendMessageAsync(epsilonEmbed).Result;
            }
            else
            {
                beginnerMappoolAnnounceMsg.ModifyAsync(beginnerEmbed);
                alphaMappoolAnnounceMsg.ModifyAsync(alphaEmbed);
                betaMappoolAnnounceMsg.ModifyAsync(betaEmbed);
                gammaMappoolAnnounceMsg.ModifyAsync(gammaEmbed);
                deltaMappoolAnnounceMsg.ModifyAsync(deltaEmbed);
                epsilonMappoolAnnounceMsg.ModifyAsync(epsilonEmbed);
            }
        }

        public void FinalizeMappoolSpectate(bool sendNewMessages)
        {
            List<OfferedMap> beginnerMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beginner)
                                                        .OrderByDescending(x => x.Votes.Count)
                                                        .Take(3)
                                                        .ToList();

            List<OfferedMap> alphaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Alpha)
                                                              .OrderByDescending(x => x.Votes.Count)
                                                              .Take(3)
                                                              .ToList();

            List<OfferedMap> betaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beta)
                                                             .OrderByDescending(x => x.Votes.Count)
                                                             .Take(3)
                                                             .ToList();

            List<OfferedMap> gammaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Gamma)
                                                              .OrderByDescending(x => x.Votes.Count)
                                                              .Take(3)
                                                              .ToList();

            List<OfferedMap> deltaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Delta)
                                                              .OrderByDescending(x => x.Votes.Count)
                                                              .Take(3)
                                                              .ToList();

            List<OfferedMap> epsilonMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Epsilon)
                                                                .OrderByDescending(x => x.Votes.Count)
                                                                .Take(3)
                                                                .ToList();

            Random rnd = new Random();

            DiscordEmbed beginnerEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Beginner")
                .WithDescription(OfferedMapToString(beginnerMappoolTop.ElementAt(rnd.Next(1, beginnerMappoolTop.Count))))
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Alpha")
                .WithDescription(OfferedMapToString(alphaMappoolTop.ElementAt(rnd.Next(1, alphaMappoolTop.Count))))
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Beta")
                .WithDescription(OfferedMapToString(betaMappoolTop.ElementAt(rnd.Next(1, betaMappoolTop.Count))))
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Gamma")
                .WithDescription(OfferedMapToString(gammaMappoolTop.ElementAt(rnd.Next(1, gammaMappoolTop.Count))))
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Delta")
                .WithDescription(OfferedMapToString(deltaMappoolTop.ElementAt(rnd.Next(1, deltaMappoolTop.Count))))
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Epsilon")
                .WithDescription(OfferedMapToString(epsilonMappoolTop.ElementAt(rnd.Next(1, epsilonMappoolTop.Count))))
                .Build();
        }

        public string OfferedMapToString(OfferedMap map)
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine($"`{map.BeatmapId}`: {emoji.RankStatusEmoji(map.Beatmap.ranked)} [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.title} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.beatmapset_id}#osu/{map.Beatmap.id})");
            str.AppendLine($"▸ {map.Beatmap.difficulty_rating}★ ▸**CS** {map.Beatmap.cs} ▸**HP**: {map.Beatmap.drain} ▸**AR**: {map.Beatmap.ar} ▸**OD**: {map.Beatmap.accuracy}");
            str.AppendLine($"Предложил: {(map.AdminMap ? "<@&708869211312619591>" : $"<@{map.SuggestedBy}>")}");
            str.AppendLine($"**__Проголосовало:__** {map.Votes.Count}\n");

            return str.ToString();
        }

        public string Vote(string memberId, int bmId)
        {
            var compitProfile = compitProvider.GetCompitProfile(memberId);

            if (compitProfile is null)
                return "Вы не зарегистрированы в конкурсе.";

            if (!mappoolProvider.CheckMapOffered(bmId, compitProfile.Category))
                return "Такой карты для данной категории нет.";

            if (mappoolProvider.CheckUserVoted(memberId))
                return "Вы уже голосовали.";

            mappoolProvider.MapVote(memberId, compitProfile.Category, bmId);

            return "done";
        }
    }
}
