using System;
using System.Text;
using System.Timers;
using System.Collections.Generic;

using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Services.Models;

using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class ShedulerService
    {
        private List<SheduledTask> sheduledTasks;
        private BackgroundQueue queue;
        private Timer timer;

        private ILogger<ShedulerService> logger;

        public ShedulerService(ILogger<ShedulerService> logger)
        {
            sheduledTasks = new List<SheduledTask>();
            queue = new BackgroundQueue();

            this.logger = logger;

            timer = new Timer(5000);
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

        public void StartSheduler()
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
        /// Запланировать удаление файла (будет удален через 30 секунд)
        /// </summary>
        /// <param name="path"></param>
        public void AddFileDeleteTask(string path)
        {
            logger.LogInformation($"File deletion sheduled. Path: {path}");
            sheduledTasks.Add(new SheduledTask(() => 
            {
                try
                {
                    File.Delete(path); 
                }
                catch(Exception e) 
                {
                    logger.LogInformation($"File deletion error. Path: {path}");
                }
            }, TimeSpan.FromSeconds(5)));
        }
    }
}
