using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using WAV_Bot_DSharp.Services.Structures;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Сервис, запускающий через определенный интервал запланированные задачи
    /// </summary>
    public class SheduleService
    {
        public List<SheduledTask> Tasks = new List<SheduledTask>();

        private Timer timer = new Timer();

        public SheduleService()
        {
            timer.Interval = 10000;
            timer.Elapsed += CheckShedules;
            timer.Start();
        }

        private void CheckShedules(object sender, ElapsedEventArgs e)
        {
            foreach (SheduledTask task in Tasks)
                if (task.Ready())
                    task.InvokeTask();
        }
    }
}
