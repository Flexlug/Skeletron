using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Database.Models
{
    public class WAVMemberCompitProfile
    {
        /// <summary>
        /// Забанен ли пользователь на конкурсе
        /// </summary>
        public bool NonGrata { get; set; } = false;

        /// <summary>
        /// Средний PP
        /// </summary>
        public double AvgPP { get; set; }

        /// <summary>
        /// osu! сервер, для которого высчитан средний PP
        /// </summary>
        public OsuServer Server { get; set; }

        /// <summary>
        /// Категория, в которую определен участник
        /// </summary>
        public CompitCategories Category { get; set; }
    }
}
