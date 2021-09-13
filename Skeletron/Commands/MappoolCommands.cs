using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skeletron.Services.Interfaces;

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using Skeletron.Database.Models;
using Skeletron.Converters;

namespace Skeletron.Commands
{
    [RequireGuild]
    [Group("mappool")]
    public class MappoolCommands : SkBaseCommandModule
    {
        private IMappoolService mappoolService;
        private OsuEnums osuEnums;

        public MappoolCommands(IMappoolService mappoolService,
                               OsuEnums osuEnums)
        {
            this.mappoolService = mappoolService;
            this.osuEnums = osuEnums;

            this.ModuleName = "Mappool";
        }

        [Command("start")]
        [Description("Начать отслеживание изменений маппула"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task StartSpectate(CommandContext ctx)
        {
            string result = await mappoolService.StartSpectating();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
            else
                await ctx.RespondAsync(result);
        }

        [Command("halt")]
        [Description("Остановить отслеживание изменений маппула без подведения результатов"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task HaltSpectate(CommandContext ctx)
        {
            string result = await mappoolService.HaltSpectating();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
            else
                await ctx.RespondAsync(result);
        }

        [Command("stop")]
        [Description("Остановить отслеживание изменений маппула и огласить результаты"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task StopSpectate(CommandContext ctx)
        {
            string result = await mappoolService.StopSpectating();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
            else
                await ctx.RespondAsync(result);
        }

        [Command("update")]
        [Description("Обновить маппул или перегенерировать сообщения"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task UpdateSpectate(CommandContext ctx)
        {
            string result = await mappoolService.UpdateMappoolStatus();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
            else
                await ctx.RespondAsync(result);
        }

        [Command("cat-update")]
        [Description("Обновить маппул для конкретной категории"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task UpdateCategorySpectate(CommandContext ctx,
            [Description("Категория, для которой необходимо обновить маппул")] string category)
        {
            CompitCategory? compitCategory = osuEnums.StringToCategory(category);
            if (compitCategory is null)
            {
                await ctx.RespondAsync("Не удалось получить категорию");
                return;
            }

            string result = await mappoolService.UpdateCategoryMappoolStatus((CompitCategory)compitCategory);
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
            else
                await ctx.RespondAsync(result);
        }

        [Command("set-channel")]
        [Description("Задать канал, в котором будут публиковаться изменения и результаты"), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task SetSpectateChannel(CommandContext ctx,
            [Description("Канал, в котором будут публиковаться изменения")] DiscordChannel channel)
        {
            string result = await mappoolService.SetAnnounceChannel(channel.Id);
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(ctx.Client, 805364968593686549));
            else
                await ctx.RespondAsync(result);
        }
    }
}
