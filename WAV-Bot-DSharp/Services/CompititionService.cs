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

namespace WAV_Bot_DSharp.Services
{
    public class CompititionService : ICompititionService
    {
        private IWAVCompitProvider wavCompit;
        private IWAVMembersProvider wavMembers;
        private ShedulerService sheduler;

        private CompitInfo compititionInfo;

        private LeaderboardUpdateSheduledTask leaderboardUpdateTask;

        private DiscordClient client;
        private DiscordChannel leaderboardChannel;
        private DiscordChannel scoresChannel;

        public CompititionService(IWAVCompitProvider wavCompit,
                                  IWAVMembersProvider wavMembers,
                                  ShedulerService sheduler,
                                  DiscordClient client)
        {
            this.wavCompit = wavCompit;
            this.wavMembers = wavMembers;
            this.sheduler = sheduler;
            this.client = client;

            this.compititionInfo = wavCompit.GetCompitionInfo();

            if (compititionInfo.LeaderboardChannel is not null)
                leaderboardChannel = client.GetChannelAsync((ulong)compititionInfo.LeaderboardChannel).Result;

            if (compititionInfo.ScoresChannel is not null)
                scoresChannel = client.GetChannelAsync((ulong)compititionInfo.ScoresChannel).Result;

            this.leaderboardUpdateTask = new LeaderboardUpdateSheduledTask(this);

            // Запустить конкурс, если тот уже идет. Может произойти при перезапуске бота
            if (compititionInfo.IsRunning)
                InitCompitition();
        }

        public void InitCompitition()
        {

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
        /// <returns></returns>
        public CompitInfo GetStatus() => compititionInfo;

        public void UpdateLeaderboard()
        {
            throw new NotImplementedException();
        }
    }
}
