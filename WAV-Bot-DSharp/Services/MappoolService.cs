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

            CheckMappoolSpectateStatus().RunSynchronously();
        }

        private async Task CheckMappoolSpectateStatus()
        {
            spectateStatus = mappoolProvider.GetMappoolStatus();

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

        public async Task<string> StartSpectating()
        {
            try
            {
                await UpdateSpectateMessage(true).ConfigureAwait(false);

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
                FinalizeMappoolSpectate();
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
                await UpdateSpectateMessage(false).ConfigureAwait(false);

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
                announceChannel = await client.GetChannelAsync(channel_id).ConfigureAwait(false);

                spectateStatus.AnnounceChannelId = announceChannel.Id.ToString();
                mappoolProvider.SetMappoolStatus(spectateStatus);
            }
            catch (Exception e)
            {
                return $"{e.Message} : {e.StackTrace}";
            }

            return "done";
        }

        public async Task UpdateSpectateMessage(bool sendNewMessages)
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
                return topVoted.ElementAt(rnd.Next(1, topVoted.Count));
        }

        public async void FinalizeMappoolSpectate()
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
                .WithDescription(OfferedMapToString(beginnerChosenMap))
                .Build();

            DiscordEmbed alphaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Alpha")
                .WithDescription(OfferedMapToString(alphaChosenMap))
                .Build();

            DiscordEmbed betaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Beta")
                .WithDescription(OfferedMapToString(betaChosenMap))
                .Build();

            DiscordEmbed gammaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Gamma")
                .WithDescription(OfferedMapToString(gammaChosenMap))
                .Build();

            DiscordEmbed deltaEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Delta")
                .WithDescription(OfferedMapToString(deltaChosenMap))
                .Build();

            DiscordEmbed epsilonEmbed = new DiscordEmbedBuilder()
                .WithTitle("Выбранная карта для Epsilon")
                .WithDescription(OfferedMapToString(epsilonChosenMap))
                .Build();

            await beginnerMappoolAnnounceMsg.ModifyAsync(beginnerEmbed);
            await alphaMappoolAnnounceMsg.ModifyAsync(alphaEmbed);
            await betaMappoolAnnounceMsg.ModifyAsync(betaEmbed);
            await gammaMappoolAnnounceMsg.ModifyAsync(gammaEmbed);
            await deltaMappoolAnnounceMsg.ModifyAsync(deltaEmbed);
            await epsilonMappoolAnnounceMsg.ModifyAsync(epsilonEmbed);

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
