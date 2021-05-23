using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Database.Models
{
    /// <summary>
    /// Информация о участнике WAV на конкретном osu сервере
    /// </summary>
    public class WAVMemberOsuProfileInfo
    {
        public WAVMemberOsuProfileInfo(int id, OsuServer server)
        {
            Id = id;
            Server = server;
            RecentLast = DateTime.Now;
            BestLast = DateTime.Now;
        }

        /// <summary>
        /// Название osu сервера
        /// </summary>
        public OsuServer Server { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Состояние отслеживания недавних скоров
        /// </summary>
        public bool TrackRecent { get; set; } = false;

        /// <summary>
        /// Состояние отслеживания лучших скоров
        /// </summary>
        public bool TrackBest { get; set; } = false;

        /// <summary>
        /// Время, когда был зафиксирован последний скор (среди недавних)
        /// </summary>
        public DateTime? RecentLast { get; set; }

        /// <summary>
        /// Время, когда был зафиксирован последний лучший скор (среди топ-50)
        /// </summary>
        public DateTime? BestLast { get; set; }
    }
}
