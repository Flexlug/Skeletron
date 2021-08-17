using System;
using System.Threading.Tasks;

using DSharpPlus.SlashCommands;

using WAV_Bot_DSharp.Services.Interfaces;

namespace WAV_Bot_DSharp.SlashCommands
{
    public class MappoolSlashCommands : SlashCommandModule
    {
        private IMappoolService mappoolService;

        public MappoolSlashCommands(IMappoolService mappoolService)
        {
            this.mappoolService = mappoolService;
        }

        [SlashCommand("mappol", "Показать предлагаемые карты для следующего W.w.W")]
        public async Task GetMappol(InteractionContext ctx,
            [Choice("Beginner", "beginner")]
            [Choice("Alpha", "alpha")]
            [Choice("Beta", "beta")]
            [Choice("Gamma", "gamma")]
            [Choice("Delta", "delta")]
            [Choice("Epsilon", "epsilon")]
            [Option("category", "Конкурсная категория")]string category)
        {

        }
    }
}
