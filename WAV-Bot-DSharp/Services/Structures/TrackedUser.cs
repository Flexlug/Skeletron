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
        public int? BanchoId { get; set; }

        /// <summary>
        /// If bancho's user recent scores are being tracked
        /// </summary>
        public bool BanchoTrackRecent { get; set; }

        /// <summary>
        /// If bancho's user top scores are being tracked
        /// </summary>
        public bool BanchoTrackTop { get; set; }

        /// <summary>
        /// Gatari id
        /// </summary>
        public int? GatariId { get; set; }

        /// <summary>
        /// If gatari's user recent scores are being tracked
        /// </summary>
        public bool GatariTrackRecent { get; set; }

        /// <summary>
        /// If gatari's user top scores are being tracked
        /// </summary>
        public bool GatariTrackTop { get; set; }
    }
}
