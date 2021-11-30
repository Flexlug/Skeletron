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
using Skeletron.Database.Models;
using Skeletron.Database.Interfaces;

using OsuNET_Api;
using OsuNET_Api.Models;
using OsuNET_Api.Models.Bancho;
using OsuNET_Api.Models.Gatari;
using Skeletron.Services.Interfaces;

namespace Skeletron.Commands
{
    public class OsuCommands : SkBaseCommandModule
    {
        private ILogger<OsuCommands> logger;

        private DiscordGuild guild;

        private OsuEmbed osuEmbeds;
        private OsuEnums osuEnums;

        private IMembersProvider wavMembers;
        private IOsuService osuService;

        private BanchoApi api;
        private GatariApi gapi;

        public OsuCommands(ILogger<OsuCommands> logger,
                           DiscordClient client, 
                           DiscordGuild guild,
                           OsuEmbed osuEmbeds,
                           OsuEnums osuEnums,
                           BanchoApi api,
                           GatariApi gapi,
                           IMembersProvider wavMembers,
                           ICompitProvider wavProvider,
                           IOsuService osuService)
        {
            ModuleName = "osu!";

            this.logger = logger;

            this.wavMembers = wavMembers;

            this.guild = guild;

            this.osuEmbeds = osuEmbeds;
            this.osuEnums = osuEnums;

            this.osuService = osuService;

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

            List<Score> scores = api.GetUserBestScores(user.id, 5);

            if (scores is null || scores.Count == 0)
            {
                await commandContext.RespondAsync($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`.");
                return;
            }

            DiscordEmbed embed = osuEmbeds.UserToEmbed(user, scores);
            await commandContext.RespondAsync(embed: embed);

        }

        [Command("rs"), Description("Получить последний скор"), RequireGuild]
        public async Task LastRecent(CommandContext commandContext,
            params string[] args)
        {
            if (!((commandContext.Channel.Name?.Contains("-bot") ?? false) ||
                  (commandContext.Channel.Name?.Contains("dev-announce") ?? false) ||
                  (commandContext.Channel.Name?.Contains("-scores") ?? false)))
            {
                await commandContext.RespondAsync("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.");
                return;
            }

            string discordId = commandContext.Member.Id.ToString();

            OsuServer? mbChoosedServer = osuEnums.StringToOsuServer(args.FirstOrDefault()?.TrimStart('-') ?? "bancho");
            if (mbChoosedServer is null)
            {
                await commandContext.RespondAsync($"Указанный сервер не поддерживается.");
                return;
            }

            OsuServer choosedServer = (OsuServer)mbChoosedServer;

            OsuProfileInfo userInfo = wavMembers.GetOsuProfileInfo(discordId, choosedServer);
            if (userInfo is null)
            {
                await commandContext.RespondAsync($"Не удалось найти ваш osu! профиль сервера `{choosedServer}`. Добавьте свой профиль через команду `osuset`");
                return;
            }

            switch (choosedServer)
            {
                case OsuServer.Gatari:
                    GScore gscore = gapi.GetUserRecentScores(userInfo.OsuId, 0, 1, true).FirstOrDefault();

                    if (gscore is null)
                    {
                        await commandContext.RespondAsync("У Вас нет недавно сыгранных карт в режиме osu!std.");
                        return;
                    }

                    GUser guser = null;
                    if (!gapi.TryGetUser(userInfo.OsuId, ref guser))
                    {
                        await commandContext.RespondAsync("Не удалось найти такого пользователя на Gatari.");
                        return;
                    }

                    DiscordEmbed gscoreEmbed = osuEmbeds.GatariScoreToEmbed(gscore, guser);
                    await commandContext.RespondAsync(embed: gscoreEmbed);

                    return;

                case OsuServer.Bancho:
                    Score score = api.GetUserRecentScores(userInfo.OsuId, true, 0, 1).FirstOrDefault();

                    if (score is null)
                    {
                        await commandContext.RespondAsync("У Вас нет недавно сыгранных карт в режиме osu!std.");
                        return;
                    }

                    User user = null;
                    if (!api.TryGetUser(userInfo.OsuId, ref user))
                    {
                        await commandContext.RespondAsync("Не удалось найти такого пользователя на Gatari.");
                        return;
                    }

                    DiscordEmbed scoreEmbed = osuEmbeds.BanchoScoreToEmbed(score, user);
                    await commandContext.RespondAsync(embed: scoreEmbed);
                    return;
            }

            await commandContext.RespondAsync($"Указанный сервер не поддерживается.");
        }

        [Command("osuset-manual"), Description("Добавить информацию о чужом osu! профиле"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), RequireGuild]
        public async Task OsuSet(CommandContext commandContext,
            [Description("Пользователь WAV сервера")] DiscordMember member,
            [Description("Никнейм osu! профиля")] string nickname,
            [Description("osu! cервер (по-умолчанию bancho)")] params string[] args)
        {
            await SetOsuProfile(commandContext, member, nickname, args);
        }

        [Command("osuset-manual"), Description("Добавить информацию о чужом osu! профиле"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder", "Assistant Moder"), RequireGuild]
        public async Task OsuSet(CommandContext commandContext,
            [Description("Пользователь WAV сервера")] ulong uid,
            [Description("Никнейм osu! профиля")] string nickname,
            [Description("osu! cервер (по-умолчанию bancho)")] params string[] args)
        {
            DiscordMember dmember = await guild.GetMemberAsync(uid);
            if (dmember is null)
            {
                await commandContext.RespondAsync("Не удалось найти такого пользователя.");
                return;
            }

            await SetOsuProfile(commandContext, dmember, nickname, args);
        }

        [Command("osuset"), Description("Добавить информацию о своём osu! профиле")]
        public async Task OsuSet(CommandContext commandContext,
            [Description("Никнейм osu! профиля")] string nickname,
            [Description("osu! cервер (по-умолчанию bancho)")] params string[] args)
        {
            await SetOsuProfile(commandContext, commandContext.User, nickname, args);
        }

        private async Task SetOsuProfile(CommandContext commandContext, DiscordUser user, string nickname, string[] args)
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
                await commandContext.RespondAsync("Вы ввели пустой никнейм.");
                return;
            }

            string res = await osuService.SetOsuProfile(user, nickname, args);
            await commandContext.RespondAsync(string.IsNullOrEmpty(res) ? "Ошибка при выполнении команды" : res);
        }
    }

}