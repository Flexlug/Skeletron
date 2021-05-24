using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ulong Player { get; set; }

        /// <summary>
        /// Количество очков, набранных в скоре
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Ссылка на файл скора
        /// </summary>
        public string ScoreUrl { get; set; }
    }
}
