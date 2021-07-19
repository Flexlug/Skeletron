using System.Collections.Generic;

using WAV_Bot_DSharp.Database.Models;

namespace WAV_Bot_DSharp.Services.Interfaces
{
    public interface IMappoolService
    {
        /// <summary>
        /// Получить список предложенных карт для данной категории
        /// </summary>
        public List<CompitMappolMap> GetCompitMappolMaps(CompitCategories catergory);

        /// <summary>
        /// Предложить карту
        /// </summary>
        /// <param name="mapUrl">Ссылка на карту</param>
        public void SuggestMap(string mapUrl);
    }
}
