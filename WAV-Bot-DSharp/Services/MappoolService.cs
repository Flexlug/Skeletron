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

using DSharpPlus.Entities;

using OsuParsers.Database.Objects;

namespace WAV_Bot_DSharp.Services
{
    public class MappoolService : IMappoolService
    {
        private IMappoolProvider mappoolProvider;
        private OsuRegex regex;

        private BanchoApi bapi;
        private GatariApi gapi;

        public MappoolService(IMappoolProvider mappoolProvider,
                              OsuRegex regex,
                              BanchoApi bapi,
                              GatariApi gapi)
        {
            this.mappoolProvider = mappoolProvider;

            this.regex = regex;

            this.bapi = bapi;
            this.gapi = gapi;
        }

        public string AddAdminMap(string category, string url)
        {
            throw new NotImplementedException();
        }

        public string AddMap(DiscordMember member, string category, string url)
        {
            throw new NotImplementedException();
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
                str.AppendLine($"{(map.AdminMap ? "<@&708869211312619591>" : $"<@{map.SuggestedBy}>")}: [{map.Beatmap.beatmapset.artist} - {map.Beatmap.beatmapset.artist} [{map.Beatmap.version}]](https://osu.ppy.sh/beatmapsets/{map.Beatmap.id}#osu/{map.Beatmap.id})");
                str.AppendLine($"▸ {map.Beatmap.difficulty_rating}★ ▸**CS** {map.Beatmap.cs} ▸**HP**: {map.Beatmap.drain} ▸**AR**: {map.Beatmap.ar} ▸**OD**: {map.Beatmap.accuracy}");
                str.AppendLine($"**__Voted:__** {map.Votes.Count}\n");
            }


        }

        public string RemoveMap(string category, string bmId)
        {
            throw new NotImplementedException();
        }

        public void ResetMappool()
        {
            throw new NotImplementedException();
        }

        public string Vote(DiscordMember member, string category, string url)
        {
            throw new NotImplementedException();
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
