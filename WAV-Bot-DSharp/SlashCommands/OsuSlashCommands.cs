using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.Logging;
using WAV_Bot_DSharp.Converters;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Bancho;
using WAV_Osu_NetApi.Bancho.Models;
using WAV_Osu_NetApi.Gatari;
using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.SlashCommands
{
    class OsuSlashCommands : SlashCommandModule
    {
        private BanchoApi api;
        private GatariApi gapi;
        private OsuUtils utils;

        private ILogger logger;

        public OsuSlashCommands(BanchoApi api, GatariApi gapi, OsuUtils utils, ILogger<OsuSlashCommands> logger)
        {
            this.api = api;
            this.gapi = gapi;
            this.utils = utils;

            this.logger = logger;

            logger.LogInformation("OsuSlashCommands loaded");
        }

        [SlashCommand("osu", "Получить информацию о профиле osu в режиме std")]
        public async Task OsuProfile(InteractionContext ctx,
            [Option("nickname", "Никнейм юзера")] string nickname,
            [Choice("Bancho server", "bancho")]
            [Choice("Gatari server", "gatari")]
            [Option("server", "Возможные параметры: bancho, gatari")] string args)
        {
            if (!(ctx.Channel.Name.Contains("-bot") || ctx.Channel.Name.Contains("dev-announce")))
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, 
                                              new DiscordInteractionResponseBuilder().WithContent("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.")
                                                                                     .AsEphemeral(true));
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent("Вы ввели пустой никнейм..")
                                                                                     .AsEphemeral(true));
                return;
            }

            if (args.Contains("gatari"))
            {
                GUser guser = null;
                if (!gapi.TryGetUser(nickname, ref guser))
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о пользователе `{nickname}`.")
                                                                                     .AsEphemeral(true));;
                    return;
                }

                List<GScore> gscores = gapi.GetUserBestScores(guser.id, 5);
                if (gscores is null || gscores.Count == 0)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`.")
                                                                                     .AsEphemeral(true));
                    return;
                }

                GStatistics gstats = gapi.GetUserStats(guser.username);
                if (gstats is null)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить статистику пользователя `{nickname}`.")
                                                                                     .AsEphemeral(true));
                    return;
                }

                DiscordEmbed gembed = utils.UserToEmbed(guser, gstats, gscores);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().AddEmbed(gembed));
                return;
            }

            if (args.Contains("bancho"))
            {
                User user = null;
                if (!api.TryGetUser(nickname, ref user))
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                                  new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о пользователе `{nickname}`.")
                                                                                         .AsEphemeral(true));
                    return;
                }

                List<Score> scores = api.GetUserBestScores(user.id, 5);

                if (scores is null || scores.Count == 0)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                                  new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`.")
                                                                                         .AsEphemeral(true));
                    return;
                }

                DiscordEmbed embed = utils.UserToEmbed(user, scores);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                                  new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                                  new DiscordInteractionResponseBuilder().WithContent("Введенный сервер не поддерживается или не существует.")
                                                                                         .AsEphemeral(true));
        }
    }
}
