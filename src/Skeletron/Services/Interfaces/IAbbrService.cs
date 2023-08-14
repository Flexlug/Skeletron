using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skeletron.Services.Interfaces
{
    public interface IAbbrService
    {
        /// <summary>
        /// Получить рандомное слово
        /// </summary>
        /// <returns></returns>
        string GetRandomWord();
    }
}
