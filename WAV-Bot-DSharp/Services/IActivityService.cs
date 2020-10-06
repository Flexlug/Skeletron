using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using WAV_Bot_DSharp.Services.Entities;

namespace WAV_Bot_DSharp.Services
{
    /// <summary>
    /// Предоставляет доступ к сервису, отслеживающему активность пользователей
    /// </summary>
    public interface IActivityService
    {
        /// <summary>
        /// Обновить список пользователей и добавить недостающих в базу данных
        /// </summary>
        /// <returns>Количество добавленных пользователей</returns>
        public Task<int> UpdateCurrentUsers();

        /// <summary>
        /// Удалить лишние записи в базе данных
        /// </summary>
        /// <returns>Количество удалённых записей</returns>
        public Task<int> ExcludeAbsentUsers();

        /// <summary>
        /// Получить информацию об активности пользователей в пагинированной панели.
        /// </summary>
        /// <param name="page">Номер страницы</param>
        /// <returns></returns>
        public Task<List<UserInfo>> ViewActivityInfo(int page);

        /// <summary>
        /// Получить список AFK пользователей
        /// </summary>
        /// <returns>Список AFK пользователей</returns>
        public Task<List<UserInfo>> GetAFKUsers(int page);

        /// <summary>
        /// Удаляет пользователя из списка отслеживания
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        public Task RemoveUser(ulong user);

        /// <summary>
        /// Добавить пользователя в список отслеживания
        /// </summary>
        /// <param name="users">Uid пользователя</param>
        /// <returns>Если true, то в БД были изменения, иначе false</returns>
        public Task<bool> AddUser(ulong user);

        /// <summary>
        /// Получить информацию об активности пользователя
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <returns>Дату последней активности пользователя</returns>
        public Task<UserInfo> GetUser(ulong user);

        /// <summary>
        /// Вручную обновить ингформацию об активности пользователя. Последняя активность будет выставлена на текущую дату и время
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <returns></returns>
        public Task ManualUpdateToPresent(ulong user);

        /// <summary>
        /// Вручную обновить ингформацию об активности пользователя. Последняя активность будет выставлена на заданное дату и время
        /// </summary>
        /// <param name="user">Uid пользователя</param>
        /// <param name="dateTime">Дата и время, на которое необходимо обновить активность</param>
        /// <returns></returns>
        public Task ManualUpdate(ulong user, DateTime dateTime);

        /// <summary>
        /// Получить общее количество страниц
        /// </summary>
        /// <returns>Количество страниц в базе данных</returns>
        public Task<int> GetTotalPages();
    }
}
