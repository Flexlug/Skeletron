using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsuNET_Api.Models;

namespace Skeletron.Database.Models
{
    public class CompitionProfile
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
        /// Желает ли человек получать уведомления о конкурсе
        /// </summary>
        public bool Notifications { get; set; } = false;

        /// <summary>
        /// osu! сервер, для которого высчитан средний PP
        /// </summary>
        public OsuServer Server { get; set; }

        /// <summary>
        /// Категория, в которую определен участник
        /// </summary>
        public CompitCategory Category { get; set; }
    }
}
