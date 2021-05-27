using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAV_Bot_DSharp.Database.Models;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface ISheetGenerator
    {
        /// <summary>
        /// Преобразовать все скоры в excel-файл
        /// </summary>
        /// <param name="scores">Список всех скоров</param>
        /// <returns></returns>
        public Task<FileInfo> CompitScoresToFile(List<CompitScore> scores);
    }
}
