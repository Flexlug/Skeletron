using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Bot_DSharp.Services.Structures
{
    /// <summary>
    /// Запланированная задача, которая должна выполняться раз в определенный промежуток времени
    /// </summary>
    public class SheduledTask
    {
        private TimeSpan Interval { get; set; }
        private DateTime LastInvokeTime { get; set; }

        /// <summary>
        /// Задача, которая должна выполниться
        /// </summary>
        public event Invoke OnInvoke;
        public delegate void Invoke();

        public SheduledTask(TimeSpan interval)
        {
            this.Interval = interval;
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

        /// <summary>
        /// Запустить задачу
        /// </summary>
        public void InvokeTask() => OnInvoke.Invoke();
    }
}
