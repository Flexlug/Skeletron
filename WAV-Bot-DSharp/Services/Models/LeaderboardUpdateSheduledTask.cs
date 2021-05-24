using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Services.Interfaces;

namespace WAV_Bot_DSharp.Services.Models
{
    /// <summary>
    /// Задача на обновление лидерборда во время конкурса
    /// </summary>
    public class LeaderboardUpdateSheduledTask : SheduledTask
    {
        private ICompititionService compititionService { get; set; }

        public LeaderboardUpdateSheduledTask(ICompititionService compititionService)
        {
            this.compititionService = compititionService;

            this.Interval = TimeSpan.FromMinutes(1);
            this.Repeat = true;
            this.Action = () =>
            {
                this.compititionService.UpdateLeaderboard();
            };
        }
    }
}
