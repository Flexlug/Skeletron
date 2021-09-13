using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;
using Skeletron.Database.Models;

using OsuNET_Api.Models.Bancho;

namespace Skeletron.Services.Interfaces
{
	public interface IMappoolService
	{
		/// <summary>
		/// Получить предложенные карты для заданной категории
		/// </summary>
		/// <param name="category">Запрашиваемая категория</param>
		/// <returns>Возвращает в виде Embed список карт или ошибку</returns>
		public DiscordEmbed GetCategoryMappool(CompitCategory category);

		/// <summary>
		/// Получить предложенные карты для заданной категории
		/// </summary>
		/// <param name="user">Запрашивающий маппул</param>
		/// <returns>Возвращает в виде Embed список карт или ошибку</returns>
		public DiscordEmbed GetCategoryMappool(DiscordMember user);

		/// <summary>
		/// Добавить голос за карту
		/// </summary>
		/// <param name="memberId">Discord ID голосующего</param>
		/// <param name="bmId">ID карты</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string Vote(string memberId, int bmId);

		/// <summary>
		/// Добавить карту
		/// </summary>
		/// <param name="memberId">Discord ID предлагающего карту</param>
		/// <param name="category">Категория, для которой предлагается карта</param>
		/// <param name="url">Ссылка на карту, за которую голосующий отдаёт свой голос</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string AddMap(string memberId, string url);

		/// <summary>
		/// Добавить карту как администратор
		/// </summary>
		/// <param name="category">Категория, для которой предлагается карта</param>
		/// <param name="url">Предлагаемая карта</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string AddAdminMap(CompitCategory category, string url);

		/// <summary>
		/// Удалить карту
		/// </summary>
		/// <param name="category">Категория, из которой удаляется карта</param>
		/// <param name="bmId">Id карты</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string RemoveMap(CompitCategory category, int bmId);

		/// <summary>
		/// Очистить все карты
		/// </summary>
		public void ResetMappool();

		/// <summary>
		/// Начать отслеживание изменений маппула
		/// </summary>
		/// <returns></returns>
		public Task<string> StartSpectating();

		/// <summary>
		/// Обновить Embed маппула
		/// </summary>
		/// <returns></returns>
		public Task<string> UpdateMappoolStatus();

		/// <summary>
		/// ОБновить Embed маппула для отдельной категории
		/// </summary>
		/// <returns></returns>
		public Task<string> UpdateCategoryMappoolStatus(CompitCategory cat);

		/// <summary>
		/// Отключить отслеживание предложки без подведения итогов
		/// </summary>
		/// <returns></returns>
		public Task<string> HaltSpectating();

		/// <summary>
		/// Остановить отслеживание предложки и подвести результаты голосования
		/// </summary>
		/// <returns></returns>
		public Task<string> StopSpectating();

		/// <summary>
		/// Задать канал для анонса маппула
		/// </summary>
		/// <param name="channel_id">Id текстового канала</param>
		/// <returns></returns>
		public Task<string> SetAnnounceChannel(ulong channel_id);
	}
}