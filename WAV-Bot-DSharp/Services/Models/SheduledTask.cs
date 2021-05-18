using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Services.Models
{
    /// <summary>
    /// Запланированная задача, которая должна выполняться раз в определенный промежуток времени
    /// </summary>
    public class SheduledTask
    {
        private TimeSpan Interval { get; set; }
        private DateTime LastInvokeTime { get; set; }

        /// <summary>
        /// Будет ли выполняться задача циклично
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Задача, которая должна выполниться
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// Создать новую задачу, которая будет выполнениа через interval времени.
        /// </summary>
        /// <param name="action">Выполняемая задача</param>
        /// <param name="interval">Интервал времени, через который будет выполнена команда</param>
        /// <param name="repeat">Будет ли команда выполняться циклично</param>
        public SheduledTask(Action action, TimeSpan interval, bool repeat = false)
        {
            this.Action = action;
            this.Interval = interval;
            this.Repeat = repeat;

            LastInvokeTime = DateTime.Now;
        }

        /// <summary>
        /// Проверяет, нужно ли выполнять зачачу
        /// </summary>
        /// <returns></returns>
        public bool Ready()
        {
            if (LastInvokeTime + Interval < DateTime.Now)
                return true;
            else
                return false;
        }
    }
}
