using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WAV_Bot_DSharp.Services.Structures
{
    public class TrackedUser
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
        /// Bancho id
        /// </summary>
        public int? banchoId { get; set; }

        /// <summary>
        /// Gatari id
        /// </summary>
        public int? gatariId { get; set; }
    }
}
