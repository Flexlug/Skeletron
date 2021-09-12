using WAV_Bot_DSharp.Database.Models;

using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Database.Interfaces
{
    public interface IMembersProvider
    {
        /// <summary>
        /// Получить участника из БД (или добавить его, если таковой отсутсвует)
        /// </summary>
        /// <param name="uid">Discord id участника, добавляемого в БД</param>
        public WAVMember GetMember(string uid);

        /// <summary>
        /// Добавить или обновить данные о сервере, на котором играет участник
        /// </summary>
        /// <param name="uid">Uid участника</param>
        /// <param name="profile">Информация об osu! профиле.</param>
        public void AddOsuServerInfo(string uid, WAVMemberOsuProfileInfo profile);

        /// <summary>
        /// Получить информацию об osu! профиле участника WAV
        /// </summary>
        /// <param name="uid">Discord id участника WAV</param>
        /// <param name="server">Название сервера</param>
        /// <returns></returns>
        public WAVMemberOsuProfileInfo GetOsuProfileInfo(string uid, OsuServer server);

        /// <summary>
        /// Получить одного 
        /// </summary>
        /// <returns></returns>
        public WAVMember Next();
    }
}
