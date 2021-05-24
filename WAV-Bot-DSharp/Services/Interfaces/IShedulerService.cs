using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Services.Models;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface IShedulerService
    {
        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="task">Планируемая задача</param>
        public void AddTask(SheduledTask task);

        /// <summary>
        /// Получить информацию о запланированных задачах, если таковые имеются
        /// </summary>
        /// <param name="name">Название задачи</param>
        public List<SheduledTask> FetchTask(string name);

        /// <summary>
        /// Удалить задачу с заданным именем
        /// </summary>
        /// <param name="name">Название задачи</param>
        public void RemoveTask(string name);

        /// <summary>
        /// Удалить задачу с заданным именем
        /// </summary>
        /// <param name="name">Ссылка на задачу</param>
        public void RemoveTask(SheduledTask task);

        /// <summary>
        /// Вернуть все запланированные задачи
        /// </summary>
        public List<SheduledTask> GetAllTasks();
    }
}
