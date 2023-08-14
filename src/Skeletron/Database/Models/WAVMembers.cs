using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Newtonsoft.Json;

namespace Skeletron.Database.Models
{
    /// <summary>
    /// Представляет собой структуру, в виде которой хранится информация о пользователях в БД
    /// </summary>
    public class WAVMembers 
    { 
        public WAVMembers(string uid)
        {
            DiscordUID = uid;
            OsuServers = new List<OsuProfileInfo>();
            CompitionProfile = null;
            LastActivity = DateTime.Now;
            ActivityPoints = 0;
        }

        /// <summary>
        /// Uid пользователя
        /// </summary>
        public string DiscordUID { get; set; }

        /// <summary>
        /// Список серверов, на которых зарегистрирован участник
        /// </summary>
        public List<OsuProfileInfo> OsuServers { get; set; }

        /// <summary>
        /// Информация об участии в конкурсах
        /// </summary>
        public CompitionProfile CompitionProfile { get; set; }

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
