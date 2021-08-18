using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.CommandsNext.Attributes;

using WAV_Bot_DSharp.Services.Interfaces;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.SlashCommands
{
    public class MappoolSlashCommands : ApplicationCommandModule
    {
        private IMappoolService mappoolService;
        private ILogger<MappoolSlashCommands> logger;

        public MappoolSlashCommands(IMappoolService mappoolService,
                                    ILogger<MappoolSlashCommands> logger)
        {
            this.mappoolService = mappoolService;

            logger.LogInformation("OsuSlashCommands loaded");
        }

        [SlashCommand("mappol-for", "Показать предлагаемые карты для следующего W.w.W. для выбранной категории.")]
        public async Task GetMappol(InteractionContext ctx,
            [Choice("Beginner", "beginner")]
            [Choice("Alpha", "alpha")]
            [Choice("Beta", "beta")]
            [Choice("Gamma", "gamma")]
            [Choice("Delta", "delta")]
            [Choice("Epsilon", "epsilon")]
            [Option("category", "Конкурсная категория")] string category)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                         new DiscordInteractionResponseBuilder()
                                            .AddEmbed(mappoolService.GetCategoryMappool(category)));
        }

        [SlashCommand("mappol", "Показать предлагаемые карты для следующего W.w.W.")]
        public async Task GetMappol(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                         new DiscordInteractionResponseBuilder()
                                            .AddEmbed(mappoolService.GetCategoryMappool(ctx.Member)));
        }

        [SlashCommand("offer", "Предложить карту на W.w.W и проголосовать за неё.")]
        public async Task AddMap(InteractionContext ctx,
            [Option("mapUrl", "Ссылка на карту (Bancho)")] string url)
        {
            string res = mappoolService.AddMap(ctx.Member.Id.ToString(), url);

            if (res == "done")
                res = ":ok_hand:";

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }

        [SlashCommand("offer-default", "[ТОЛЬКО ДЛЯ АДМИНОВ] Предложить карту на W.w.W в качестве карты по умолчанию."), RequireRoles(RoleCheckMode.Any, "Admin", "Moder")]
        public async Task AddMapAdmin(InteractionContext ctx,
            [Choice("Beginner", "beginner")]
            [Choice("Alpha", "alpha")]
            [Choice("Beta", "beta")]
            [Choice("Gamma", "gamma")]
            [Choice("Delta", "delta")]
            [Choice("Epsilon", "epsilon")]
            [Option("category", "Конкурсная категория")] string category,
            [Option("mapUrl", "Ссылка на карту (Bancho)")] string url)
        {
            string res = mappoolService.AddAdminMap(ctx.Member.Id.ToString(), url);

            if (res == "done")
                res = ":ok_hand:";

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }

        public async Task Vote(InteractionContext ctx,
            [Option("id", "ID карты")] string bmId)
        {
            string res = mappoolService.Vote(ctx.Member.Id.ToString(), bmId);

            if (res == "done")
                res = ":ok_hand:";

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }
    }
}
