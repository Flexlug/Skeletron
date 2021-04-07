using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.Logging;

using WAV_Bot_DSharp.Services;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Services.Structures;

namespace WAV_Bot_DSharp.Commands
{
    /// <summary>
    /// Команды для отслеживания активности пользователей WAV севрера
    /// </summary>
    [RequirePermissions(Permissions.Administrator), RequireGuild]
    public class ActivityCommands : BaseCommandModule
    {
        IReadOnlyDictionary<ulong, DiscordMember> serverUsers;
        IActivityService activity;
        ILogger<ActivityCommands> logger;

        public ActivityCommands(IActivityService activityService, ILogger<ActivityCommands> logger)
        {
            this.activity = activityService;
            this.logger = logger;;

            logger.LogInformation("ActivityCommands loaded");
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
            int totalPages = await activity.GetTotalPagesAsync();

            if (page <= totalPages && page > 0)
            { 
                List<UserInfo> users = await activity.ViewActivityInfoAsync(page);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithFooter($"Pages: {page} of {totalPages}")
                    .WithTitle("Users list");

                foreach(UserInfo user in users)
                {
                    DiscordMember member = null;
                    try
                    {
                         member = await commandContext.Guild.GetMemberAsync(user.Uid);
                    }
                    catch(Exception e) 
                    {
                        logger.LogWarning($"Can't find user {user.Uid}");
                    }

                    embed.AddField($"{(member == null ? user.Uid.ToString() : member.DisplayName)}", $"{user.LastActivity.ToShortDateString()} {user.LastActivity.ToLongTimeString()} ({(int)(DateTime.Now - user.LastActivity).TotalDays} days AFK)");
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
            IReadOnlyCollection<DiscordMember> allUsers = await commandContext.Guild.GetAllMembersAsync();
            Dictionary<ulong, DiscordMember> usersAndIDs = new Dictionary<ulong, DiscordMember>(allUsers.Select(x => new KeyValuePair<ulong, DiscordMember>(x.Id, x)));

            int newMembers = await activity.UpdateCurrentUsersAsync(usersAndIDs);
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
            await activity.ManualUpdateToPresentAsync(member.Id);
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
            await activity.ManualUpdateAsync(member.Id, dateTime);
            await commandContext.RespondAsync($"Успешно. {member.DisplayName}: {dateTime}");
        }

        /// <summary>
        /// Проверяет наличие всех пользователей из БД на сервере и удаляет неправильные записи
        /// </summary>
        /// <param name="commandContext">Контекст команды</param>
        /// <returns></returns>
        [Command("exclude-absent"), Description("Manual update user activity to specified date")]
        public async Task ExcludeAbsentUsers(CommandContext commandContext)
        {
            IReadOnlyCollection<DiscordMember> allUsers = await commandContext.Guild.GetAllMembersAsync();
            Dictionary<ulong, DiscordMember> usersAndIDs = new Dictionary<ulong, DiscordMember>(allUsers.Select(x => new KeyValuePair<ulong, DiscordMember>(x.Id, x)));

            int absentCountawait  = await activity.ExcludeAbsentUsersAsync(usersAndIDs);
            await commandContext.RespondAsync($"Успешно. Удалено {absentCountawait} записей.");
        }

        /// <summary>
        /// Возвращает список пользователей, которые уже слишком дого находятся AFK
        /// </summary>
        /// <param name="commandContext">Контекст команды</param>
        /// <param name="page">Номер страницы</param>
        /// <returns></returns>
        [Command("who-is-next"), Description("Get list of AFK users")]
        public async Task WhoIsNext(CommandContext commandContext,
            [Description("Number of page")] int page)
        {
            List<UserInfo> users = await activity.GetAFKUsersAsync(page);

            if (users.Count == 0)
            {
                await commandContext.RespondAsync("Вроде все ещё шевелятся... пока...");
                return;
            }

            int totalPages = users.Count / ActivityService.PAGE_SIZE + 1;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithFooter($"Pages: {page} of {totalPages}")
                .WithTitle("Users list");

            foreach (UserInfo user in users)
            {
                DiscordMember member = await commandContext.Guild.GetMemberAsync(user.Uid);
                embed.AddField($"{(member.DisplayName == string.Empty ? user.Uid.ToString() : member.DisplayName)}", $"{user.LastActivity.ToShortDateString()} {user.LastActivity.ToLongTimeString()} ({(int)(DateTime.Now - user.LastActivity).TotalDays} days AFK)");
            }

            await commandContext.RespondAsync("Вот от них уже мертвечиной несёт.", embed: embed);
        }
    }
}
