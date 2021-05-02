using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using WAV_Bot_DSharp.Converters;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Bancho;
using WAV_Osu_NetApi.Bancho.Models;
using WAV_Osu_NetApi.Gatari;
using WAV_Osu_NetApi.Gatari.Models;

namespace WAV_Bot_DSharp.SlashCommands
{
    class OsuSCommands : SlashCommandModule
    {
        private BanchoApi api;
        private GatariApi gapi;
        private OsuUtils utils;

        public OsuSCommands()
        {

        }

        [SlashCommand("osu", "Получить информацию о профиле osu в режиме std")]
        public async Task OsuProfile(InteractionContext ctx,
            [Option("nickname", "Никнейм юзера")] string nickname,
            [Option("params", "Возможные параметры: -gatari = получить информацию с сервера gatari")] params string[] args)
        {
            if (!(ctx.Channel.Name.Contains("-bot") || ctx.Channel.Name.Contains("dev-announce")))
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource, 
                                              new DiscordInteractionResponseBuilder().WithContent("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!."));
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent("Вы ввели пустой никнейм.."));
                return;
            }

            if (args.Contains("-gatari"))
            {
                GUser guser = null;
                if (!gapi.TryGetUser(nickname, ref guser))
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о пользователе `{nickname}`."));;
                    return;
                }

                List<GScore> gscores = gapi.GetUserBestScores(guser.id, 5);
                if (gscores is null || gscores.Count == 0)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`."));
                    return;
                }

                GStatistics gstats = gapi.GetUserStats(guser.username);
                if (gstats is null)
                {
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить статистику пользователя `{nickname}`."));
                    return;
                }

                DiscordEmbed gembed = utils.UserToEmbed(guser, gstats, gscores);
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().AddEmbed(gembed));
                return;
            }

            User user = null;
            if (!api.TryGetUser(nickname, ref user))
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о пользователе `{nickname}`."));
                return;
            }

            List<Score> scores = api.GetUserBestScores(user.id, 5);

            if (scores is null || scores.Count == 0)
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent($"Не удалось получить информацию о лучших скорах пользователя `{nickname}`."));
                return;
            }

            DiscordEmbed embed = utils.UserToEmbed(user, scores);
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}
