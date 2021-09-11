using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Services.Models;

using WAV_Osu_NetApi;
using WAV_Osu_NetApi.Models;
using WAV_Osu_NetApi.Models.Bancho;
using WAV_Osu_NetApi.Models.Gatari;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Services
{
    public class CompititionService : ICompititionService
    {
        private IWAVCompitProvider wavCompit;
        private IWAVMembersProvider wavMembers;

        private IShedulerService sheduler;
        private SheduledTask recountTask;

        private CompitInfo compititionInfo;

        private OsuRegex osuRegex;
        private OsuEnums osuEnums;
        private OsuEmbed osuEmbed;

        private LeaderboardUpdateSheduledTask leaderboardUpdateTask;

        private DiscordGuild guild;
        private DiscordClient client;

        private DiscordChannel leaderboardChannel;
        private DiscordChannel scoresChannel;
        private DiscordMessage leaderboardMessage;

        private BanchoApi bapi;
        private GatariApi gapi;

        private DiscordRole beginnerRole;
        private DiscordRole alphaRole;
        private DiscordRole betaRole;
        private DiscordRole gammaRole;
        private DiscordRole deltaRole;
        private DiscordRole epsilonRole;

        private DiscordRole nonGrata;

        private ILogger<CompititionService> logger;

        public CompititionService(IWAVCompitProvider wavCompit,
                                  IWAVMembersProvider wavMembers,
                                  IShedulerService sheduler,
                                  OsuRegex osuRegex,
                                  OsuEnums osuEnums,
                                  OsuEmbed osuEmbed,
                                  DiscordClient client,
                                  DiscordGuild guild,
                                  BanchoApi bapi,
                                  GatariApi gapi,
                                  ILogger<CompititionService> logger)
        {
            this.logger = logger;

            this.wavCompit = wavCompit;
            this.wavMembers = wavMembers;
            this.sheduler = sheduler;
            this.client = client;

            this.osuRegex = osuRegex;
            this.osuEnums = osuEnums;
            this.osuEmbed = osuEmbed;

            this.bapi = bapi;
            this.gapi = gapi;

            this.compititionInfo = wavCompit.GetCompitionInfo();

            this.leaderboardUpdateTask = new LeaderboardUpdateSheduledTask(this);

            this.guild = guild;

            this.beginnerRole = guild.GetRole(830432931150692362);
            this.alphaRole = guild.GetRole(816610025258352641);
            this.betaRole = guild.GetRole(816609978240204821);
            this.gammaRole = guild.GetRole(816609883763376188);
            this.deltaRole = guild.GetRole(816609826301935627);
            this.epsilonRole = guild.GetRole(816609630359912468);
            this.nonGrata = guild.GetRole(834475420073197579);

            this.logger.LogInformation("CompititionService loaded");

            recountTask = new SheduledTask("RecountTask",
                                           () => RecountTask(),
                                           TimeSpan.FromMinutes(1),
                                           true);

            // Запустить конкурс, если тот уже идет. Может произойти при перезапуске бота
            if (compititionInfo.IsRunning)
            {
                this.logger.LogInformation("Detected compitition is running. Attempting initialize compitition...");

                string checkRes = CompititionPreexecutionCheck().Result;
                if (checkRes == "done")
                    InitCompitition();
                else
                    logger.LogCritical($"Compitition is running, but preexecution check is not passed! {checkRes}");
            }
            else
            {
                StartRecountTask();
            }
        }

        private async Task StartRecountTask() 
        {
            if (!sheduler.FetchTask(recountTask))
            {
                sheduler.AddTask(recountTask);
                logger.LogInformation("Added SheduledTask recountTask");
            }
            else
            {
                logger.LogInformation("Can't add RecountTask, because it already exists");
            }
        }

        private async Task StopRecountTask()
        {
            if (sheduler.FetchTask(recountTask))
            {
                sheduler.RemoveTask(recountTask);
                logger.LogInformation("Removed SheduledTask recountTask");
            }
            else
            {
                logger.LogInformation("Can't delete RecountTask. No task");
            }
        }

        /// <summary>
        /// Запустить конкурс. Создать лидерборд.
        /// </summary>
        public async Task InitCompitition()
        {
            await StopRecountTask();

            if (!string.IsNullOrEmpty(compititionInfo.LeaderboardMessageUID))
            {
                leaderboardMessage = await leaderboardChannel.GetMessageAsync(ulong.Parse(compititionInfo.LeaderboardMessageUID)) ??
                                     await leaderboardChannel.SendMessageAsync(osuEmbed.ScoresToLeaderBoard(compititionInfo,
                                                                                                            wavCompit.GetCategoryBestScores(CompitCategory.Beginner),
                                                                                                            wavCompit.GetCategoryBestScores(CompitCategory.Alpha),
                                                                                                            wavCompit.GetCategoryBestScores(CompitCategory.Beta),
                                                                                                            wavCompit.GetCategoryBestScores(CompitCategory.Gamma),
                                                                                                            wavCompit.GetCategoryBestScores(CompitCategory.Delta),
                                                                                                            wavCompit.GetCategoryBestScores(CompitCategory.Epsilon)));
            }
            else
            {
                leaderboardMessage = await leaderboardChannel.SendMessageAsync(osuEmbed.ScoresToLeaderBoard(compititionInfo));
                compititionInfo.StartDate = DateTime.Now;
            }

            compititionInfo.LeaderboardMessageUID = leaderboardMessage.Id.ToString();
            compititionInfo.IsRunning = true;

            UpdateCompitInfo();
        }

        public void RecountTask()
        {
            var member = wavMembers.Next();

            if (member.OsuServers.Count == 0)
            {
                logger.LogDebug($"Skipping recount for {member.DiscordUID} - no osu! servers");
                return;
            }

            if (member.CompitionProfile is null)
            {
                logger.LogDebug($"Skipping recount for {member.DiscordUID} - no registration");
                return;
            }

            OsuServer server = member.CompitionProfile.Server;

            WAVMemberOsuProfileInfo profileInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);

            if (profileInfo is null)
            {
                logger.LogDebug($"Skipping recount for {member.DiscordUID} - no default osu! server");
                return;
            }

            DiscordMember dMember = null;
            try
            {
                dMember = guild.GetMemberAsync(ulong.Parse(member.DiscordUID)).Result;
            }
            catch(DSharpPlus.Exceptions.NotFoundException)
            {
                logger.LogWarning($"Skipping recount for {member?.DiscordUID} osu: {profileInfo?.OsuNickname} - couldn't get DiscordMember");
                return;
            }
            catch(AggregateException)
            {
                logger.LogWarning($"Skipping recount for {member?.DiscordUID} osu: {profileInfo?.OsuNickname} - couldn't get DiscordMember");
                return;
            }
            catch(Exception e)
            {
                logger.LogError(e, $"Unexpected exception in RecountTask for {member?.DiscordUID} osu: {profileInfo?.OsuNickname}");
                return;
            }

            RecountMember(dMember, profileInfo, member.CompitionProfile);
            logger.LogInformation($"Recounted {dMember.Username}");
        }

        public async Task StopCompition()
        {
            compititionInfo.IsRunning = false;
            compititionInfo.LeaderboardMessageUID = string.Empty;

            await StartRecountTask();

            UpdateCompitInfo();
        }

        public async Task SubmitScore(CompitScore score)
        {
            wavCompit.SubmitScore(score);
            await UpdateLeaderboard();
        }

        /// <summary>
        /// Задать для категории карту
        /// </summary>
        /// <param name="mapUrl">Ссылка на карту (только bancho)</param>
        /// <param name="category">Название категории</param>
        public async Task<bool> SetMap(string mapUrl, string category)
        {
            Tuple<int, int> mapInfo = osuRegex.GetBMandBMSIdFromBanchoUrl(mapUrl);
            if (mapInfo is null)
                return false;

            Beatmap beatmap = bapi.GetBeatmap(mapInfo.Item2);
            if (beatmap is null)
                return false;

            CompitCategory? compitCategory = osuEnums.StringToCategory(category);
            if (compitCategory is null)
                return false;

            switch (compitCategory)
            {
                case CompitCategory.Beginner:
                    compititionInfo.BeginnerMap = beatmap;
                    break;

                case CompitCategory.Alpha:
                    compititionInfo.AlphaMap = beatmap;
                    break;

                case CompitCategory.Beta:
                    compititionInfo.BetaMap = beatmap;
                    break;

                case CompitCategory.Gamma:
                    compititionInfo.GammaMap = beatmap;
                    break;

                case CompitCategory.Delta:
                    compititionInfo.DeltaMap = beatmap;
                    break;

                case CompitCategory.Epsilon:
                    compititionInfo.EpsilonMap = beatmap;
                    break;

                default:
                    return false;
            }

            UpdateCompitInfo();

            return true;
        }

        public async Task SetDeadline(DateTime deadline)
        {
            compititionInfo.Deadline = deadline;
            UpdateCompitInfo();
        }

        /// <summary>
        /// Проверка выполнения всех условий для старта конкурса
        /// </summary>
        /// <returns></returns>
        public async Task<string> CompititionPreexecutionCheck()
        {
            StringBuilder sb = new StringBuilder();

            if (compititionInfo.BeginnerMap is null)
                sb.AppendLine("Не задана карта для категории Beginner");

            if (compititionInfo.AlphaMap is null)
                sb.AppendLine("Не задана карта для категории Alpha");

            if (compititionInfo.BetaMap is null)
                sb.AppendLine("Не задана карта для категории Beta");

            if (compititionInfo.GammaMap is null)
                sb.AppendLine("Не задана карта для категории Gamma");

            if (compititionInfo.DeltaMap is null)
                sb.AppendLine("Не задана карта для категории Delta");

            if (compititionInfo.EpsilonMap is null)
                sb.AppendLine("Не задана карта для категории Epsilon");


            if (compititionInfo.Deadline is null)
            {
                sb.AppendLine("Не задана дата окончания конкурса");
            }
            else
            {
                if (compititionInfo.Deadline < DateTime.Now)
                    sb.AppendLine($"Некорректная дата окончания конкурса: {compititionInfo.Deadline}.");
            }

            if (string.IsNullOrEmpty(compititionInfo.LeaderboardChannelUID))
            {
                sb.AppendLine("Не задан канал для отображения лидерборда");
            }
            else
            {
                leaderboardChannel = await client.GetChannelAsync(ulong.Parse(compititionInfo.LeaderboardChannelUID));
                if (leaderboardChannel is null)
                    sb.AppendLine($"Не удалось получить доступ к каналу для лидерборда: {compititionInfo.LeaderboardChannelUID}");
            }


            if (string.IsNullOrEmpty(compititionInfo.ScoresChannelUID)) 
            { 
                sb.AppendLine("Не задан канал для сбора скачаных скоров");
            }
            else
            {
                scoresChannel = await client.GetChannelAsync(ulong.Parse(compititionInfo.ScoresChannelUID));
                if (scoresChannel is null)
                    sb.AppendLine($"Не удалось получить доступ к каналу для скачаных скоров: {compititionInfo.ScoresChannelUID}");
            }

            if (sb.Length != 0)
                return sb.ToString();
            else
                return "done";
        }

        /// <summary>
        /// Задать канал, в котором будет лидерборд
        /// </summary>
        /// <param name="channel">ID текстового канала</param>
        public async Task<bool> SetLeaderboardChannel(string channel)
        {
            DiscordChannel lbChannel = await client.GetChannelAsync(ulong.Parse(channel));

            if (lbChannel is null)
                return false;

            compititionInfo.LeaderboardChannelUID = lbChannel.Id.ToString();
            leaderboardChannel = lbChannel;

            UpdateCompitInfo();

            return true;
        }

        /// <summary>
        /// Задать канал, куда будут отправляться скоры участников
        /// </summary>
        /// <param name="channel">ID текстового канала</param>
        public async Task<bool> SetScoresChannel(string channel)
        {
            DiscordChannel sChannel = await client.GetChannelAsync(ulong.Parse(channel));

            if (sChannel is null)
                return false;

            compititionInfo.ScoresChannelUID = sChannel.Id.ToString();
            scoresChannel = sChannel;

            UpdateCompitInfo();

            return true;
        }

        /// <summary>
        /// Перезаписать информацию о конкурсе в БД
        /// </summary>
        private void UpdateCompitInfo() => wavCompit.SetCompitionInfo(compititionInfo);

        /// <summary>
        /// Вернуть информацию о конкурсе
        /// </summary>
        public CompitInfo GetCompitInfo() => compititionInfo;

        /// <summary>
        /// Обновить лидерборд
        /// </summary>
        public async Task UpdateLeaderboard()
        {
            DiscordEmbed newEmbed = osuEmbed.ScoresToLeaderBoard(compititionInfo,
                                                                 wavCompit.GetCategoryBestScores(CompitCategory.Beginner),
                                                                 wavCompit.GetCategoryBestScores(CompitCategory.Alpha),
                                                                 wavCompit.GetCategoryBestScores(CompitCategory.Beta),
                                                                 wavCompit.GetCategoryBestScores(CompitCategory.Gamma),
                                                                 wavCompit.GetCategoryBestScores(CompitCategory.Delta),
                                                                 wavCompit.GetCategoryBestScores(CompitCategory.Epsilon));

            await leaderboardMessage.ModifyAsync(embed: newEmbed);
        }

        /// <summary>
        /// Зарегистрировать участника в конкурсе - вычислить среднее из 5 топ скоров и присвоить роль
        /// </summary>
        /// <param name="user">Регистрируемый участник</param>
        /// <param name="osuInfo">Информация о профиле</param>
        public async Task RegisterMember(DiscordUser user, WAVMemberOsuProfileInfo osuInfo)
        {
            double avgPP = await CalculateAvgPP(osuInfo.OsuId, osuInfo.Server);

            WAVMemberCompitProfile compitProfile = new WAVMemberCompitProfile()
            {
                AvgPP = avgPP,
                Category = PPToCategory(avgPP),
                NonGrata = false,
                Notifications = true,
                Server = osuInfo.Server
            };

            wavCompit.AddCompitProfile(user.Id.ToString(), compitProfile);

            await EnableNotifications(user, compitProfile);
        }

        /// <summary>
        /// Пересчитать PP для заданного пользователя
        /// </summary>
        /// <returns></returns>
        public async Task RecountMember(DiscordMember user, WAVMemberOsuProfileInfo osuProfile, WAVMemberCompitProfile oldCompitProfile)
        {
            double avgPP = await CalculateAvgPP(osuProfile.OsuId, osuProfile.Server);

            logger.LogDebug($"avgPP for {user.Username}: {avgPP}");
            WAVMemberCompitProfile compitProfile = new WAVMemberCompitProfile()
            {
                AvgPP = avgPP,
                Category = PPToCategory(avgPP),
                NonGrata = oldCompitProfile.NonGrata,
                Notifications = oldCompitProfile.Notifications,
                Server = oldCompitProfile.Server
            };

            if (compitProfile.Category != oldCompitProfile.Category)
            {
                logger.LogDebug($"Category upgrade for {user.Username} – {compitProfile.Category}");
                if (compitProfile.Notifications)
                    await SendNewCategoryDMNotification(user, compitProfile);
            }

            wavCompit.AddCompitProfile(user.Id.ToString(), compitProfile);

            await EnableNotifications(user, compitProfile);
        }

        public async Task SendNewCategoryDMNotification(DiscordMember user, WAVMemberCompitProfile compitProfile)
        {
            logger.LogDebug($"Sent DM notification to {user.Username} about new category – {compitProfile.Category}");
            DiscordDmChannel channel = await user.CreateDmChannelAsync();
            await channel.SendMessageAsync(new DiscordEmbedBuilder()
                .WithTitle($"Вы перешли в следующую категорию – {compitProfile.Category} :partying_face:")
                .WithDescription($"Вы набрали достаточно PP для того, чтобы перейти в следующую категорию. Поздравляем!")
                .WithFooter("Не хотите получать уведомления? Воспользуйтесь командой `sk!www notify false`.")
                .Build());
        }

        /// <summary>
        /// Высчитать средний PP из первых 5 топ скоров
        /// </summary>
        /// <param name="id">ID osu! профиля</param>
        /// <param name="server">osu! сервер, на котором находится профиль</param>
        /// <returns></returns>
        public async Task<double> CalculateAvgPP(int id, OsuServer server)
        {
            double avgPP = 0;

            switch (server)
            {
                case OsuServer.Bancho:
                    List<Score> scores = bapi.GetUserBestScores(id, 5);

                    if (scores is null || scores.Count == 0)
                        throw new Exception("Couldn't get best scores for this user");

                    foreach (var score in scores)
                        avgPP += (double)score.pp;

                    break;

                case OsuServer.Gatari:
                    List<GScore> gscores = gapi.GetUserBestScores(id, 5);

                    if (gscores is null || gscores.Count == 0)
                        throw new Exception("Couldn't get best scores for this user");

                    foreach (var gscore in gscores)
                        avgPP += (double)gscore.pp;

                    break;
            }

            avgPP /= 5;
            return avgPP;
        }

        public CompitCategory PPToCategory(double avgPP)
        {
            CompitCategory category;

            if (avgPP < 35)
                category = CompitCategory.Beginner;
            else
                if (avgPP < 100)
                category = CompitCategory.Alpha;
            else
                if (avgPP < 200)
                category = CompitCategory.Beta;
            else
                if (avgPP < 300)
                category = CompitCategory.Gamma;
            else
                if (avgPP < 500)
                category = CompitCategory.Delta;
            else
                category = CompitCategory.Epsilon;

            return category;
        }

        public async Task SetNonGrata(DiscordUser user, bool toggle)
        {
            DiscordMember member = await guild.GetMemberAsync(user.Id);
            wavCompit.SetNonGrata(user.Id.ToString(), toggle);

            if (toggle)
            {
                await member.GrantRoleAsync(nonGrata);
            }
            else
            {
                if (member.Roles.Contains(nonGrata))
                    await member.RevokeRoleAsync(nonGrata);
            }
        }

        private async Task RemoveWrongNotificationRoles(DiscordUser user, CompitCategory category)
        {
            DiscordMember member = await guild.GetMemberAsync(user.Id);
            if (member.Roles.Contains(beginnerRole))
                if (category != CompitCategory.Beginner)
                    await member.RevokeRoleAsync(beginnerRole);

            if (member.Roles.Contains(alphaRole))
                if (category != CompitCategory.Alpha)
                    await member.RevokeRoleAsync(alphaRole);

            if (member.Roles.Contains(betaRole))
                if (category != CompitCategory.Beta)
                    await member.RevokeRoleAsync(betaRole);

            if (member.Roles.Contains(gammaRole))
                if (category != CompitCategory.Gamma)
                    await member.RevokeRoleAsync(gammaRole);

            if (member.Roles.Contains(deltaRole))
                if (category != CompitCategory.Delta)
                    await member.RevokeRoleAsync(deltaRole);

            if (member.Roles.Contains(epsilonRole))
                if (category != CompitCategory.Epsilon)
                await member.RevokeRoleAsync(epsilonRole);

        }

        /// <summary>
        /// Включить уведомления о конкурсе
        /// </summary>
        /// <param name="member">Участник, которому нужно присвоить соответствующую роль</param>
        public async Task EnableNotifications(DiscordUser user, WAVMemberCompitProfile profile = null)
        {
            DiscordMember member = await guild.GetMemberAsync(user.Id);

            WAVMemberCompitProfile compitProfile = profile ?? wavCompit.GetCompitProfile(member.Id.ToString());
            if (compitProfile is null)
            {
                throw new NullReferenceException($"Couldn't get compitition profile for {member}");
            }

            if (!compitProfile.Notifications)
            {
                logger.LogInformation("Notifications for this user are disabled");
                return;
            }

            switch (compitProfile.Category)
            {
                case CompitCategory.Beginner:
                    await member.GrantRoleAsync(beginnerRole);
                    break;

                case CompitCategory.Alpha:
                    await member.GrantRoleAsync(alphaRole);
                    break;

                case CompitCategory.Beta:
                    await member.GrantRoleAsync(betaRole);
                    break;

                case CompitCategory.Gamma:
                    await member.GrantRoleAsync(gammaRole);
                    break;

                case CompitCategory.Delta:
                    await member.GrantRoleAsync(deltaRole);
                    break;

                case CompitCategory.Epsilon:
                    await member.GrantRoleAsync(epsilonRole);
                    break;
            }

            await RemoveWrongNotificationRoles(member, compitProfile.Category);
        }

        /// <summary>
        /// Выключить уведомления о конкурсе
        /// </summary>
        /// <param name="member">Участник, с которого нужно снять соответствующую роль</param>
        public async Task DisableNotifications(DiscordUser user)
        {
            DiscordMember member = await guild.GetMemberAsync(user.Id);

            WAVMemberCompitProfile compitProfile = wavCompit.GetCompitProfile(member.Id.ToString());
            if (compitProfile is null)
            {
                throw new NullReferenceException($"Couldn't get compitition profile for {member}");
            }

            if (member.Roles.Contains(beginnerRole))
                await member.RevokeRoleAsync(beginnerRole);

            if (member.Roles.Contains(alphaRole))
                await member.RevokeRoleAsync(alphaRole);

            if (member.Roles.Contains(betaRole))
                await member.RevokeRoleAsync(betaRole);

            if (member.Roles.Contains(gammaRole))
                await member.RevokeRoleAsync(gammaRole);

            if (member.Roles.Contains(deltaRole))
                await member.RevokeRoleAsync(deltaRole);

            if (member.Roles.Contains(epsilonRole))
                await member.RevokeRoleAsync(epsilonRole);
        }
    }
}
