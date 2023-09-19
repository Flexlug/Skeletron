using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.Logging;

using Skeletron.Converters;

using OsuNET_Api;
using OsuNET_Api.Models.Bancho;
using OsuNET_Api.Models.Gatari;
using Skeletron.Services.Interfaces;

namespace Skeletron.Commands
{
    public class OsuCommands : SkBaseCommandModule
    {
        private ILogger<OsuCommands> logger;

        private OsuEmbed osuEmbeds;
        private OsuEnums osuEnums;

        private BanchoApi api;
        private GatariApi gapi;

        public OsuCommands(ILogger<OsuCommands> logger,
                           DiscordClient client, 
                           OsuEmbed osuEmbeds,
                           OsuEnums osuEnums,
                           BanchoApi api,
                           GatariApi gapi,
                           IOsuService osuService)
                           //IMembersProvider wavMembers,
                           //ICompitProvider wavProvider,
        {
            ModuleName = "osu!";

            this.logger = logger;

            this.osuEmbeds = osuEmbeds;
            this.osuEnums = osuEnums;

            this.api = api;
            this.gapi = gapi;

            logger.LogInformation("OsuCommands loaded");
        }

        [Command("map-search"), Description("Поиск карт по заданной строке"), RequireGuild]
        public async Task OsuSearch(CommandContext ctx, 
            [Description("Поисковый запрос"), RemainingText] string querry)
        {
            if (string.IsNullOrEmpty(querry))
            {
                await ctx.RespondAsync("Задан пустой поисковый запрос");
                return;
            }

            var res = api.Search(querry);

            if (res is null || res.Count == 0)
            {
                await ctx.RespondAsync("Ничего не найдено");
                return;
            }

            var b = res.First();
            await ctx.RespondAsync(embed: osuEmbeds.BeatmapToEmbed(b.beatmaps.Last(), b));
        }

        [Command("osu"), Description("Получить информацию об osu! профиле"), RequireGuild]
        public async Task OsuProfile(CommandContext commandContext,
            [Description("osu! никнейм")] string nickname,
            params string[] args)
        {
            if (!((commandContext.Channel.Name?.Contains("-bot") ?? false) || 
                  (commandContext.Channel.Name?.Contains("dev-announce") ?? false) ||
                  (commandContext.Channel.Name?.Contains("-scores") ?? false)))
            {
                await commandContext.RespondAsync("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.");
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                await commandContext.RespondAsync("Вы ввели пустую строку.");
                return;
            }

            if (args.Any(x => x.ToLower() == "-gatari"))
            {
                GUser guser = null;
                if (!gapi.TryGetUser(nickname, ref guser))
                {
                    await commandContext.RespondAsync($"Не удалось получить информацию о пользователе `{nickname}`.");
                    return;
                }

                List<GScore> gscores = gapi.GetUserBestScores(guser.id, 5);
                if (gscores is null || gscores.Count == 0)
                {
                    await commandContext.RespondAsync($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`.");
                    return;
                }

                GStatistics gstats = gapi.GetUserStats(guser.username);
                if (gstats is null)
                {
                    await commandContext.RespondAsync($"Не удалось получить статистику пользователя `{nickname}`.");
                    return;
                }

                DiscordEmbed gembed = osuEmbeds.UserToEmbed(guser, gstats, gscores);
                await commandContext.RespondAsync(embed: gembed);
                return;
            }

            User user = null;
            if (!api.TryGetUser(nickname, ref user))
            {
                await commandContext.RespondAsync($"Не удалось получить информацию о пользователе `{nickname}`.");
                return;
            }

            List<Score> scores = api.GetUserBestScores(user.id, 5, user.playmode);

            if (scores is null || scores.Count == 0)
            {
                await commandContext.RespondAsync($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`.");
                return;
            }

            DiscordEmbed embed = osuEmbeds.UserToEmbed(user, scores);
            await commandContext.RespondAsync(embed: embed);

        }
    }

}