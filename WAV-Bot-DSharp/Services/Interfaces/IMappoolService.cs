using DSharpPlus.Entities;

using System.Collections.Generic;

using WAV_Bot_DSharp.Database.Models;

using WAV_Osu_NetApi.Models.Bancho;

namespace WAV_Bot_DSharp.Services.Interfaces
{
	public interface IMappoolService
	{
		/// <summary>
		/// Получить предложенные карты для заданной категории
		/// </summary>
		/// <param name="category">Запрашиваемая категория</param>
		/// <returns>Возвращает в виде Embed список карт или ошибку</returns>
		public DiscordEmbed GetCategoryMappool(string category);

		/// <summary>
		/// Добавить голос за карту
		/// </summary>
		/// <param name="member">Голосующий</param>
		/// <param name="category">Категория, в которой находится выбранная карта</param>
		/// <param name="url">Ссылка на карту, за которую голосующий отдаёт свой голос</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string Vote(DiscordMember member, string category, string url);

		/// <summary>
		/// Добавить карту
		/// </summary>
		/// <param name="member">Предложивший карту</param>
		/// <param name="category">Категория, для которой предлагается карта</param>
		/// <param name="url">Ссылка на карту, за которую голосующий отдаёт свой голос</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string AddMap(DiscordMember member, string category, string url);

		/// <summary>
		/// Добавить карту как администратор
		/// </summary>
		/// <param name="category">Категория, для которой предлагается карта</param>
		/// <param name="url">Предлагаемая карта</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string AddAdminMap(string category, string url);

		/// <summary>
		/// Удалить карту
		/// </summary>
		/// <param name="category">Категория, из которой удаляется карта</param>
		/// <param name="bmId">Id карты</param>
		/// <returns>Возвращает строку "done" в случае успеха. Иначе возвращает ошибку</returns>
		public string RemoveMap(string category, string bmId);

		/// <summary>
		/// Очистить все карты
		/// </summary>
		public void ResetMappool();
	}
}