using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Services.Models;

using WAV_Osu_NetApi;

using Microsoft.Extensions.Logging;
using WAV_Osu_NetApi.Models;
using WAV_Osu_NetApi.Models.Bancho;
using WAV_Osu_NetApi.Models.Gatari;

namespace WAV_Bot_DSharp.Services
{
    public class CompititionService : ICompititionService
    {
        private IWAVCompitProvider wavCompit;
        private IWAVMembersProvider wavMembers;
        private IShedulerService sheduler;

        private CompitInfo compititionInfo;

        private LeaderboardUpdateSheduledTask leaderboardUpdateTask;

        private DiscordGuild guild;
        private DiscordClient client;

        private DiscordChannel leaderboardChannel;
        private DiscordChannel scoresChannel;

        private BanchoApi bapi;
        private GatariApi gapi;

        private DiscordRole beginnerRole;
        private DiscordRole alphaRole;
        private DiscordRole betaRole;
        private DiscordRole gammaRole;
        private DiscordRole deltaRole;
        private DiscordRole epsilonRole;

        private ILogger<CompititionService> logger;

        public CompititionService(IWAVCompitProvider wavCompit,
                                  IWAVMembersProvider wavMembers,
                                  IShedulerService sheduler,
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

            this.bapi = bapi;
            this.gapi = gapi;

            this.compititionInfo = wavCompit.GetCompitionInfo();

            if (compititionInfo.LeaderboardChannel is not null)
                leaderboardChannel = client.GetChannelAsync((ulong)compititionInfo.LeaderboardChannel).Result;

            if (compititionInfo.ScoresChannel is not null)
                scoresChannel = client.GetChannelAsync((ulong)compititionInfo.ScoresChannel).Result;

            this.leaderboardUpdateTask = new LeaderboardUpdateSheduledTask(this);

            this.guild = guild;

            this.beginnerRole = guild.GetRole(830432931150692362);
            this.alphaRole = guild.GetRole(816610025258352641);
            this.betaRole = guild.GetRole(816609978240204821);
            this.gammaRole = guild.GetRole(816609883763376188);
            this.deltaRole = guild.GetRole(816609826301935627);
            this.epsilonRole = guild.GetRole(816609630359912468);

            this.logger.LogInformation("CompititionService loaded");

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
        }

        public void InitCompitition()
        {

        }

        /// <summary>
        /// Проверка выполнения всех условий для старта конкурса
        /// </summary>
        /// <returns></returns>
        public async Task<string> CompititionPreexecutionCheck()
        {
            StringBuilder sb = new StringBuilder();

            if (compititionInfo.BeginnerMapHash is null)
                sb.AppendLine("Не задана карта для категории Beginner");

            if (string.IsNullOrEmpty(compititionInfo.AlphaMapHash))
                sb.AppendLine("Не задана карта для категории Alpha");

            if (string.IsNullOrEmpty(compititionInfo.BetaMapHash))
                sb.AppendLine("Не задана карта для категории Beta");

            if (string.IsNullOrEmpty(compititionInfo.GammaMapHash))
                sb.AppendLine("Не задана карта для категории Gamma");

            if (string.IsNullOrEmpty(compititionInfo.DeltaMapHash))
                sb.AppendLine("Не задана карта для категории Delta");

            if (string.IsNullOrEmpty(compititionInfo.EpsilonMapHash))
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

            if (compititionInfo.LeaderboardChannel is null)
            {
                sb.AppendLine("Не задан канал для отображения лидерборда");
            }
            else
            {
                leaderboardChannel = await client.GetChannelAsync((ulong)compititionInfo.LeaderboardChannel);
                if (leaderboardChannel is null)
                    sb.AppendLine($"Не удалось получить доступ к каналу для лидерборда: {compititionInfo.LeaderboardChannel}");
            }


            if (compititionInfo.ScoresChannel is null)
            {
                sb.AppendLine("Не задан канал для сбора скачаных скоров");
            }
            else
            {
                scoresChannel = await client.GetChannelAsync((ulong)compititionInfo.ScoresChannel);
                if (leaderboardChannel is null)
                    sb.AppendLine($"Не удалось получить доступ к каналу для скачаных скоров: {compititionInfo.ScoresChannel}");
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
        public async Task SetLeaderboardChannel(ulong channel)
        {
            DiscordChannel lbChannel = await client.GetChannelAsync(channel);

            if (lbChannel is null)
                throw new NullReferenceException("Can't get access to channel");

            compititionInfo.LeaderboardChannel = lbChannel.Id;
            leaderboardChannel = lbChannel;

            UpdateCompitInfo();
        }

        /// <summary>
        /// Задать канал, куда будут отправляться скоры участников
        /// </summary>
        /// <param name="channel">ID текстового канала</param>
        public async Task SetScoresChannel(ulong channel)
        {
            DiscordChannel sChannel = await client.GetChannelAsync(channel);

            if (sChannel is null)
                throw new NullReferenceException("Can't get access to channel");

            compititionInfo.ScoresChannel = sChannel.Id;
            scoresChannel = sChannel;

            UpdateCompitInfo();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Зарегистрировать участника в конкурсе - вычислить среднее из 5 топ скоров и присвоить роль
        /// </summary>
        /// <param name="member">Регистрируемый участник</param>
        /// <param name="osuInfo">Информация о профиле</param>
        public async Task RegisterMember(DiscordMember member, WAVMemberOsuProfileInfo osuInfo)
        {
            double avgPP = await CalculateAvgPP(osuInfo.Id, osuInfo.Server);

            CompitCategories category;

            if (avgPP < 35)
                category = CompitCategories.Beginner;
            else
                if (avgPP < 100)
                category = CompitCategories.Alpha;
            else
                if (avgPP < 200)
                category = CompitCategories.Beta;
            else
                if (avgPP < 300)
                category = CompitCategories.Gamma;
            else
                if (avgPP < 500)
                category = CompitCategories.Delta;
            else
                category = CompitCategories.Epsilon;


            WAVMemberCompitProfile compitProfile = new WAVMemberCompitProfile()
            {
                AvgPP = avgPP,
                Category = category,
                NonGrata = false,
                Notifications = true,
                Server = osuInfo.Server
            };

            wavCompit.AddCompitProfile(member.Id, compitProfile);

            await EnableNotifications(member, compitProfile);
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

        private async Task RemoveWrongNotificationRoles(DiscordMember member, CompitCategories category)
        {
            if (member.Roles.Contains(beginnerRole))
                if (category != CompitCategories.Beginner)
                    await member.RevokeRoleAsync(beginnerRole);

            if (member.Roles.Contains(alphaRole))
                if (category != CompitCategories.Alpha)
                    await member.RevokeRoleAsync(alphaRole);

            if (member.Roles.Contains(betaRole))
                if (category != CompitCategories.Beta)
                    await member.RevokeRoleAsync(betaRole);

            if (member.Roles.Contains(gammaRole))
                if (category != CompitCategories.Gamma)
                    await member.RevokeRoleAsync(gammaRole);

            if (member.Roles.Contains(deltaRole))
                if (category != CompitCategories.Delta)
                    await member.RevokeRoleAsync(deltaRole);

            if (member.Roles.Contains(epsilonRole))
                if (category != CompitCategories.Epsilon)
                await member.RevokeRoleAsync(epsilonRole);

        }

        /// <summary>
        /// Включить уведомления о конкурсе
        /// </summary>
        /// <param name="member">Участник, которому нужно присвоить соответствующую роль</param>
        public async Task EnableNotifications(DiscordMember member, WAVMemberCompitProfile profile = null)
        {
            WAVMemberCompitProfile compitProfile = profile ?? wavCompit.GetCompitProfile(member.Id);
            if (compitProfile is null)
            {
                throw new NullReferenceException($"Couldn't get compitition profile for {member}");
            }

            switch (compitProfile.Category)
            {
                case CompitCategories.Beginner:
                    await member.GrantRoleAsync(beginnerRole);
                    break;

                case CompitCategories.Alpha:
                    await member.GrantRoleAsync(alphaRole);
                    break;

                case CompitCategories.Beta:
                    await member.GrantRoleAsync(betaRole);
                    break;

                case CompitCategories.Gamma:
                    await member.GrantRoleAsync(gammaRole);
                    break;

                case CompitCategories.Delta:
                    await member.GrantRoleAsync(deltaRole);
                    break;

                case CompitCategories.Epsilon:
                    await member.GrantRoleAsync(epsilonRole);
                    break;
            }

            await RemoveWrongNotificationRoles(member, compitProfile.Category);
        }

        /// <summary>
        /// Выключить уведомления о конкурсе
        /// </summary>
        /// <param name="member">Участник, с которого нужно снять соответствующую роль</param>
        public async Task DisableNotifications(DiscordMember member)
        {
            WAVMemberCompitProfile compitProfile = wavCompit.GetCompitProfile(member.Id);
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
