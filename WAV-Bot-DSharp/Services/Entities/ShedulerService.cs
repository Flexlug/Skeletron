using System;
using System.Text;
using System.Timers;
using System.Collections.Generic;

using WAV_Bot_DSharp.Threading;
using WAV_Bot_DSharp.Services.Structures;
using NLog;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class ShedulerService
    {
        private List<SheduledTask> sheduledTasks;
        private BackgroundQueue queue;
        private Timer timer;

        private ILogger logger;

        public ShedulerService(ILogger logger)
        {
            sheduledTasks = new List<SheduledTask>();
            queue = new BackgroundQueue();

            this.logger = logger;

            timer = new Timer(1000);
            timer.Elapsed += Timer_Elapsed;

            logger.Info("ShedulerService started");
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            logger.Debug("ShedulerService timer elapsed");

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
            logger.Debug("ShedulerService timer started");
            timer.Start();
        }

        public void AddTask(SheduledTask task) => sheduledTasks.Add(task);
    }
}
