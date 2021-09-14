using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skeletron.Database.Interfaces;
using Skeletron.Database.Models;
using Skeletron.Services.Interfaces;
using Skeletron.Converters;

using OsuNET_Api;
using OsuNET_Api.Models.Bancho;
using OsuNET_Api.Models.Gatari.Enums;

using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

using OsuParsers.Database.Objects;
using DSharpPlus;

namespace Skeletron.Services
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

        private OsuRegex osuRegex;
        private OsuEmoji osuEmoji;

        private BanchoApi bapi;
        private GatariApi gapi;

        private EmojiUtlis emoji;

        public MappoolService(ILogger<MappoolService> logger,
                              IMappoolProvider mappoolProvider,
                              ICompitProvider compitProvider,
                              ICompititionService compitService,
                              DiscordClient client,
                              OsuRegex osuRegex,
                              OsuEmoji osuEmoji,
                              BanchoApi bapi,
                              GatariApi gapi,
                              EmojiUtlis emoji)
        {
            this.logger = logger;

            this.client = client;
            client.MessageReactionAdded += OnReaction;

            this.mappoolProvider = mappoolProvider;
            this.compitService = compitService;
            this.compitProvider = compitProvider;

            this.osuRegex = osuRegex;
            this.osuEmoji = osuEmoji;

            this.bapi = bapi;
            this.gapi = gapi;

            this.emoji = emoji;

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
            var ids = osuRegex.GetBMandBMSIdFromBanchoUrl(url);
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
                AdditionDate = DateTime.Now,
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

            if (mappoolProvider.MapsCount(cat) >= 9)
                return "Превышен лимит на количество предложенных карт.";

            var ids = osuRegex.GetBMandBMSIdFromBanchoUrl(url);
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
                AdditionDate = DateTime.Now,
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

        public async Task<string> UpdateCategoryMappoolStatus(CompitCategory cat)
        {
            try
            {
                if (!spectateStatus.IsSpectating)
                    return "Отслеживание отключено";

                await UpdateCategoryMessage(cat).ConfigureAwait(false);
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

            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendLine($"`{i + 1}` : {OfferedMapToShortString(list[i])}");
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
                .OrderBy(x => x.AdditionDate)
                .ToList();

            DiscordEmbed categoryEmbed = new DiscordEmbedBuilder()
                .WithTitle($"Предложенные карты для {category}")
                .WithDescription(await OfferedMapsToString(categoryTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            switch (category)
            { 
                case CompitCategory.Beginner:
                    beginnerMappoolAnnounceMsg = await beginnerMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    await GenerateDigitEmojis(beginnerMappoolAnnounceMsg, categoryTop.Count);
                    break;

                case CompitCategory.Alpha:
                    alphaMappoolAnnounceMsg = await alphaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    await GenerateDigitEmojis(alphaMappoolAnnounceMsg, categoryTop.Count);
                    break;

                case CompitCategory.Beta:
                    betaMappoolAnnounceMsg = await betaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    await GenerateDigitEmojis(betaMappoolAnnounceMsg, categoryTop.Count);
                    break;

                case CompitCategory.Gamma:
                    gammaMappoolAnnounceMsg = await gammaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    await GenerateDigitEmojis(gammaMappoolAnnounceMsg, categoryTop.Count);
                    break;

                case CompitCategory.Delta:
                    deltaMappoolAnnounceMsg = await deltaMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    await GenerateDigitEmojis(deltaMappoolAnnounceMsg, categoryTop.Count);
                    break;

                case CompitCategory.Epsilon:
                    epsilonMappoolAnnounceMsg = await epsilonMappoolAnnounceMsg.ModifyAsync(categoryEmbed);
                    await GenerateDigitEmojis(epsilonMappoolAnnounceMsg, categoryTop.Count);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public async Task UpdateAllSpectateMessages(bool sendNewMessages)
        {
            List<OfferedMap> beginnerMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beginner)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            List<OfferedMap> alphaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Alpha)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            List<OfferedMap> betaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Beta)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            List<OfferedMap> gammaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Gamma)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            List<OfferedMap> deltaMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Delta)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            List<OfferedMap> epsilonMappoolTop = mappoolProvider.GetCategoryMaps(CompitCategory.Epsilon)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            DiscordEmbed beginnerEmbed = new DiscordEmbedBuilder()
                .WithTitle("Предложенные карты для Beginner")
                .WithDescription(await OfferedMapsToString(beginnerMappoolTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Предложенные карты для Alpha")
                .WithDescription(await OfferedMapsToString(alphaMappoolTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Предложенные карты для Beta")
                .WithDescription(await OfferedMapsToString(betaMappoolTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Предложенные карты для Gamma")
                .WithDescription(await OfferedMapsToString(gammaMappoolTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Предложенные карты для Delta")
                .WithDescription(await OfferedMapsToString(deltaMappoolTop))
                .WithFooter($"Время последнего обновления маппула: {DateTime.Now}")
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithTitle("Предложенные карты для Epsilon")
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

                await GenerateDigitEmojis(beginnerMappoolAnnounceMsg, beginnerMappoolTop.Count);
                await GenerateDigitEmojis(alphaMappoolAnnounceMsg, alphaMappoolTop.Count);
                await GenerateDigitEmojis(betaMappoolAnnounceMsg, betaMappoolTop.Count);
                await GenerateDigitEmojis(gammaMappoolAnnounceMsg, gammaMappoolTop.Count);
                await GenerateDigitEmojis(deltaMappoolAnnounceMsg, deltaMappoolTop.Count);
                await GenerateDigitEmojis(epsilonMappoolAnnounceMsg, epsilonMappoolTop.Count);

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
                beginnerMappoolAnnounceMsg = await beginnerMappoolAnnounceMsg.ModifyAsync(beginnerEmbed);
                await GenerateDigitEmojis(beginnerMappoolAnnounceMsg, beginnerMappoolTop.Count);

                alphaMappoolAnnounceMsg = await alphaMappoolAnnounceMsg.ModifyAsync(alphaEmbed);
                await GenerateDigitEmojis(alphaMappoolAnnounceMsg, alphaMappoolTop.Count);

                betaMappoolAnnounceMsg = await betaMappoolAnnounceMsg.ModifyAsync(betaEmbed);
                await GenerateDigitEmojis(betaMappoolAnnounceMsg, betaMappoolTop.Count);

                gammaMappoolAnnounceMsg = await gammaMappoolAnnounceMsg.ModifyAsync(gammaEmbed);
                await GenerateDigitEmojis(gammaMappoolAnnounceMsg, gammaMappoolTop.Count);

                deltaMappoolAnnounceMsg = await deltaMappoolAnnounceMsg.ModifyAsync(deltaEmbed);
                await GenerateDigitEmojis(deltaMappoolAnnounceMsg, deltaMappoolTop.Count);

                epsilonMappoolAnnounceMsg = await epsilonMappoolAnnounceMsg.ModifyAsync(epsilonEmbed);
                await GenerateDigitEmojis(epsilonMappoolAnnounceMsg, epsilonMappoolTop.Count);
            }
        }

        private async Task GenerateDigitEmojis(DiscordMessage msg, int count)
        {
            for (int i = msg.Reactions.Count; i < count; i++)
                await msg.CreateReactionAsync(emoji.DigitToEmoji(i + 1));

            if (msg.Reactions.Count > count)
                for (int i = msg.Reactions.Count; i > count; i--)
                    await msg.DeleteReactionAsync(emoji.DigitToEmoji(i), msg.Author);
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
                .WithImageUrl(beginnerChosenMap.Beatmap.beatmapset.covers.Card2x)
                .WithDescription(OfferedMapToLongString(beginnerChosenMap))
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Alpha")
                .WithImageUrl(alphaChosenMap.Beatmap.beatmapset.covers.Card2x)
                .WithDescription(OfferedMapToLongString(alphaChosenMap))
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Beta")
                .WithImageUrl(betaChosenMap.Beatmap.beatmapset.covers.Card2x)
                .WithDescription(OfferedMapToLongString(betaChosenMap))
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Gamma")
                .WithImageUrl(gammaChosenMap.Beatmap.beatmapset.covers.Card2x)
                .WithDescription(OfferedMapToLongString(gammaChosenMap))
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Delta")
                .WithImageUrl(deltaChosenMap.Beatmap.beatmapset.covers.Card2x)
                .WithDescription(OfferedMapToLongString(deltaChosenMap))
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Epsilon")
                .WithImageUrl(epsilonChosenMap.Beatmap.beatmapset.covers.Card2x)
                .WithDescription(OfferedMapToLongString(epsilonChosenMap))
                .Build();

            beginnerMappoolAnnounceMsg = await beginnerMappoolAnnounceMsg.ModifyAsync(beginnerEmbed);
            await beginnerMappoolAnnounceMsg.DeleteAllReactionsAsync();

            alphaMappoolAnnounceMsg = await alphaMappoolAnnounceMsg.ModifyAsync(alphaEmbed);
            await alphaMappoolAnnounceMsg.DeleteAllReactionsAsync();

            betaMappoolAnnounceMsg = await betaMappoolAnnounceMsg.ModifyAsync(betaEmbed);
            await betaMappoolAnnounceMsg.DeleteAllReactionsAsync();

            gammaMappoolAnnounceMsg = await gammaMappoolAnnounceMsg.ModifyAsync(gammaEmbed);
            await gammaMappoolAnnounceMsg.DeleteAllReactionsAsync();

            deltaMappoolAnnounceMsg = await deltaMappoolAnnounceMsg.ModifyAsync(deltaEmbed);
            await deltaMappoolAnnounceMsg.DeleteAllReactionsAsync();

            epsilonMappoolAnnounceMsg = await epsilonMappoolAnnounceMsg.ModifyAsync(epsilonEmbed);
            await epsilonMappoolAnnounceMsg.DeleteAllReactionsAsync();

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

            str.AppendLine($"{osuEmoji.RankStatusEmoji(map.Beatmap.ranked)} [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.title} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.beatmapset_id}#osu/{map.Beatmap.id})");
            str.AppendLine($"▸ {map.Beatmap.difficulty_rating}★ ▸**CS** {map.Beatmap.cs} ▸**HP**: {map.Beatmap.drain} ▸**AR**: {map.Beatmap.ar} ▸**OD**: {map.Beatmap.accuracy}");
            str.Append($"Предложил: {(map.AdminMap ? "<@&708869211312619591>" : $"<@{map.SuggestedBy}>")} ");
            str.AppendLine($"Проголосовало: {map.Votes.Count}");

            return str.ToString();
        }

        public string OfferedMapToShortString(OfferedMap map) =>
            $"{osuEmoji.RankStatusEmoji(map.Beatmap.ranked)} [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.title} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.beatmapset_id}#osu/{map.Beatmap.id}) : {map.Votes.Count}";

        private async Task OnReaction(DiscordClient sender, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            CompitCategory? cat = null;

            if (e.User.Username == "Skeletron")
                return;

            if (e.Message.Id == beginnerMappoolAnnounceMsg.Id)
                cat = CompitCategory.Beginner;

            if (e.Message.Id == alphaMappoolAnnounceMsg.Id)
                cat = CompitCategory.Alpha;

            if (e.Message.Id == betaMappoolAnnounceMsg.Id)
                cat = CompitCategory.Beta;

            if (e.Message.Id == gammaMappoolAnnounceMsg.Id)
                cat = CompitCategory.Gamma;

            if (e.Message.Id == deltaMappoolAnnounceMsg.Id)
                cat = CompitCategory.Delta;

            if (e.Message.Id == epsilonMappoolAnnounceMsg.Id)
                cat = CompitCategory.Epsilon;

            if (cat is null)
                return;

            logger.LogDebug($"Detected vote attempt – {e.User.Username} – {cat}");
            await Vote(e.User, e.Emoji, (CompitCategory)cat);

            await e.Message.DeleteReactionAsync(e.Emoji, e.User);
        }

        public async Task Vote(DiscordUser user, DiscordEmoji voteEmoji, CompitCategory cat)
        {
            var compitProfile = compitProvider.GetCompitProfile(user.Id.ToString());

            if (compitProfile is null)
            {
                logger.LogDebug($"Голосующий не зарегистрирован в конкурсе – {user.Username}");
                return;
            }

            if (compitProfile.Category != cat)
            {
                logger.LogDebug($"Категория голосующего не соответствует категории карты. {user.Username}. {compitProfile.Category} – {cat}");
                return;
            }

            if (mappoolProvider.CheckUserVoted(user.Id.ToString()))
            {
                logger.LogDebug($"Человек уже проголосовал или предложил карту - {user.Username}");
                return;
            }

            int index = 0;
            try
            {
                index = emoji.EmojiToDigit(voteEmoji);
            }
            catch(ArgumentOutOfRangeException ex)
            {
                logger.LogDebug($"Голосующий использовал недопустимый эмодзи - {user.Username} – {voteEmoji.Name}");
                return;
            }
            catch(Exception e)
            {
                logger.LogError($"Неожиданное исключение: {e.Message}\n{e.StackTrace}");
                return;
            }

            List<OfferedMap> maps = mappoolProvider
                .GetCategoryMaps(cat)
                .OrderBy(x => x.AdditionDate)
                .ToList();

            if (index == 0 || index > maps.Count)
            {
                logger.LogError($"Индекс, соответствующий эмодзи голосующего, выходит за границы массива – {user.Username} – {index}");
                return;
            }

            mappoolProvider.MapVote(user.Id.ToString(), cat, maps.ElementAt(index - 1).BeatmapId);

            logger.LogError($"Отдан голос – {user.Username} – bmId: {maps.ElementAt(index - 1).BeatmapId}, index: {index}");

            await UpdateCategoryMappoolStatus(cat);
        }
    }
}
