using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using OsuParsers.Decoders;
using OsuParsers.Replays;

using WAV_Osu_NetApi.Models;
using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Services.Interfaces;
using System.Collections.Generic;

namespace WAV_Bot_DSharp.Commands
{
    [Group("www")]
    public class CompititionCommands : SkBaseCommandModule
    {
        private ILogger<CompititionCommands> logger;

        private OsuEmbed osuEmbeds;
        private OsuEnums osuEnums;

        private WebClient webClient;

        private DiscordGuild guild;

        private IWAVCompitProvider wavCompit;
        private IWAVMembersProvider wavMembers;
        private ICompititionService compititionService;

        private ISheetGenerator generator;

        public CompititionCommands(ILogger<CompititionCommands> logger,
                                   OsuEmbed osuEmbeds,
                                   OsuEnums osuEnums,
                                   DiscordClient client,
                                   DiscordGuild guild,
                                   IWAVMembersProvider wavMembers,
                                   IWAVCompitProvider wavCompit,
                                   ICompititionService compititionService,
                                   ISheetGenerator generator)
        {
            this.osuEmbeds = osuEmbeds;
            this.osuEnums = osuEnums;
            this.logger = logger;
            this.guild = guild;
            this.wavCompit = wavCompit;
            this.wavMembers = wavMembers;
            this.compititionService = compititionService;
            this.generator = generator;

            this.webClient = new WebClient();

            this.ModuleName = "W.w.W команды";

            this.logger.LogInformation("CompititionCommands loaded");
        }

        [Command("start"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task StartCompit(CommandContext commandContext)
        {
            string checkResult = await compititionService.CompititionPreexecutionCheck();

            if (checkResult == "done")
            {
                await compititionService.InitCompitition();
                await commandContext.RespondAsync("Starting...");
            }
            else
            {
                await commandContext.RespondAsync($"`{checkResult}`");
            }
        }

        [Command("stop"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task StopCompit(CommandContext commandContext)
        {
            await compititionService.StopCompition();
            await commandContext.RespondAsync("Stopped");
        }

        [Command("update-leaderboard"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task UpdateLeaderboard(CommandContext commandContext)
        {
            await compititionService.UpdateLeaderboard();
            await commandContext.RespondAsync("Leaderboard updated");
        }

        [Command("set-map"), Description("Задать карту для выбранной категории"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task SetMap(CommandContext commandContext,
            [Description("Ссылка на карту (только bancho)")] string url,
            [Description("Категория")] string category)
        {
            if (compititionService.GetCompitInfo().IsRunning)
            {
                await commandContext.RespondAsync("Нельзя редактировать маппул во время конкурса.");
                return;
            }

            bool res = await compititionService.SetMap(url, category);
            if (!res)
                await commandContext.RespondAsync("Не удалось задать карту. Проверьте ссылку или имя категории.");
            else
                await commandContext.RespondAsync("Карта успешно задана.");
        }

        [Command("set-deadline"), Description("Задать дату окончания конкурса"), RequireGuild, Hidden]
        public async Task SetDeadline(CommandContext commandContext,
            [Description("Дата, когда конкурс должен закончиться")] DateTime deadline)
        {
            if (deadline < DateTime.Now)
                await commandContext.RespondAsync($"Неправильная дата: {deadline}.");

            await compititionService.SetDeadline(deadline);
            await commandContext.RespondAsync($"Дата окончания конкурса: {deadline}.");
        }

        [Command("set-scores-channel"), Description("Задать канал для скоров"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task SetScoresChannel(CommandContext commandContext,
            [Description("Текстовый канал")] DiscordChannel channel)
        {
            bool res = await compititionService.SetScoresChannel(channel.Id.ToString());
            if (!res)
                await commandContext.RespondAsync("Не удалось задать канал для скоров. Возможно канал недоступен для бота.");
            else
                await commandContext.RespondAsync("Канал для скоров успешно задан.");
        }

        [Command("set-leaderboard-channel"), Description("Задать канал для лидерборда"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task SetLeaderboardChannel(CommandContext commandContext,
            [Description("Текстовый канал")] DiscordChannel channel)
        {
            bool res = await compititionService.SetLeaderboardChannel(channel.Id.ToString());
            if (!res)
                await commandContext.RespondAsync("Не удалось задать канал для лидерборда. Возможно канал недоступен для бота.");
            else
                await commandContext.RespondAsync("Канал для лидерборда успешно задан.");
        }

        [Command("notify-manual"), Description("Включить или отключить пинги по всему, что связано с конкурсом"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task ToggleNotifications(CommandContext commandContext,
            DiscordUser discordUser,
            bool toggle)
        {
            if (discordUser is null)
            {
                await commandContext.RespondAsync("Не удалось найти такого пользователя.");
                return;
            }

            WAVMember member = wavMembers.GetMember(discordUser.Id.ToString());

            if (member.CompitionProfile is null)
            {
                await commandContext.RespondAsync("Указаный пользователь не зарегистрирован.");
                return;
            }

            if (toggle)
            {
                await compititionService.EnableNotifications(discordUser, member.CompitionProfile);
                await commandContext.RespondAsync("Уведомления включены.");
            }
            else
            {
                await compititionService.DisableNotifications(discordUser);
                await commandContext.RespondAsync("Уведомления выключены.");
            }
        }

        [Command("notify"), Description("Включить или отключить пинги по всему, что связано с конкурсом"), RequireGuild]
        public async Task ToggleNotifications(CommandContext commandContext,
            [Description("True или False")] bool toggle)
        {
            await ToggleNotifications(commandContext, commandContext.Member, toggle);
        }

        [Command("recount"), Description("Пересчитать среднее PP"), RequireGuild]
        public async Task Recount(CommandContext context)
        {
            await RecountManual(context, context.Member);
        }

        [Command("recount-manual"), Description("Пересчитать среднее PP для заданного участника"), RequireGuild, Hidden]
        public async Task RecountManual(CommandContext context, DiscordMember dmember)
        {
            WAVMember member = wavMembers.GetMember(dmember.Id.ToString());

            if (member.OsuServers.Count == 0)
            {
                await context.RespondAsync("К Вашему профилю ещё нет привязаных osu! профилей. Привяжите свой профиль через `osuset`.");
                return;
            }

            if (member.CompitionProfile is null)
            {
                await context.RespondAsync("Вы не зарегистрированы.");
                return;
            }

            OsuServer server = member.CompitionProfile.Server;

            WAVMemberOsuProfileInfo profileInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);
            if (profileInfo is null)
            {
                await context.RespondAsync($"К Вашему профилю нет привязанного профиля сервера {osuEnums.OsuServerToString(server)}.");
                return;
            }

            CompitInfo compitInfo = wavCompit.GetCompitionInfo();
            if (compitInfo.IsRunning)
            {
                await context.RespondAsync("Во время конкурса нельзя пересчитать свои PP.");
                return;
            }

            try
            {
                await compititionService.RegisterMember(dmember, profileInfo);
                await context.RespondAsync($"Средний PP пересчитан.");
            }
            catch (Exception e)
            {
                await context.RespondAsync(e.Message);
            }
        }

        [Command("register"), Description("Зарегистрироваться в конкурсе W.w.W и получить категорию. Зарегистрироваться можно только один раз. Средний PP будет время от времени пересчитываться.")]
        public async Task Register(CommandContext commandContext,
            [Description("Сервер, на котором находится основной osu! профиль")] string strServer)
        {
            await RegisterUser(commandContext, commandContext.User, strServer);
        }

        [Command("non-grata"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task NonGrata(CommandContext commandContext,
            DiscordUser user,
            bool toggle)
        {
            if (user is null)
            {
                await commandContext.RespondAsync("Не удалось найти такого пользователя.");
                return;
            }

            compititionService.SetNonGrata(user, toggle);
            await commandContext.RespondAsync($"Задан статус non-grata `{toggle}` для {user.Username}.");
        }

        [Command("register-manual-by-nickname"), Description("Зарегистрировать другого участника в конкурсе"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task RegisterUser(CommandContext commandContext,
            [Description("Регистрируемый участник")] string strMember,
            [Description("Сервер, на котором находится основной osu! профиль")] string strServer)
        {
            DiscordUser duser = (await guild.GetAllMembersAsync()).FirstOrDefault(x => x.Username == strMember);
            if (duser is null)
            {
                await commandContext.RespondAsync("Не удалось найти такого пользователя.");
                return;
            }

            await RegisterUser(commandContext, duser, strServer);
        }

        [Command("register-manual"), Description("Зарегистрировать другого участника в конкурсе"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task RegisterUser(CommandContext commandContext,
            [Description("Регистрируемый участник")] DiscordUser dmember,
            [Description("Сервер, на котором находится основной osu! профиль")] string strServer)
        {
            WAVMember member = wavMembers.GetMember(dmember.Id.ToString());

            if (member.OsuServers.Count == 0)
            {
                await commandContext.RespondAsync("К Вашему профилю ещё нет привязаных osu! профилей. Привяжите свой профиль через `osuset`.");
                return;
            }

            OsuServer? mbServer = osuEnums.StringToOsuServer(strServer);
            if (mbServer is null)
            {
                await commandContext.RespondAsync($"Не удалось распознать название сервера {strServer}.");
                return;
            }

            OsuServer server = (OsuServer)mbServer;
            WAVMemberOsuProfileInfo profileInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);
            if (profileInfo is null)
            {
                await commandContext.RespondAsync($"К Вашему профилю не привязанного профиля сервера {osuEnums.OsuServerToString(server)}.");
                return;
            }

            try
            {
                await compititionService.RegisterMember(dmember, profileInfo);
                await commandContext.RespondAsync($"Регистрация прошла успешно.");
            }
            catch (Exception e)
            {
                await commandContext.RespondAsync(e.Message);
            }
        }

        [Command("get-report"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task SendScoreSheet(CommandContext commandContext)
        {
            List<CompitScore> scores = wavCompit.GetAllScores();
            if (scores.Count == 0)
            {
                await commandContext.RespondAsync("Нет каких-либо скоров для создания отчета.");
                return;
            }

            FileStream sheetfileInfo = await generator.CompitScoresToFile(scores);
            await commandContext.RespondAsync(new DiscordMessageBuilder().WithFile(sheetfileInfo));
        }

        [Command("status"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task GetStatus(CommandContext commandContext)
        {
            CompitInfo compitInfo = wavCompit.GetCompitionInfo();

            await commandContext.RespondAsync(osuEmbeds.CompitInfoToEmbed(compitInfo));
        }

        [Command("reset-scores"), RequireUserPermissions(Permissions.Administrator), RequireGuild, Hidden]
        public async Task ResetAllScores(CommandContext commandContext)
        {
            wavCompit.DeleteAllScores();

            await commandContext.RespondAsync("Все скоры удалены.");
        }

        [Command("submit"), Description("Отправить свой скор (к сообщению необходимо прикрепить свой реплей в формате .osr)."), RequireDirectMessage, Cooldown(1, 30, CooldownBucketType.User)]
        public async Task SubmitScore(CommandContext commandContext)
        {
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Id);
            logger.LogInformation($"DM {msg.Author}: {msg.Content} : {msg.Attachments.Count}");
            
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

            CompitInfo compitInfo = compititionService.GetCompitInfo();

            if (!compitInfo.IsRunning)
            {
                await commandContext.RespondAsync("На данный момент конкурс ещё не запущен. Следите за обновлениями.");
                return;
            }

            WAVMember wavMember = wavMembers.GetMember(commandContext.User.Id.ToString());
            WAVMemberCompitProfile compitProfile = wavCompit.GetCompitProfile(commandContext.User.Id.ToString());
            if (compitProfile is null)
            {
                await commandContext.RespondAsync("Вы не зарегистрированы на конкурсе.");
                return;
            }

            if (compitProfile.NonGrata)
            {
                await commandContext.RespondAsync("Извините, но вы не можете принять участие в данном конкурсе, т.к. внесены в черный список.");
                return;
            }

            var scores = wavCompit.GetUserScores(commandContext.User.Id.ToString());
            if (!(scores is null || scores.Count == 0))
            {
                await commandContext.RespondAsync("Вы уже отправляли свой скор.");
                return;
            }

            Replay replay = null;

            string fileName = string.Empty;
            try
            {
                fileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
                webClient.DownloadFile(attachment.Url, $"downloads/{fileName}");

                replay = ReplayDecoder.Decode($"downloads/{fileName}");
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Exception while parsing score"); 
                await commandContext.RespondAsync("Не удалось считать реплей. Возмонжо он повержден.");
                return;
            }

            if (!replay.Mods.HasFlag(OsuParsers.Enums.Mods.ScoreV2))
            {
                await commandContext.RespondAsync("Мы не можем принять данный скор по причине того, что он поставлен с использованием скор системы V1. Для конкурса обязательно использование мода Score V2.");
                return;
            }

            const int allowedMods = (int)(OsuParsers.Enums.Mods.Perfect | OsuParsers.Enums.Mods.SuddenDeath | OsuParsers.Enums.Mods.ScoreV2 | OsuParsers.Enums.Mods.NoFail);
            if (((int)replay.Mods | allowedMods) != allowedMods)
            {
                await commandContext.RespondAsync("Мы не можем принять данный скор по причине того, что он поставлен с запрещенными на W.W.W модами. \nРазрешенные на W.W.W моды - `SD`, `PF`, `NF`\nСкор система: V2");
                return;
            }
            

            if (replay.ReplayTimestamp + TimeSpan.FromHours(3) > compitInfo.Deadline || replay.ReplayTimestamp + TimeSpan.FromHours(3) < compitInfo.StartDate)
            {
                await commandContext.RespondAsync("Мы не можем принять данный скор по причине того, что он поставлен не во время конкурса.");
                return;
            }

            bool correctMap = false;

            switch (compitProfile.Category)
            {
                case CompitCategories.Beginner:
                    if (replay.BeatmapMD5Hash == compitInfo.BeginnerMap.checksum)
                        correctMap = true;
                    break;

                case CompitCategories.Alpha:
                    if (replay.BeatmapMD5Hash == compitInfo.AlphaMap.checksum)
                        correctMap = true;
                    break;

                case CompitCategories.Beta:
                    if (replay.BeatmapMD5Hash == compitInfo.BetaMap.checksum)
                        correctMap = true;
                    break;

                case CompitCategories.Gamma:
                    if (replay.BeatmapMD5Hash == compitInfo.GammaMap.checksum)
                        correctMap = true;
                    break;

                case CompitCategories.Delta:
                    if (replay.BeatmapMD5Hash == compitInfo.DeltaMap.checksum)
                        correctMap = true;
                    break;

                case CompitCategories.Epsilon:
                    if (replay.BeatmapMD5Hash == compitInfo.EpsilonMap.checksum)
                        correctMap = true;
                    break;
            }

            if (!correctMap)
            {
                await commandContext.RespondAsync($"Мы не можем принять данный скор по причине того, что он поставлен не на той карте, которая выдана вашей категории {osuEnums.CategoryToString(wavMember.CompitionProfile.Category)}.");
                return;
            }

            string osuNickname = wavMember.OsuServers.FirstOrDefault(x => x.Server == compitProfile.Server).OsuNickname;
            if (replay.PlayerName != osuNickname)
            {
                await commandContext.RespondAsync($"Ваш никнейм не совпадает с автором скора. Если вы меняли никнейм, вызовите `sk!wmw recount`.");
                return;
            }

            //if (wavCompit.CheckScoreExists(replay.OnlineId.ToString()))
            //{
            //    await commandContext.RespondAsync($"Вы уже отправляли раннее данный скор.");
            //    return;
            //}

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Osu nickname: `{replay.PlayerName}`");
            sb.AppendLine($"Discord nickname: `{msg.Author.Username}`");
            sb.AppendLine($"Score: `{replay.ReplayScore:N0}`"); // Format: 123456789 -> 123 456 789
            sb.AppendLine($"Category: `{osuEnums.CategoryToString(wavMember.CompitionProfile.Category) ?? "No category"}`");
            sb.AppendLine($"Mods: `{osuEnums.ModsToString((WAV_Osu_NetApi.Models.Bancho.Mods)replay.Mods)}`");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithAuthor(msg.Author.Username, iconUrl: msg.Author.AvatarUrl)
                                                                 .WithTitle($"Added replay {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}")
                                                                 .WithUrl(attachment.Url)
                                                                 .WithDescription(sb.ToString())
                                                                 .AddField("OSR Link:", attachment.Url)
                                                                 .AddField("File name:", $"`{attachment.FileName}`")
                                                                 .WithTimestamp(DateTime.Now);

            DiscordChannel wavScoresChannel = guild.GetChannel(ulong.Parse(compitInfo.ScoresChannelUID));
            DiscordMessage scoreMessage = await wavScoresChannel.SendMessageAsync(new DiscordMessageBuilder()
                                                            .WithFile(new FileStream($"downloads/{fileName}", FileMode.Open))
                                                            .WithEmbed(embed));


            await compititionService.SubmitScore(new CompitScore()
            {
                DiscordUID = commandContext.User.Id.ToString(),
                DiscordNickname = $"{commandContext.User.Username}#{commandContext.User.Discriminator}",
                Nickname = osuNickname,
                Category = compitProfile.Category,
                Score = replay.ReplayScore,
                ScoreId = replay.OnlineId.ToString(),
                ScoreUrl = scoreMessage.Attachments.FirstOrDefault()?.Url
            });

            await commandContext.RespondAsync("Ваш скор был отправлен на рассмотрение. Спасибо за участие!");
        }
    }

}
