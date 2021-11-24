using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using OsuNET_Api;
using OsuNET_Api.Models;
using OsuNET_Api.Models.Bancho;
using OsuNET_Api.Models.Gatari;
using Skeletron.Converters;
using Skeletron.Database.Interfaces;
using Skeletron.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeletron.Services
{
    /// <summary>
    /// Сервис, отслеживащий ссылки из osu
    /// </summary>
    internal class OsuService
    {
        private ILogger<OsuService> _logger;

        private OsuRegex osuRegex;
        private OsuEmbed osuEmbeds;
        private OsuEnums osuEnums;

        private IMembersProvider wavMembers;
        private ICompitProvider wavCompit;

        private BanchoApi api;
        private GatariApi gapi;

        public OsuService(ILogger<OsuService> logger,
                          DiscordClient client,
                          OsuRegex osuRegex,
                          BanchoApi api,
                          GatariApi gapi,
                          OsuEnums osuEnums,
                          OsuEmbed osuEmbeds,
                          IMembersProvider wavMembers,
                          ICompitProvider wavCompit)
        {
            this.osuRegex = osuRegex;

            this.api = api;
            this.gapi = gapi;
            this.osuEnums = osuEnums;
            this.osuEmbeds = osuEmbeds;

            this.wavMembers = wavMembers;
            this.wavCompit = wavCompit;

            this._logger = logger;
            _logger.LogInformation("OsuService loaded");

            client.MessageCreated += Client_MessageCreated;
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            await GetMap(sender, e);
        }

        public async Task GetMap(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (!e.Message.Content.Contains("http"))
                return;

            if (e.Channel is null)
                return;

            if (e.Message.ReferencedMessage?.Author.IsBot ?? false)
                return;

            if (!(e.Channel.Name.Contains("-osu") ||
                  e.Channel.Name.Contains("bot-debug") ||
                  e.Channel.Name.Contains("dev-announce")))
                return;

            if (e.Message.Content[0] == '>' || e.Message.Content[0] == '!' || e.Message.Content[0] == '<' || e.Message.Content[0..2] == "y!")
                return;

            // Check, if it is map url from bancho
            Tuple<int, int> BMSandBMid = osuRegex.GetBMandBMSIdFromBanchoUrl(e.Message.Content);
            if (!(BMSandBMid is null))
            {
                int bms_id = BMSandBMid.Item1,
                    bm_id = BMSandBMid.Item2;

                Beatmap bm = api.GetBeatmap(bm_id);
                Beatmapset bms = api.GetBeatmapset(bms_id);
                GBeatmap gbm = gapi.TryGetBeatmap(bm_id);

                if (!(bm is null || bms is null))
                {
                    DiscordEmbed embed = osuEmbeds.BeatmapToEmbed(bm, bms, gbm);
                    await e.Message.RespondAsync(embed: embed);
                }

                return;
            }

            // Check, if it is beatmapset url from gatari
            int? BMSid = osuRegex.GetBMSIdFromGatariUrl(e.Message.Content);
            if (!(BMSid is null))
            {
                int bms_id = (int)BMSid;

                Beatmapset bms = api.GetBeatmapset(bms_id);

                int bm_id = bms.beatmaps.First().id;

                Beatmap bm = api.GetBeatmap(bm_id);
                GBeatmap gbm = gapi.TryGetBeatmap(bm_id);

                if (!(bm is null || bms is null))
                {
                    DiscordEmbed embed = osuEmbeds.BeatmapToEmbed(bm, bms, gbm);
                    await e.Message.RespondAsync(embed: embed);
                }

                return;
            }

            // Check, if it is beatmap url from gatari
            int? BMid = osuRegex.GetBMIdFromGatariUrl(e.Message.Content);
            if (!(BMid is null))
            {
                int bm_id = (int)BMid;

                Beatmap bm = api.GetBeatmap(bm_id);
                Beatmapset bms = api.GetBeatmapset(bm.beatmapset_id);
                GBeatmap gbm = gapi.TryGetBeatmap(bm_id);

                if (!(bm is null || bms is null))
                {
                    DiscordEmbed embed = osuEmbeds.BeatmapToEmbed(bm, bms, gbm);
                    await e.Message.RespondAsync(embed: embed);
                }

                return;
            }

            // Check, if it is user link from bancho
            int? userId = osuRegex.GetUserIdFromBanchoUrl(e.Message.Content);
            if (!(userId is null))
            {
                int user_id = (int)userId;

                User user = null;
                if (!api.TryGetUser(user_id, ref user))
                    return;

                List<Score> scores = api.GetUserBestScores(user_id, 5);

                if (!(scores is null) && scores.Count == 5)
                {
                    DiscordEmbed embed = osuEmbeds.UserToEmbed(user, scores);
                    await e.Message.RespondAsync(embed: embed);
                }

                return;
            }

            // Check, if it is user link from bancho
            int? guserId = osuRegex.GetUserIdFromGatariUrl(e.Message.Content);
            if (!(guserId is null))
            {
                int guser_id = (int)guserId;

                GUser guser = null;
                if (!gapi.TryGetUser(guser_id, ref guser))
                    return;

                List<GScore> gscores = gapi.GetUserBestScores(guser.id, 5);
                if (gscores is null || gscores.Count == 0)
                    return;

                GStatistics gstats = gapi.GetUserStats(guser.username);
                if (gstats is null)
                    return;

                DiscordEmbed gembed = osuEmbeds.UserToEmbed(guser, gstats, gscores);
                await e.Message.RespondAsync(embed: gembed);
                return;
            }
        }

        public async Task<string> SetOsuProfileFor(DiscordUser user, string nickname, params string[] args)
        {
            string discordId = user.Id.ToString();

            WAVMembers member = wavMembers.GetMember(discordId);

            int osu_id = 0;
            string osu_nickname = string.Empty;

            OsuServer? mbChoosedServer = osuEnums.StringToOsuServer(args.FirstOrDefault()?.TrimStart('-') ?? "bancho");
            if (mbChoosedServer is null)
            {
                return $"Указанный сервер не поддерживается.";
            }

            OsuServer choosedServer = (OsuServer)mbChoosedServer;

            switch (choosedServer)
            {
                case OsuServer.Gatari:
                    GUser guser = null;
                    if (!gapi.TryGetUser(nickname, ref guser))
                    {
                        return "Не удалось найти такого пользователя на Gatari.";
                    }
                    osu_nickname = guser.username;
                    osu_id = guser.id;
                    break;

                case OsuServer.Bancho:
                    User buser = null;
                    if (!api.TryGetUser(nickname, ref buser))
                    {
                        return "Не удалось найти такого пользователя на Bancho.";
                    }
                    osu_nickname = buser.username;
                    osu_id = buser.id;
                    break;

                default:
                    return $"Сервер `{choosedServer}` не поддерживается.";
            }

            try
            {
                OsuProfileInfo profile = new OsuProfileInfo()
                {
                    OsuId = osu_id,
                    OsuNickname = osu_nickname,
                    Server = choosedServer
                };

                wavMembers.AddOsuServerInfo(discordId, profile);
                return $"Вы успешно добавили информацию о профиле `{osu_nickname}` на сервере `{osuEnums.OsuServerToString(choosedServer)}` для `{user.Username}`";
            }
            catch (NullReferenceException)
            {
                _logger.LogError("User not found in OsuSet command");
                return "Вас не удалось найти в базе данных участников WAV.";
            }
        }

    }
}
