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
        /// Никнейм пользователя на сервере
        /// </summary>
        public string Nickname { get; set; }
    }
}
