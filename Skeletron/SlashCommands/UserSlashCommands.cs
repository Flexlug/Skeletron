using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

using Microsoft.Extensions.Logging;

using Skeletron.Services.Interfaces;

namespace Skeletron.SlashCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Singleton)]
    public class UserSlashCommands : ApplicationCommandModule
    {
        private IWordsService service;

        private ILogger<UserSlashCommands> logger;

        //private DiscordRole mapSpectator;
        //private DiscordRole contentSpectator;

        public UserSlashCommands(IWordsService service,
                                 ILogger<UserSlashCommands> logger,
                                 DiscordGuild guild)
        {
            this.service = service;

            this.logger = logger;

            //this.mapSpectator = guild.GetRole(873588134938562580);
            //this.contentSpectator= guild.GetRole(873587868524765274);

            logger.LogInformation("UserSlashCommands loaded");
        }

        [SlashCommand("words-check", "Првоерить наличие слова в канале words")]
        public async Task WordsCheck(InteractionContext ctx,
            [Option("word", "Проверяемое слово")] string word)
        {
            string checkingWord = word.ToLower();

            logger.LogInformation($"Triggered \'word\' command with param: {word} by {ctx.Member.Username}");

            if (service.CheckWord(checkingWord))
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                    .AsEphemeral(true)
                    .WithContent("Такое слово уже есть. Используйте другое"));
            else
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                    .AsEphemeral(true)
                    .WithContent("Такого слова нет"));
        }

        //[SlashCommand("ping-role", "Пингануть выбранную роль")]
        //public async Task PingRole(InteractionContext ctx,
        //    [Choice("Map Spectator", "map_spectator")]
        //    [Choice("Content Spectator", "content_spectator")]
        //    [Option("Role", "Роль, которую вы хотите пингануть")] string role,
        //    [Option("Text", "Комментарий, которым будет сопровождаться пинг")] string text)
        //{
        //    if (string.IsNullOrEmpty(text))
        //    {
        //        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
        //                    .AsEphemeral(true)
        //                    .WithContent("Нельзя делать пинг с пустым комментарием."));
        //        return;
        //    }

        //    switch (role)
        //    {
        //        case "map_spectator":
        //            if (ctx.Member.Roles.Any(x => x.Name == "Mapper"))
        //                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
        //                    .WithContent($"{mapSpectator.Mention}\n{text}"));
        //            else
        //                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
        //                    .AsEphemeral(true)
        //                    .WithContent("У вас нет прав на пинг данной роли."));
        //            break;

        //        case "content_spectator":
        //            if (ctx.Member.Roles.Any(x => x.Name == "Content Maker"))
        //                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
        //                    .WithContent($"{contentSpectator.Mention}\n{text}"));
        //            else
        //                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
        //                    .AsEphemeral(true)
        //                    .WithContent("У вас нет прав на пинг данной роли."));
        //            break;

        //        default:
        //            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
        //                .AsEphemeral(true)
        //                .WithContent("Неверно указана роль"));
        //            break;
        //    }
        //}
    }
}
