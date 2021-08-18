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

using OsuParsers.Database.Objects;

namespace WAV_Bot_DSharp.Services
{
    public class MappoolService : IMappoolService
    {
        private IMappoolProvider mappoolProvider;
        private IWAVCompitProvider compitProvider;
        private IWAVMembersProvider members;

        private OsuRegex regex;

        private BanchoApi bapi;
        private GatariApi gapi;

        public MappoolService(IMappoolProvider mappoolProvider,
                              IWAVCompitProvider compitProvider,
                              IWAVMembersProvider membersProvider,
                              OsuRegex regex,
                              BanchoApi bapi,
                              GatariApi gapi)
        {
            this.mappoolProvider = mappoolProvider;

            this.regex = regex;

            this.members = membersProvider;
            this.compitProvider = compitProvider;

            this.bapi = bapi;
            this.gapi = gapi;
        }

        public string AddAdminMap(string category, string url)
        {
            var cat = StrToCompitCategory(category);

            if (cat is null)
                return $"Неизвестная категория: {category}";

            var ids = regex.GetBMandBMSIdFromBanchoUrl(url);
            var bBeatmap = bapi.GetBeatmap(ids.Item2);
            if (bBeatmap is null)
                return $"Не удалось получить карту с id {ids.Item2}";

            mappoolProvider.MapAdd(new()
            {
                AdminMap = true,
                Beatmap = bBeatmap,
                BeatmapId = ids.Item2,
                Category = (CompitCategories)cat,
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
                AdminMap = true,
                Beatmap = bBeatmap,
                BeatmapId = ids.Item2,
                Category = (CompitCategories)cat,
                SuggestedBy = memberId,
                Votes = new() { memberId }
            });

            return "done";
        }

        public DiscordEmbed GetCategoryMappool(string category)
        {
            var cat = StrToCompitCategory(category);

            if (cat is null)
                return new DiscordEmbedBuilder()
                    .WithTitle("Ошибка")
                    .WithDescription($"Неизвестная категория: {category}.")
                    .Build();

            var maps = mappoolProvider.GetCategoryMaps((CompitCategories)cat);

            StringBuilder str = new();
            if (maps is null || maps.Count == 0)
                str.AppendLine("Никто ещё не предложил карт для данной категории.");

            foreach(var map in maps)
            {
                str.AppendLine($"`{map.BeatmapId}`: [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.artist} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.id}#osu/{map.Beatmap.id})");
                str.AppendLine($"▸ {map.Beatmap.difficulty_rating}★ ▸**CS** {map.Beatmap.cs} ▸**HP**: {map.Beatmap.drain} ▸**AR**: {map.Beatmap.ar} ▸**OD**: {map.Beatmap.accuracy}");
                str.AppendLine($"Предложил: {(map.AdminMap ? "<@&708869211312619591>" : $"<@{map.SuggestedBy}>")}");
                str.AppendLine($"**__Проголосовало:__** {map.Votes.Count}\n");
            }

            return new DiscordEmbedBuilder()
                .WithTitle($"Предложка для категории {category}")
                .WithDescription(str.ToString())
                .WithFooter(DateTime.Now.ToLongTimeString())
                .Build();
        }

        public DiscordEmbed GetCategoryMappool(DiscordMember user)
        {
            var compitProfile = compitProvider.GetCompitProfile(user.Id.ToString());

            if (compitProvider is null)
                return new DiscordEmbedBuilder()
                    .WithTitle("Ошибка")
                    .WithDescription("Вы не зарегистрированы в конкурсе.");


            var cat = compitProfile.Category;

            var maps = mappoolProvider.GetCategoryMaps(cat);

            StringBuilder str = new();
            if (maps is null || maps.Count == 0)
                str.AppendLine("Никто ещё не предложил карт для данной категории.");

            foreach (var map in maps)
            {
                str.AppendLine($"`{map.BeatmapId}`: [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.artist} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.id}#osu/{map.Beatmap.id})");
                str.AppendLine($"▸ {map.Beatmap.difficulty_rating}★ ▸**CS** {map.Beatmap.cs} ▸**HP**: {map.Beatmap.drain} ▸**AR**: {map.Beatmap.ar} ▸**OD**: {map.Beatmap.accuracy}");
                str.AppendLine($"Предложил: {(map.AdminMap ? "<@&708869211312619591>" : $"<@{map.SuggestedBy}>")}");
                str.AppendLine($"**__Проголосовало:__** {map.Votes.Count}\n");
            }

            return new DiscordEmbedBuilder()
                .WithTitle($"Предложка для категории {cat}")
                .WithDescription(str.ToString())
                .WithFooter(DateTime.Now.ToLongTimeString())
                .Build();
        }

        public string RemoveMap(string category, string bmId)
        {
            var cat = StrToCompitCategory(category);

            if (cat is null)
                return $"Неизвестная категория: {category}.";

            int id;
            if (!int.TryParse(bmId, out id))
                return "Введенный id не является числом.";

            if (mappoolProvider.CheckMapOffered(id, (CompitCategories)cat))
                mappoolProvider.MapRemove((CompitCategories)cat, id);
            else
                return "Данной карты нет.";

            return "done";
        }

        public void ResetMappool()
        {
            mappoolProvider.ResetMappool();
        }

        public string Vote(string memberId, string bmId)
        {
            var compitProfile = compitProvider.GetCompitProfile(memberId);

            if (compitProvider is null)
                return "Вы не зарегистрированы в конкурсе.";

            int id;
            if (!int.TryParse(bmId, out id))
                return "Введенный id не является числом.";

            if (!mappoolProvider.CheckMapOffered(id, compitProfile.Category))
                return "Такой карты для данной категории нет.";

            if (mappoolProvider.CheckUserVoted(memberId))
                return "Вы уже голосовали.";

            mappoolProvider.MapVote(memberId, compitProfile.Category, id);

            return "done";
        }

        private CompitCategories? StrToCompitCategory(string category)
        {
            switch (category)
            {
                case "beginner":
                    return CompitCategories.Beginner;

                case "alpha":
                    return CompitCategories.Alpha;

                case "beta":
                    return CompitCategories.Beta;

                case "gamma":
                    return CompitCategories.Gamma;

                case "delta":
                    return CompitCategories.Delta;

                case "epsilon":
                    return CompitCategories.Epsilon;

                default:
                    return null;
            }
        }
    }
}
