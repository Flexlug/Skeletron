using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Database.Models;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface ICompititionService
    {
        /// <summary>
        /// Вернуть информацию о конкурсе
        /// </summary>
        public CompitInfo GetCompitInfo();

        /// <summary>
        /// Зарегистрировать участника в конкурсе - вычислить среднее из 5 топ скоров и присвоить роль
        /// </summary>
        /// <param name="member">Регистрируемый участник</param>
        /// <param name="osuInfo">Информация о профиле</param>
        public Task RegisterMember(DiscordMember member, WAVMemberOsuProfileInfo osuInfo);

        /// <summary>
        /// Выключить уведомления о конкурсе
        /// </summary>
        /// <param name="member">Участник, с которого нужно снять соответствующую роль</param>
        public Task DisableNotifications(DiscordMember member);

        /// <summary>
        /// Включить уведомления о конкурсе
        /// </summary>
        /// <param name="member">Участник, которому нужно присвоить соответствующую роль</param>
        public Task EnableNotifications(DiscordMember member, WAVMemberCompitProfile profile = null);

        public Task UpdateLeaderboard();
    }
}
