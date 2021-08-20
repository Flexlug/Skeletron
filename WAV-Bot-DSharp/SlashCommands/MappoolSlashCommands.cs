using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;


using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Bot_DSharp.Database.Models;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Singleton)]
    [SlashCommandGroup("mappool", "Команды для управления предложкой")]
    public class MappoolSlashCommands : ApplicationCommandModule
    {
        private IMappoolService mappoolService;
        private ILogger<MappoolSlashCommands> logger;

        public MappoolSlashCommands(IMappoolService mappoolService,
                                    ILogger<MappoolSlashCommands> logger)
        {
            this.mappoolService = mappoolService;

            this.logger = logger;

            logger.LogInformation("MappolSlashCommands loaded");
        }

        [SlashCommand("get-for", "Показать предлагаемые карты для следующего W.w.W. для выбранной категории.")]
        public async Task GetMappol(InteractionContext ctx,
            [Option("category", "Конкурсная категория")] CompitCategory category)
        {
            if (!((ctx.Channel.Name?.Contains("-bot") ?? false) ||
                  (ctx.Channel.Name?.Contains("dev-announce") ?? false) ||
                  (ctx.Channel.Name?.Contains("-scores") ?? false)))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.")
                                                                                     .AsEphemeral(true));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                         new DiscordInteractionResponseBuilder()
                                            .AddEmbed(mappoolService.GetCategoryMappool(category)));
        }

        [SlashCommand("get", "Показать предлагаемые карты для следующего W.w.W.")]
        public async Task GetMappol(InteractionContext ctx)
        {
            if (!((ctx.Channel.Name?.Contains("-bot") ?? false) ||
                  (ctx.Channel.Name?.Contains("dev-announce") ?? false) ||
                  (ctx.Channel.Name?.Contains("-scores") ?? false)))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                              new DiscordInteractionResponseBuilder().WithContent("Использование данной команды запрещено в этом текстовом канале. Используйте специально отведенный канал для ботов, связанных с osu!.")
                                                                                     .AsEphemeral(true));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
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

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }
        [SlashCommand("vote", "Проголосовать за выбранную карту")]
        public async Task Vote(InteractionContext ctx,
            [Option("id", "ID карты")] long bmId)
        {
            string res = mappoolService.Vote(ctx.Member.Id.ToString(), (int)bmId);

            if (res == "done")
                res = ":ok_hand:";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                          new DiscordInteractionResponseBuilder()
                                             .AsEphemeral(true)
                                             .WithContent(res));
        }
    }
}
