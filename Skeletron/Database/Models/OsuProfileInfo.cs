using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using OsuNET_Api.Models;

namespace Skeletron.Database.Models
{
    /// <summary>
    /// Информация о участнике WAV на конкретном osu сервере
    /// </summary>
    public class OsuProfileInfo
    {
        /// <summary>
        /// Название osu сервера
        /// </summary>
        public OsuServer Server { get; set; }

        /// <summary>
        /// ID пользователя
        /// </summary>
        public int OsuId { get; set; }

        /// <summary>
        /// Никнейм пользователя на сервере
        /// </summary>
        public string OsuNickname { get; set; }
    }
}
