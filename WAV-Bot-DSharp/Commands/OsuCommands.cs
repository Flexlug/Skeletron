using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using OsuParsers.Replays;
using OsuParsers.Decoders;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Database;
using WAV_Bot_DSharp.Services.Entities;

using WAV_Osu_NetApi;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Osu_NetApi.Models.Bancho;
using WAV_Osu_NetApi.Models.Gatari;
using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Commands
{
    public class OsuCommands : SkBaseCommandModule
    {
        private ILogger<OsuCommands> logger;

        private DiscordChannel wavScoresChannel;
        private DiscordGuild guild;

        private WebClient webClient;

        private OsuEmbed osuEmbeds;
        private OsuEmoji osuEmoji;
        private OsuRegex osuRegex;
        private OsuEnums osuEnums;

        private IWAVMembersProvider wavMembers;
        private IWAVCompitProvider wavCompit;

        private BanchoApi api;
        private GatariApi gapi;

        private ShedulerService sheduler;

        private readonly ulong WAV_UID = 708860200341471264;

        public OsuCommands(ILogger<OsuCommands> logger,
                           DiscordClient client,
                           OsuEmbed osuEmbeds,
                           OsuEmoji osuEmoji,
                           OsuRegex osuRegex,
                           OsuEnums osuEnums,
                           BanchoApi api,
                           GatariApi gapi,
                           IWAVMembersProvider wavMembers,
                           IWAVCompitProvider wavProvider)
        {
            ModuleName = "Osu commands";

            this.logger = logger;
            this.wavScoresChannel = client.GetChannelAsync(829466881353711647).Result;
            this.webClient = new WebClient();

            this.wavMembers = wavMembers;
            this.wavCompit = wavProvider;

            this.guild = client.GetGuildAsync(WAV_UID).Result;

            this.osuEmbeds = osuEmbeds;
            this.osuEmoji = osuEmoji;
            this.osuRegex = osuRegex;
            this.osuEnums = osuEnums;

            this.api = api;
            this.gapi = gapi;

            logger.LogInformation("OsuCommands loaded");

            client.MessageCreated += Client_MessageCreated;
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (!e.Message.Content.Contains("http"))
                return;

            if (!(e.Channel.Name.Contains("-osu") ||
                  e.Channel.Name.Contains("map-offer") ||
                  e.Channel.Name.Contains("bot-debug") ||
                  e.Channel.Name.Contains("dev-announce") ||
                  e.Channel.Name.Contains("www-register")))
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


        [Command("osu"), Description("Получить информацию об osu! профиле"), RequireGuild]
        public async Task OsuProfile(CommandContext commandContext,
            [Description("Osu nickname")] string nickname,
            params string[] args)
        {
            if (!(commandContext.Channel.Name.Contains("-bot") || commandContext.Channel.Name.Contains("dev-announce")))
            {
                await commandContext.RespondAsync("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.");
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                await commandContext.RespondAsync("Вы ввели пустую строку.");
                return;
            }

            if (args.Contains("-gatari"))
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
            if (!(commandContext.Channel.Name.Contains("-bot") || commandContext.Channel.Name.Contains("dev-announce")))
            {
                await commandContext.RespondAsync("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.");
                return;
            }

            ulong discordId = commandContext.Member.Id;

            OsuServer? mbChoosedServer = osuEnums.StringToOsuServer(args.FirstOrDefault().TrimStart('-') ?? "bancho");
            if (mbChoosedServer is null)
            {
                await commandContext.RespondAsync($"Указанный сервер не поддерживается.");
                return;
            }

            OsuServer choosedServer = (OsuServer)mbChoosedServer;

            WAVMemberOsuProfileInfo userInfo = wavMembers.GetOsuProfileInfo(discordId, choosedServer);
            if (userInfo is null)
            {
                await commandContext.RespondAsync($"Не удалось найти ваш osu! профиль сервера `{choosedServer}`. Добавьте свой профиль через команду `osuset`");
                return;
            }

            switch (choosedServer)
            {
                case OsuServer.Gatari:
                    GScore gscore = gapi.GetUserRecentScores(userInfo.Id, 0, 1, true).First();

                    GUser guser = null;
                    if (!gapi.TryGetUser(userInfo.Id, ref guser))
                    {
                        await commandContext.RespondAsync("Не удалось найти такого пользователя на Gatari.");
                        return;
                    }

                    DiscordEmbed gscoreEmbed = osuEmbeds.GatariScoreToEmbed(gscore, guser);
                    await commandContext.RespondAsync(embed: gscoreEmbed);

                    return;

                case OsuServer.Bancho:
                    Score score = api.GetUserRecentScores(userInfo.Id, true, 0, 1).First();

                    User user = null;
                    if (!api.TryGetUser(userInfo.Id, ref user))
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
            await SetOsuProfileFor(commandContext, member, nickname, args);
        }

        [Command("osuset"), Description("Добавить информацию о своём osu! профиле"), RequireGuild]
        public async Task OsuSet(CommandContext commandContext,
            [Description("Никнейм osu! профиля")] string nickname,
            [Description("osu! cервер (по-умолчанию bancho)")] params string[] args)
        {
            await SetOsuProfileFor(commandContext, commandContext.Member, nickname, args);
        }


        public async Task SetOsuProfileFor(CommandContext commandContext, DiscordUser user, string nickname, params string[] args)
        {
            ulong discordId = user.Id;

            WAVMember member = null;

            if (wavMembers.GetMember(discordId) is null)
                member = new WAVMember(discordId);

            if (!(commandContext.Channel.Name.Contains("-bot") || commandContext.Channel.Name.Contains("dev-announce")))
            {
                await commandContext.RespondAsync("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.");
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                await commandContext.RespondAsync("Вы ввели пустой никнейм.");
                return;
            }

            int osu_id = 0;
            string osu_nickname = string.Empty;

            OsuServer? mbChoosedServer = osuEnums.StringToOsuServer(args.FirstOrDefault().TrimStart('-') ?? "bancho");
            if (mbChoosedServer is null)
            {
                await commandContext.RespondAsync($"Указанный сервер не поддерживается.");
                return;
            }

            OsuServer choosedServer = (OsuServer)mbChoosedServer;

            switch (choosedServer)
            {
                case OsuServer.Gatari:
                    GUser guser = null;
                    if (!gapi.TryGetUser(nickname, ref guser))
                    {
                        await commandContext.RespondAsync("Не удалось найти такого пользователя на Gatari.");
                        return;
                    }
                    osu_nickname = guser.username;
                    osu_id = guser.id;
                    break;

                case OsuServer.Bancho:
                    User buser = null;
                    if (!api.TryGetUser(nickname, ref buser))
                    {
                        await commandContext.RespondAsync("Не удалось найти такого пользователя на Bancho.");
                        return;
                    }
                    osu_nickname = buser.username;
                    osu_id = buser.id;
                    break;

                default:
                    await commandContext.RespondAsync($"Сервер `{choosedServer}` не поддерживается.");
                    break;
            }

            try
            {
                wavMembers.AddOsuServerInfo(discordId, choosedServer, osu_id);
                await commandContext .RespondAsync($"Вы успешно добавили информацию о своём профиле `{osu_nickname}` на сервере `{osuEnums.OsuServerToString(choosedServer)}`");
            }
            catch (NullReferenceException)
            {
                logger.LogError("User not found in OsuSet command");
                await commandContext.RespondAsync("Вас не удалось найти в базе данных участников WAV.");
            }
        }

        [Command("submit"), RequireDirectMessage]
        public async Task SubmitScore(CommandContext commandContext)
        {
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Id);
            logger.LogInformation($"DM {msg.Author}: {msg.Content} : {msg.Attachments.Count}");

            WAVMemberCompitInfo compitInfo = wavCompit.GetParticipationInfo(commandContext.Member.Id);

            if (compitInfo.NonGrata)
            {
                await commandContext.RespondAsync("Извините, но вы не можете принять участие в данном конкурсе, т.к. внесены в черный список.");
                return;
            }

            if (compitInfo.ProvidedScore)
            {
                await commandContext.RespondAsync("Вы уже отправили скор.");
                return;
            }

            if (msg.Attachments.Count == 0)
            {
                await commandContext.RespondAsync("Вы не прикрепили к сообщению никаких файлов.");
                return;
            }

            if (msg.Attachments.Count > 1)
            {
                await commandContext.RespondAsync("К сообщению можно прикрепить только один файл.");
                return;
            }

            DiscordAttachment attachment = msg.Attachments.First();

            if (!attachment.FileName.EndsWith("osr"))
            {
                await commandContext.RespondAsync("Файл не является реплеем.");
                return;
            }

            Replay replay = null;

            try
            {
                string fileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
                webClient.DownloadFile(attachment.Url, $"downloads/{fileName}");

                replay = ReplayDecoder.Decode($"downloads/{fileName}");
                sheduler.AddFileDeleteTask(fileName);
            }

            catch (Exception e)
            {
                logger.LogCritical(e, "Exception while parsing score");
            }

            if ((int)replay.Mods != 0)
            {
                const int b = (int)(OsuParsers.Enums.Mods.NoFail | OsuParsers.Enums.Mods.Perfect | OsuParsers.Enums.Mods.SuddenDeath);
                if (((int)replay.Mods | b) != b)
                {
                    await commandContext.RespondAsync("Мы не можем принять данный скор по причине того, что он поставлен с запрещенными на W.w.W модами. \nРазрешенные на W.w.W моды - `NF`, `SD`, `PF`\nСкор система: V1");
                    return;
                }
            }

            DiscordMember member = await guild.GetMemberAsync(msg.Author.Id);
            string category = member.Roles.Select(x => x.Name)
                                          .FirstOrDefault((x) =>
                                          {
                                              foreach (var xx in (new string[] { "beginner", "alpha", "beta", "gamma", "delta", "epsilon" }))
                                                  if (x.Contains(xx))
                                                      return true;
                                              return false;
                                          });

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Osu nickname: `{replay.PlayerName}`");
            sb.AppendLine($"Discord nickname: `{msg.Author.Username}`");
            sb.AppendLine($"Score: `{replay.ReplayScore:N0}`"); // Format: 123456789 -> 123 456 789
            sb.AppendLine($"Category: `{category ?? "No category"}`");
            sb.AppendLine($"Mods: `{osuEnums.ModsToString((Mods)replay.Mods)}`");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithAuthor(msg.Author.Username, iconUrl: msg.Author.AvatarUrl)
                                                                 .WithTitle($"Added replay {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}")
                                                                 .WithUrl(attachment.Url)
                                                                 .WithDescription(sb.ToString())
                                                                 .AddField("OSR Link:", attachment.Url)
                                                                 .AddField("File name:", $"`{attachment.FileName}`")
                                                                 .WithTimestamp(DateTime.Now);

            await wavScoresChannel.SendMessageAsync(embed: embed);
            await commandContext.RespondAsync("Ваш скор был отправлен на рассмотрение. Спасибо за участие!");
        }
    }

}