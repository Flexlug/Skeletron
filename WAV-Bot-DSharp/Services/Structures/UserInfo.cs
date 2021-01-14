using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WAV_Bot_DSharp.Services.Structures
{
    /// <summary>
    /// Представляет собой структуру, в виде которой хранится информация о пользователях в БД
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// DataBase ID
        /// </summary>
        [Key]
        public ulong Id { get; set; }

        /// <summary>
        /// Uid пользователя
        /// </summary>
        public ulong Uid { get; set; }

        /// <summary>
        /// Дата последней активности
        /// </summary>
        public DateTime LastActivity { get; set; }
    }
}
