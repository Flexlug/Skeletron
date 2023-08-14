using System;
using System.IO;

namespace Skeletron.Services.Models
{
    /// <summary>
    /// Планируемая задача на удаление файла
    /// </summary>
    public class RemoveFileSheduledTask : SheduledTask
    {
        /// <summary>
        /// Имя удаляемого файла
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Создать задачу на удаление файла через заданное время
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="interval">Время, через которое файл будет удален</param>
        public RemoveFileSheduledTask(string filePath, TimeSpan interval)
        {
            this.FileName = filePath;

            this.Repeat = false;
            this.Interval = interval;
            this.Action = () =>
            {
                try
                {
                    File.Delete(FileName);
                }
                catch (Exception) { }
            };
        }
    }
}
