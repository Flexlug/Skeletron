using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Database.Models;

using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Database.Interfaces
{
    public interface IWAVCompitProvider
    {
        /// <summary>
        /// Получить информацию об участии данного пользователя в конкурсах WAV
        /// </summary>
        /// <param name="uid">Discord id</param>
        /// <returns></returns>
        public WAVMemberCompitInfo GetParticipationInfo(ulong uid);

        /// <summary>
        /// Указать, что участник принял участие в конкурсе WAV
        /// </summary>
        /// <param name="uid">Discord id участника</param>
        public void SetMemberParticipated(ulong uid);

        /// <summary>
        /// Сбросить всю информацию об участии каждого человека в конкурсе
        /// </summary>
        public void ResetAllCompitInfo();

        /// <summary>
        /// Зарегистрировать участника как участника конкурса. 
        /// </summary>
        /// <param name="server">Название сервера, для которого нужно пересчитать скоры</param>
        public double RecountMember(ulong member, OsuServer server);
    }
}
