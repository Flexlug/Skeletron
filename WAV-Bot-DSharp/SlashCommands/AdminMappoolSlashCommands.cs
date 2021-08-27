using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Database.Models;
using DSharpPlus.Entities;

namespace WAV_Bot_DSharp.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Singleton)]
    [SlashCommandGroup("mappool-admin", "Команды для управления предложкой", false)]
    public class AdminMappoolSlashCommands : ApplicationCommandModule
    {
        private IMappoolService mappoolService;
        private ILogger<AdminMappoolSlashCommands> logger;

        public AdminMappoolSlashCommands(IMappoolService mappoolService,
                                        ILogger<AdminMappoolSlashCommands> logger)
        {
            this.mappoolService = mappoolService;

            this.logger = logger;

            logger.LogInformation("AdminMappolSlashCommands loaded");
        }

        [SlashCommand("offer-default", "[ТОЛЬКО ДЛЯ АДМИНОВ] Предложить карту на W.w.W в качестве карты по умолчанию.")]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        public async Task AddMapAdmin(InteractionContext ctx,
            [Option("category", "Конкурсная категория")] CompitCategory category,
            [Option("mapUrl", "Ссылка на карту (Bancho)")] string url)
        {
            string res = mappoolService.AddAdminMap(category, url);

            if (res == "done")
                res = ":ok_hand:";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }

        [SlashCommand("offer-remove", "[ТОЛЬКО ДЛЯ АДМИНОВ] Удалить выбранную карту")]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        public async Task RemoveMap(InteractionContext ctx,
            [Option("category", "Конкурсная категория")] CompitCategory category,
            [Option("id", "ID карты")] long bmId)
        {
            string res = mappoolService.RemoveMap(category, (int)bmId);

            if (res == "done")
                res = ":ok_hand:";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }

        [SlashCommand("reset", "[ТОЛЬКО ДЛЯ АДМИНОВ] Очистить всю предложку")]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        public async Task ResetMappool(InteractionContext ctx)
        {
            mappoolService.ResetMappool();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                              new DiscordInteractionResponseBuilder()
                                 .AsEphemeral(true)
                                 .WithContent(":ok_hand:"));
        }
    }
}
