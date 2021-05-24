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
        /// ID карты для категории Beginner
        /// </summary>
        public int? BeginnerMapId { get; set; }

        /// <summary>
        /// ID карты для категории Alpha
        /// </summary>
        public int? AlphaMapId { get; set; }

        /// <summary>
        /// ID карты для категории Beta
        /// </summary>
        public int? BetaMapId { get; set; }

        /// <summary>
        /// ID карты для категории Gamma
        /// </summary>
        public int? GammaMapId { get; set; }

        /// <summary>
        /// ID карты для категории Delta
        /// </summary>
        public int? DeltaMapId { get; set; }

        /// <summary>
        /// ID карты для категории Epsilon
        /// </summary>
        public int? EpsilonMapId { get; set; }
    }
}
