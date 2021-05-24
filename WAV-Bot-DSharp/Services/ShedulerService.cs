using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;

using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Services.Models;
using WAV_Bot_DSharp.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class ShedulerService : IShedulerService
    {
        private List<SheduledTask> sheduledTasks;
        private BackgroundQueue queue;
        private Timer timer;

        private ILogger<ShedulerService> logger;

        /// <summary>
        /// Частота проверки наличия задач (мс)
        /// </summary>
        private const int SHEDULER_INTERVAL = 1000;

        public ShedulerService(ILogger<ShedulerService> logger)
        {
            sheduledTasks = new List<SheduledTask>();
            queue = new BackgroundQueue();

            this.logger = logger;

            timer = new Timer(SHEDULER_INTERVAL);
            timer.Elapsed += Timer_Elapsed;

            logger.LogInformation("ShedulerService started");

            StartSheduler();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var task in sheduledTasks)
                if (task.Ready())
                {
                    queue.QueueTask(task.Action);
                    if (!task.Repeat)
                        sheduledTasks.Remove(task);
                }
        }

        private void StartSheduler()
        {
            logger.LogDebug("ShedulerService timer started");
            timer.Start();
        }

        /// <summary>
        /// Добавить задачу
        /// </summary>
        /// <param name="task">Планируемая задача</param>
        public void AddTask(SheduledTask task) => sheduledTasks.Add(task);

        /// <summary>
        /// Получить информацию о запланированных задачах, если таковые имеются
        /// </summary>
        /// <param name="name">Название задачи</param>
        public List<SheduledTask> FetchTask(string name)
        {
            return sheduledTasks.Select(x => x)
                                .Where(x => x.Name == name)
                                .ToList();
        }

        /// <summary>
        /// Удалить задачу с заданным именем
        /// </summary>
        /// <param name="name">Название задачи</param>
        public void RemoveTask(string name)
        {
            SheduledTask task = sheduledTasks.FirstOrDefault(x => x.Name == name);

            if (task is null)
                return;

            sheduledTasks.Remove(task);
        }

        /// <summary>
        /// Удалить задачу с заданным именем
        /// </summary>
        /// <param name="name">Ссылка на задачу</param>
        public void RemoveTask(SheduledTask task)
        {
            if (sheduledTasks.Exists(x => x.Equals(task)))
                sheduledTasks.Remove(task);
        }

        /// <summary>
        /// Вернуть все запланированные задачи
        /// </summary>
        public List<SheduledTask> GetAllTasks() => sheduledTasks;
    }
}
