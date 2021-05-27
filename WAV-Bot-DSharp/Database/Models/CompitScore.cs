using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WAV_Bot_DSharp.Database.Models
{
    /// <summary>
    /// Информация о скоре пользователя
    /// </summary>
    public class CompitScore
    {
        /// <summary>
        /// Discord ID пользователя
        /// </summary>
        public string DiscordUID { get; set; }

        /// <summary>
        /// Никнейм osu! профиля
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Категория скора
        /// </summary>
        public CompitCategories Category { get; set; }

        /// <summary>
        /// Количество очков, набранных в скоре
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// ID скора
        /// </summary>
        public string ScoreId { get; set; }

        /// <summary>
        /// Ссылка на файл скора
        /// </summary>
        public string ScoreUrl { get; set; }
    }
}
