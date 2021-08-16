using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Osu_NetApi.Models.Bancho;

namespace WAV_Bot_DSharp.Database.Models
{
    public class OfferedMap
    {
        /// <summary>
        /// ID beatmap-ы
        /// </summary>
        public int BeatmapId { get; set; }

        /// <summary>
        /// Данная карта была предложена администрацией сервера в качестве 
        /// </summary>
        public bool AdminMap { get; set; }

        /// <summary>
        /// Предлагаемая карта
        /// </summary>
        public Beatmap Beatmap { get; set; }

        /// <summary>
        /// Категория, для которой предлагается карта
        /// </summary>
        public CompitCategories Category { get; set; }

        /// <summary>
        /// Discord ID предложившего карту
        /// </summary>
        public string SuggestedBy { get; set; }

        /// <summary>
        /// Проголосовавшие за карту
        /// </summary>
        public List<string> Votes {  get; set; }
    }
}
