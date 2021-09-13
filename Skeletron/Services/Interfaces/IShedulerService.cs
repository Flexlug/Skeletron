using System.Collections.Generic;

using Skeletron.Services.Models;

namespace Skeletron.Services.Interfaces
{
    public interface IShedulerService
    {
        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="task">Планируемая задача</param>
        public void AddTask(SheduledTask task);

        /// <summary>
        /// Проверить наличие указанной задачи
        /// </summary>
        /// <param name="name">Название задачи</param>
        public bool FetchTask(SheduledTask task);

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
