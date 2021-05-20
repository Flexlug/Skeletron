using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WAV_Bot_DSharp.Services.Models
{
    /// <summary>
    /// Представляет собой структуру, в виде которой хранится информация о пользователях в БД
    /// </summary>
    public class WAVMember 
    { 
        /// <summary>
        /// Uid пользователя
        /// </summary>
        public ulong Uid { get; set; }

        /// <summary>
        /// Список серверов, на которых зарегистрирован участник
        /// </summary>
        public List<WAVMemberOsuServerInfo> OsuServers { get; set; }

        /// <summary>
        /// Информация об участии в конкурсах
        /// </summary>
        public WAVMemberCompitInfo CompitionInfo { get; set; }

        /// <summary>
        /// Дата последней активности
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Количество очков активности
        /// </summary>
        public int ActivityPoints { get; set; }
    }
}
