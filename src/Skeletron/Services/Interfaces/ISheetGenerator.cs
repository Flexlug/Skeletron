using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Skeletron.Database.Models;

namespace Skeletron.Services.Interfaces
{
    public interface ISheetGenerator
    {
        /// <summary>
        /// Преобразовать все скоры в excel-файл
        /// </summary>
        /// <param name="scores">Список всех скоров</param>
        /// <returns></returns>
        public Task<FileStream> CompitScoresToFile(List<CompitScore> scores);
    }
}
