using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;


using Skeletron.Services.Interfaces;
using Skeletron.Database.Models;

using Microsoft.Extensions.Logging;

namespace Skeletron.SlashCommands
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
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                         new DiscordInteractionResponseBuilder()
                                            .AddEmbed(mappoolService.GetCategoryMappool(category))
                                            .AsEphemeral(true));
        }

        [SlashCommand("get", "Показать предлагаемые карты для следующего W.w.W.")]
        public async Task GetMappol(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                                         new DiscordInteractionResponseBuilder()
                                            .AddEmbed(mappoolService.GetCategoryMappool(ctx.Member))
                                            .AsEphemeral(true));
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
    }
}
