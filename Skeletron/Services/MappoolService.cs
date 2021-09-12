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
        private ICompititionService compitService;
        private ICompitProvider compitProvider;

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
                              ICompitProvider compitProvider,
                              ICompititionService compitService,
                              DiscordClient client,
                              OsuRegex regex,
                              OsuEmoji emoji,
                              BanchoApi bapi,
                              GatariApi gapi)
        {
            this.logger = logger;

            this.client = client;

            this.mappoolProvider = mappoolProvider;
            this.compitService = compitService;
            this.compitProvider = compitProvider;

            this.regex = regex;
            this.emoji = emoji;

            this.bapi = bapi;
            this.gapi = gapi;

            this.spectateStatus = mappoolProvider.GetMappoolStatus();

            debugChannel = client.GetChannelAsync(823835078298828841).Result;

            CheckMappoolSpectateStatus().Wait();
        }

        private async Task CheckMappoolSpectateStatus()
        {
            // Если отслеживание не запущено, то вырубаем проверку
            if (!spectateStatus.IsSpectating)
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
                announceChannel = await client.GetChannelAsync(spectateId).ConfigureAwait(false);
                beginnerMappoolAnnounceMsg = await announceChannel.GetMessageAsync(beginnerId).ConfigureAwait(false);
                alphaMappoolAnnounceMsg = await announceChannel.GetMessageAsync(alphaId).ConfigureAwait(false);
                betaMappoolAnnounceMsg = await announceChannel.GetMessageAsync(betaId).ConfigureAwait(false);
                gammaMappoolAnnounceMsg = await announceChannel.GetMessageAsync(gammaId).ConfigureAwait(false);
                deltaMappoolAnnounceMsg = await announceChannel.GetMessageAsync(deltaId).ConfigureAwait(false);
                epsilonMappoolAnnounceMsg = await announceChannel.GetMessageAsync(epsilonId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await debugChannel.SendMessageAsync($"Ошибка при запуске отслеживания маппула: {ex.InnerException}\n{ex.Message}").ConfigureAwait(false);
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

            UpdateCategoryMessage(cat).Wait();

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

            UpdateCategoryMessage(cat).Wait();

            return "done";
        }

        public DiscordEmbed GetCategoryMappool(CompitCategory cat)
        {
            var maps = mappoolProvider.GetCategoryMaps(cat);

            StringBuilder str = new();
            if (maps is null || maps.Count == 0)
                str.AppendLine("Никто ещё не предложил карт для данной категории.");

            foreach (var map in maps)
                str.AppendLine(OfferedMapToLongString(map));

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
                str.AppendLine(OfferedMapToLongString(map));

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

            UpdateCategoryMessage(cat).Wait();

            return "done";
        }

        public bool SpectatingEnabled() => spectatingEnabled;

        public void ResetMappool()
        {
            mappoolProvider.ResetMappool();
        }

        public async Task<string> StartSpectating()
        {
            try
            {
                if (string.IsNullOrEmpty(spectateStatus.AnnounceChannelId))
                    return "Канал для маппула не задан";

                announceChannel = await client.GetChannelAsync(ulong.Parse(spectateStatus.AnnounceChannelId));

                if (spectateStatus.IsSpectating)
                    return "Отслеживание маппула уже включено";

                await UpdateAllSpectateMessages(true).ConfigureAwait(false);

                spectateStatus.IsSpectating = true;
                mappoolProvider.SetMappoolStatus(spectateStatus);
            }
            catch(Exception e)
            {
                return $"{e.Message} : {e.StackTrace}";
            }

            return "done";
        }

        public async Task<string> StopSpectating()
        {
            try
            {
                if (!spectateStatus.IsSpectating)
                    return "Отслеживание уже отключено";

                await FinalizeMappoolSpectate().ConfigureAwait(false);

                spectateStatus.IsSpectating = false;
                mappoolProvider.SetMappoolStatus(spectateStatus);
            }
            catch (Exception e)
            {
                return $"{e.Message} : {e.StackTrace}";
            }

            return "done";
        }

        public async Task<string> UpdateMappoolStatus()
        {
            try
            {
                if (!spectateStatus.IsSpectating)
                    return "Отслеживание отключено";

                await UpdateAllSpectateMessages(false).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return $"{e.Message} : {e.StackTrace}";
            }

            return "done";
        }

        public async Task<string> HaltSpectating()
        {
            try
            {
                spectateStatus.IsSpectating = false;
                mappoolProvider.SetMappoolStatus(spectateStatus);
            }
            catch (Exception e)
            {
                return $"{e.Message} : {e.StackTrace}";
            }

            return "done";
        }

        public async Task<string> SetAnnounceChannel(ulong channel_id)
        {
            try
            {
                announceChannel = await client.GetChannelAsync(channel_id);

                spectateStatus.AnnounceChannelId = announceChannel.Id.ToString();
                mappoolProvider.SetMappoolStatus(spectateStatus);
            }
            catch (Exception e)
            {
                return $"{e.Message} : {e.StackTrace}";
            }

            return "done";
        }

        public async Task<string> OfferedMapsToString(List<OfferedMap> list)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var map in list)
            {
                sb.AppendLine(OfferedMapToShortString(map));
            }

            //if (list.Count != 3) {
            //    for (int i = list.Count; i < 3; i++)
            //    {
            //        sb.AppendLine("-");
            //        sb.AppendLine("-");
            //        sb.AppendLine("-");
            //        sb.AppendLine();
            //    }
            //}

            return sb.ToString();
        }

        public async Task UpdateCategoryMessage(CompitCategory category)
        {
            List<OfferedMap> categoryTop = mappoolProvider.GetCategoryMaps(category)
                .OrderByDescending(x => x.Votes.Count)
                .Take(3)
                .ToList();

            DiscordEmbed categoryEmbed = new DiscordEmbedBuilder()
                .WithTitle($"Топ-3 карт для {category}")
                .WithDescription(await OfferedMapsToString(categoryTop))
                .Build();

            switch (category)
            {
                case CompitCategory.Beginner:
                    await beginnerMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    break;

                case CompitCategory.Alpha:
                    await alphaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    break;

                case CompitCategory.Beta:
                    await betaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    break;

                case CompitCategory.Gamma:
                    await gammaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    break;

                case CompitCategory.Delta:
                    await deltaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    break;

                case CompitCategory.Epsilon:
                    await epsilonMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    break;

                default:
                    throw new NotImplementedException();
            }


        }

        public async Task UpdateAllSpectateMessages(bool sendNewMessages)
        {
            List<OfferedMap> beginnerMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beginner)
                .OrderByDescending(x => x.Votes.Count)
                .ToList();

            List<OfferedMap> alphaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Alpha)
                .OrderByDescending(x => x.Votes.Count)
                .ToList();

            List<OfferedMap> betaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beta)
                .OrderByDescending(x => x.Votes.Count)
                .ToList();

            List<OfferedMap> gammaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Gamma)
                .OrderByDescending(x => x.Votes.Count)
                .ToList();

            List<OfferedMap> deltaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Delta)
                .OrderByDescending(x => x.Votes.Count)
                .Take(3)
                .ToList();

            List<OfferedMap> epsilonMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Epsilon)
                .OrderByDescending(x => x.Votes.Count)
                .ToList();

            DiscordEmbed beginnerEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карт для Beginner")
                .WithDescription(await OfferedMapsToString(beginnerMappoolTop))
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карт для Alpha")
                .WithDescription(await OfferedMapsToString(alphaMappoolTop))
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карта для Beta")
                .WithDescription(await OfferedMapsToString(betaMappoolTop))
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карта для Gamma")
                .WithDescription(await OfferedMapsToString(gammaMappoolTop))
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карта для Delta")
                .WithDescription(await OfferedMapsToString(deltaMappoolTop))
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithTitle("Топ-3 карта для Epsilon")
                .WithDescription(await OfferedMapsToString(epsilonMappoolTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            if (sendNewMessages)
            {
                beginnerMappoolAnnounceMsg = await announceChannel.SendMessageAsync(beginnerEmbed).ConfigureAwait(false);
                alphaMappoolAnnounceMsg = await announceChannel.SendMessageAsync(alphaEmbed).ConfigureAwait(false);
                betaMappoolAnnounceMsg = await announceChannel.SendMessageAsync(betaEmbed).ConfigureAwait(false);
                gammaMappoolAnnounceMsg = await announceChannel.SendMessageAsync(gammaEmbed).ConfigureAwait(false);
                deltaMappoolAnnounceMsg = await announceChannel.SendMessageAsync(deltaEmbed).ConfigureAwait(false);
                epsilonMappoolAnnounceMsg = await announceChannel.SendMessageAsync(epsilonEmbed).ConfigureAwait(false);

                spectateStatus.BeginnerMessageId = beginnerMappoolAnnounceMsg.Id.ToString();
                spectateStatus.AlphaMessageId = alphaMappoolAnnounceMsg.Id.ToString();
                spectateStatus.BetaMessageId = betaMappoolAnnounceMsg.Id.ToString();
                spectateStatus.GammaMessageId = gammaMappoolAnnounceMsg.Id.ToString();
                spectateStatus.DeltaMessageId = deltaMappoolAnnounceMsg.Id.ToString();
                spectateStatus.EpsilonMessageId = epsilonMappoolAnnounceMsg.Id.ToString();

                mappoolProvider.SetMappoolStatus(spectateStatus);
            }
            else
            {
                await beginnerMappoolAnnounceMsg.ModifyAsync(beginnerEmbed);
                await alphaMappoolAnnounceMsg.ModifyAsync(alphaEmbed);
                await betaMappoolAnnounceMsg.ModifyAsync(betaEmbed);
                await gammaMappoolAnnounceMsg.ModifyAsync(gammaEmbed);
                await deltaMappoolAnnounceMsg.ModifyAsync(deltaEmbed);
                await epsilonMappoolAnnounceMsg.ModifyAsync(epsilonEmbed);
            }
        }

        private async Task<OfferedMap> ChooseMap(List<OfferedMap> maps)
        {
            int maxVotes = maps.Max(x => x.Votes.Count);
            Random rnd = new Random();

            var topVoted = maps.FindAll(x => x.Votes.Count == maxVotes);
            if (topVoted.Count == 1)
                return topVoted.First();
            else
                return topVoted.ElementAt(rnd.Next(0, topVoted.Count - 1));
        }

        public async Task FinalizeMappoolSpectate()
        {
            Random rnd = new Random();

            OfferedMap beginnerChosenMap = await ChooseMap(mappoolProvider.GetCategoryMaps(CompitCategory.Beginner)),
                       alphaChosenMap = await ChooseMap(mappoolProvider.GetCategoryMaps(CompitCategory.Alpha)),
                       betaChosenMap = await ChooseMap(mappoolProvider.GetCategoryMaps(CompitCategory.Beta)),
                       gammaChosenMap = await ChooseMap(mappoolProvider.GetCategoryMaps(CompitCategory.Gamma)),
                       deltaChosenMap = await ChooseMap(mappoolProvider.GetCategoryMaps(CompitCategory.Delta)),
                       epsilonChosenMap = await ChooseMap(mappoolProvider.GetCategoryMaps(CompitCategory.Epsilon));

            DiscordEmbed beginnerEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Beginner")
                .WithDescription(OfferedMapToLongString(beginnerChosenMap))
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Alpha")
                .WithDescription(OfferedMapToLongString(alphaChosenMap))
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Beta")
                .WithDescription(OfferedMapToLongString(betaChosenMap))
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Gamma")
                .WithDescription(OfferedMapToLongString(gammaChosenMap))
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Delta")
                .WithDescription(OfferedMapToLongString(deltaChosenMap))
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Epsilon")
                .WithDescription(OfferedMapToLongString(epsilonChosenMap))
                .Build();

            await beginnerMappoolAnnounceMsg.ModifyAsync(beginnerEmbed);
            await alphaMappoolAnnounceMsg.ModifyAsync(alphaEmbed);
            await betaMappoolAnnounceMsg.ModifyAsync(betaEmbed);
            await gammaMappoolAnnounceMsg.ModifyAsync(gammaEmbed);
            await deltaMappoolAnnounceMsg.ModifyAsync(deltaEmbed);
            await epsilonMappoolAnnounceMsg.ModifyAsync(epsilonEmbed);

            await compitService.SetMap($@"https://osu.ppy.sh/beatmapsets/{beginnerChosenMap.Beatmap.beatmapset_id}#osu/{beginnerChosenMap.Beatmap.id}", "beginner");
            await compitService.SetMap($@"https://osu.ppy.sh/beatmapsets/{alphaChosenMap.Beatmap.beatmapset_id}#osu/{alphaChosenMap.Beatmap.id}", "alpha");
            await compitService.SetMap($@"https://osu.ppy.sh/beatmapsets/{betaChosenMap.Beatmap.beatmapset_id}#osu/{betaChosenMap.Beatmap.id}", "beta");
            await compitService.SetMap($@"https://osu.ppy.sh/beatmapsets/{gammaChosenMap.Beatmap.beatmapset_id}#osu/{gammaChosenMap.Beatmap.id}", "gamma");
            await compitService.SetMap($@"https://osu.ppy.sh/beatmapsets/{deltaChosenMap.Beatmap.beatmapset_id}#osu/{deltaChosenMap.Beatmap.id}", "delta");
            await compitService.SetMap($@"https://osu.ppy.sh/beatmapsets/{epsilonChosenMap.Beatmap.beatmapset_id}#osu/{epsilonChosenMap.Beatmap.id}", "epsilon");
        }

        public string OfferedMapToLongString(OfferedMap map)
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine($"`{map.Beatmap.id}` {emoji.RankStatusEmoji(map.Beatmap.ranked)} [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.title} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.beatmapset_id}#osu/{map.Beatmap.id})");
            str.AppendLine($"▸ {map.Beatmap.difficulty_rating}★ ▸**CS** {map.Beatmap.cs} ▸**HP**: {map.Beatmap.drain} ▸**AR**: {map.Beatmap.ar} ▸**OD**: {map.Beatmap.accuracy}");
            str.Append($"Предложил: {(map.AdminMap ? "<@&708869211312619591>" : $"<@{map.SuggestedBy}>")} ");
            str.AppendLine($"Проголосовало: {map.Votes.Count}");

            return str.ToString();
        }

        public string OfferedMapToShortString(OfferedMap map) =>
            $"`{map.Beatmap.id}` {emoji.RankStatusEmoji(map.Beatmap.ranked)} [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.title} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.beatmapset_id}#osu/{map.Beatmap.id}) : {map.Votes.Count}";

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
