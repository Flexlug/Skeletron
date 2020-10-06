using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using WAV_Bot_DSharp.Services;
using WAV_Bot_DSharp.Services.Entities;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Команды для отслеживания активности пользователей WAV севрера
    /// </summary>
    [RequirePermissions(Permissions.Administrator)]
    public class ActivityCommands : BaseCommandModule
    {
        IActivityService activity;

        public ActivityCommands(IActivityService activityService)
        {
            this.activity = activityService;
        }

        /// <summary>
        /// Показывает список всех юзеров в пагинированном виде
        /// </summary>
        /// <param name="commandContext">Контекст команды</param>
        /// <param name="page">Номер страницы</param>
        /// <returns></returns>
        [Command("get-page"), Description("Shows users list")]
        public async Task GetActivity(CommandContext commandContext,
            [Description("Number of page")] int page) 
        {
            int totalPages = await activity.GetTotalPages();

            if (page <= totalPages && page > 0)
            { 
                List<UserInfo> users = await activity.ViewActivityInfo(page);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithFooter($"Pages: {page} of {totalPages}")
                    .WithTitle("Users list");

                foreach(UserInfo user in users)
                {
                    DiscordMember member = await commandContext.Guild.GetMemberAsync(user.Uid);
                    embed.AddField($"{(member.DisplayName == string.Empty ? user.Uid.ToString() : member.DisplayName)}", $"{user.LastActivity.ToShortDateString()} {user.LastActivity.ToLongTimeString()} ({(int)(DateTime.Now - user.LastActivity).TotalDays} days AFK)");
                }

                await commandContext.RespondAsync("", embed:embed);
            }
            else
            {
                await commandContext.RespondAsync($"Неправильно введён номер страницы. Общее количество страниц: {totalPages}");
            }
        }

        /// <summary>
        /// Обновляет базу данных в соответствии с пользователями на сервере
        /// </summary>
        /// <param name="commandContext">Контекст команды</param>
        /// <returns></returns>
        [Command("update-all-users"), Description("Update all users on this guild and update DB")]
        public async Task UpdateUsers(CommandContext commandContext)
        {
            int newMembers = await activity.UpdateCurrentUsers();
            await commandContext.RespondAsync($"Успешно. Количество новых записей: {newMembers}");
        }

        /// <summary>
        /// Задаёт текущую дату в качестве последней активности пользователя
        /// </summary>
        /// <param name="commandContext">Контекст команды</param>
        /// <param name="member">Участник, информацию о котором нужно обновить</param>
        /// <returns></returns>
        [Command("manual-update-to-current"), Description("Manual update user activity to present date")]
        public async Task ManualUpdateToCurrent(CommandContext commandContext,
            [Description("User to update")] DiscordMember member)
        {
            await activity.ManualUpdateToPresent(member.Id);
            await commandContext.RespondAsync($"Успешно. {member.DisplayName}: {DateTime.Now}");
        }

        /// <summary>
        /// Задаёт заданную дату в качестве последней активности пользователя
        /// </summary>
        /// <param name="commandContext">Контекст команды</param>
        /// <param name="member">Участник, информацию о котором нужно обновить</param>
        /// <param name="dateTime">Заданная дата</param>
        /// <returns></returns>
        [Command("manual-update"), Description("Manual update user activity to specified date")]
        public async Task ManualUpdate(CommandContext commandContext,
            [Description("User to update")] DiscordMember member,
            [Description("Specified datetime")] DateTime dateTime)
        {
            await activity.ManualUpdate(member.Id, dateTime);
            await commandContext.RespondAsync($"Успешно. {member.DisplayName}: {dateTime}");
        }
    }
}
