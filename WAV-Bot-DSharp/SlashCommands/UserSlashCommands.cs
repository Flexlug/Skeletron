using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands;

using Microsoft.Extensions.Logging;

using WAV_Bot_DSharp.Services.Interfaces;

namespace WAV_Bot_DSharp.SlashCommands
{
    public class UserSlashCommands : SlashCommandModule
    {
        private IWordsService service;

        private ILogger<UserSlashCommands> logger;

        public UserSlashCommands(IWordsService service,
                                 ILogger<UserSlashCommands> logger)
        {
            this.service = service;

            this.logger = logger;

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
    }
}
