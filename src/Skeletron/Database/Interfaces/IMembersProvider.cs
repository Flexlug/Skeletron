using Skeletron.Database.Models;

using OsuNET_Api.Models;

namespace Skeletron.Database.Interfaces
{
    public interface IMembersProvider
    {
        /// <summary>
        /// Получить участника из БД (или добавить его, если таковой отсутсвует)
        /// </summary>
        /// <param name="uid">Discord id участника, добавляемого в БД</param>
        public WAVMembers GetMember(string uid);

        /// <summary>
        /// Добавить или обновить данные о сервере, на котором играет участник
        /// </summary>
        /// <param name="uid">Uid участника</param>
        /// <param name="profile">Информация об osu! профиле.</param>
        public void AddOsuServerInfo(string uid, OsuProfileInfo profile);

        /// <summary>
        /// Получить информацию об osu! профиле участника WAV
        /// </summary>
        /// <param name="uid">Discord id участника WAV</param>
        /// <param name="server">Название сервера</param>
        /// <returns></returns>
        public OsuProfileInfo GetOsuProfileInfo(string uid, OsuServer server);

        /// <summary>
        /// Получить одного 
        /// </summary>
        /// <returns></returns>
        public WAVMembers Next();
    }
}
