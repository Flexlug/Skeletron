using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Database.Models;

namespace WAV_Bot_DSharp.Database.Interfaces
{
    public interface IMappoolProvider
    {
        /// <summary>
        /// Сбросить весь маппул
        /// </summary>
        public void ResetMappool(); // Для админской команды

        /// <summary>
        /// Получить все карты
        /// </summary>
        /// <returns></returns>
        public List<OfferedMap> GetAllMaps(); // Для админской команды

        /// <summary>
        /// Предложенные карты для указанной категории
        /// </summary>
        /// <param name="category">Категория, для которой предложены карты</param>
        /// <returns></returns>
        public List<OfferedMap> GetCategoryMaps(CompitCategory category); // Для того, чтобы показать список всех карт для конкретной категории

        /// <summary>
        /// Проверить, предлагал ли кто-либо уже такую карту
        /// </summary>
        /// <param name="mapId">ID карты</param>
        /// <returns></returns>
        public bool CheckMapOffered(int mapId, CompitCategory category);

        /// <summary>
        /// Проверить, предлагал ли уже пользователь какую-либо карту
        /// </summary>
        /// <param name="userId">ID пользователя</param>
        /// <returns></returns>
        public bool CheckUserSubmitedAny(string userId);

        /// <summary>
        /// Проверить, голосовал ли пользователь за какие-либо карты
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool CheckUserVoted(string userId);

        /// <summary>
        /// Добавить предложенную кем-то карту
        /// </summary>
        /// <param name="map">Предлагаемая карта</param>
        /// <result>True, если человек ещё не добавлял каких-либо своих карт. False, если уже предлагал (карта в таком случае добавлена не будет)</result>
        public void MapAdd(OfferedMap map); // Для того, чтобы могли добавить свою карту

        /// <summary>
        /// Добавить запись о голосе за карту
        /// </summary>
        /// <param name="userId">Discord ID предложившего карту</param>
        /// <param name="beatmapId">    </param>
        /// <returns></returns>
        public void MapVote(string userId, CompitCategory category, int beatmapId);

        /// <summary>
        /// Удалить карту
        /// </summary>
        /// <param name="category"></param>
        /// <param name="beatmapId"></param>
        public void MapRemove(CompitCategory category, int beatmapId);
    }
}
