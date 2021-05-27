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

        /// <summary>
        /// Проверка выполнения всех условий для старта конкурса
        /// </summary>
        public Task<string> CompititionPreexecutionCheck();

        /// <summary>
        /// Задать для категории карту
        /// </summary>
        /// <param name="mapUrl">Ссылка на карту (только bancho)</param>
        /// <param name="category">Название категории</param>
        /// <returns></returns>
        public Task<bool> SetMap(string mapUrl, string category);

        /// <summary>
        /// Задать канал, в котором будет лидерборд
        /// </summary>
        /// <param name="channel">ID текстового канала</param>
        public Task<bool> SetLeaderboardChannel(string channel);

        /// <summary>
        /// Задать канал, куда будут отправляться скоры участников
        /// </summary>
        /// <param name="channel">ID текстового канала</param>
        public Task<bool> SetScoresChannel(string channel);

        /// <summary>
        /// Задать дату окончания конкурса
        /// </summary>
        /// <param name="deadline">Дата, когда конкурс должен закончиться</param>
        /// <returns></returns>
        public Task SetDeadline(DateTime deadline);

        /// <summary>
        /// Добавить скор и обновить лидерборд
        /// </summary>
        /// <param name="score">Новый скор</param>
        /// <returns></returns>
        public Task SubmitScore(CompitScore score);

        /// <summary>
        /// Остановить конкурс
        /// </summary>
        /// <returns></returns>
        public Task StopCompition();

        /// <summary>
        /// Запустить конкурс. Создать лидерборд.
        /// </summary>
        public Task InitCompitition();

        /// <summary>
        /// Обновить лидерборд
        /// </summary>
        /// <returns></returns>
        public Task UpdateLeaderboard();

        /// <summary>
        /// Задать статус non-grata для заданного пользователя
        /// </summary>
        /// <param name="member">Discord UID пользователя</param>
        /// <param name="toggle">True/False</param>
        public Task SetNonGrata(DiscordMember member, bool toggle);
    }
}
