using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Services.Interfaces;

using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace WAV_Bot_DSharp.Commands
{
    [RequireGuild]
    [Group("mappool-spectate")]
    public class MappoolCommands : SkBaseCommandModule
    {
        private IMappoolService mappoolService;

        public MappoolCommands(IMappoolService mappoolService)
        {
            this.mappoolService = mappoolService;
        }

        [Command("start")]
        [Description("Начать отслеживание изменений маппула")]
        public async Task StartSpectate(CommandContext ctx)
        {
            string result = await mappoolService.StartSpectating();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            else
                await ctx.RespondAsync(result);
        }

        [Command("halt")]
        [Description("Остановить отслеживание изменений маппула без подведения результатов")]
        public async Task HaltSpectate(CommandContext ctx)
        {
            string result = await mappoolService.HaltSpectating();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            else
                await ctx.RespondAsync(result);
        }

        [Command("stop")]
        [Description("Остановить отслеживание изменений маппула и огласить результаты")]
        public async Task StopSpectate(CommandContext ctx)
        {
            string result = await mappoolService.StopSpectating();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            else
                await ctx.RespondAsync(result);
        }

        [Command("update")]
        [Description("Обновить маппул или перегенерировать сообщения")]
        public async Task UpdateSpectate(CommandContext ctx)
        {
            string result = await mappoolService.UpdateMappoolStatus();
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            else
                await ctx.RespondAsync(result);
        }

        [Command("set-channel")]
        [Description("Задать канал, в котором будут публиковаться изменения и результаты")]
        public async Task SetSpectateChannel(CommandContext ctx,
            [Description("Канал, в котором будут публиковаться изменения")] DiscordChannel channel)
        {
            string result = await mappoolService.SetAnnounceChannel(channel.Id);
            if (result == "done")
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":ok_hand:"));
            else
                await ctx.RespondAsync(result);
        }
    }
}
