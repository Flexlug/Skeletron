using System;

namespace WAV_Bot_DSharp.Services.Models
{
    /// <summary>
    /// Запланированная задача, которая должна выполняться один раз или через заданный промежуток времени
    /// </summary>
    public class SheduledTask
    {
        /// <summary>
        /// Название задачи
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Интервал между повторениями задачи
        /// </summary>
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Будет ли выполняться задача циклично
        /// </summary>
        public bool Repeat { get; set; }

        /// <summary>
        /// Задача, которая должна выполниться
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// Время, когда задача была запущена в последний раз
        /// </summary>
        private DateTime LastInvokeTime { get; set; }

        /// <summary>
        /// Создать новую задачу, которая будет выполнениа через interval времени.
        /// </summary>
        /// <param name="action">Выполняемая задача</param>
        /// <param name="interval">Интервал времени, через который будет выполнена команда</param>
        /// <param name="repeat">Будет ли команда выполняться циклично</param>
        public SheduledTask(string name, Action action, TimeSpan interval, bool repeat = false)
        {
            this.Action = action;
            this.Interval = interval;
            this.Repeat = repeat;

            LastInvokeTime = DateTime.Now;
        }

        /// <summary>
        /// Создать новую задачу, которая будет выполнениа через interval времени.
        /// </summary>
        protected SheduledTask() { }

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
        /// Обновить последнюю дату и время запуска задачи
        /// </summary>
        public void UpdateLastInvokationTime() => LastInvokeTime = DateTime.Now;
    }
}
