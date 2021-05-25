using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Database.Models
{
    /// <summary>
    /// Общая информация о конкурсе
    /// </summary>
    public class CompitInfo
    {
        /// <summary>
        /// Запущен ли сейчас конкурс
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// ID сообщения с лидербордом
        /// </summary>
        public ulong? LeaderboardMessage { get; set; }

        /// <summary>
        /// ID текстового канала, где будет вестись лидерборд
        /// </summary>
        public ulong? LeaderboardChannel { get; set; }

        /// <summary>
        /// ID текстового канала, куда будут отправляться все скоры
        /// </summary>
        public ulong? ScoresChannel { get; set; }

        /// <summary>
        /// Дата окончания конкурса
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Дата начала конкурса
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// ID карты для категории Beginner
        /// </summary>
        public string BeginnerMapHash { get; set; }

        /// <summary>
        /// ID карты для категории Alpha
        /// </summary>
        public string AlphaMapHash { get; set; }

        /// <summary>
        /// ID карты для категории Beta
        /// </summary>
        public string BetaMapHash { get; set; }

        /// <summary>
        /// ID карты для категории Gamma
        /// </summary>
        public string GammaMapHash { get; set; }

        /// <summary>
        /// ID карты для категории Delta
        /// </summary>
        public string DeltaMapHash { get; set; }

        /// <summary>
        /// ID карты для категории Epsilon
        /// </summary>
        public string EpsilonMapHash { get; set; }
    }
}
