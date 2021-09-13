using System.Linq;
using System.Timers;
using System.Collections.Generic;

using Skeletron.Threading;
using Skeletron.Services.Models;
using Skeletron.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace Skeletron.Services.Entities
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
                    task.UpdateLastInvokationTime();
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

        public bool FetchTask(SheduledTask task)
        {
            return sheduledTasks.Exists(x => x.Equals(task));
        }
    }
}
