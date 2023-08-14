using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsuNET_Api.Models.Bancho;

namespace Skeletron.Database.Models
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
        public string LeaderboardMessageUID { get; set; }

        /// <summary>
        /// ID текстового канала, где будет вестись лидерборд
        /// </summary>
        public string LeaderboardChannelUID { get; set; }

        /// <summary>
        /// ID текстового канала, куда будут отправляться все скоры
        /// </summary>
        public string ScoresChannelUID { get; set; }

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
        public Beatmap BeginnerMap { get; set; }

        /// <summary>
        /// ID карты для категории Alpha
        /// </summary>
        public Beatmap AlphaMap { get; set; }

        /// <summary>
        /// ID карты для категории Beta
        /// </summary>
        public Beatmap BetaMap { get; set; }

        /// <summary>
        /// ID карты для категории Gamma
        /// </summary>
        public Beatmap GammaMap { get; set; }

        /// <summary>
        /// ID карты для категории Delta
        /// </summary>
        public Beatmap DeltaMap { get; set; }

        /// <summary>
        /// ID карты для категории Epsilon
        /// </summary>
        public Beatmap EpsilonMap { get; set; }
    }
}
