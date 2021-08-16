using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using WAV_Bot_DSharp.Services.Interfaces;

namespace WAV_Bot_DSharp.Commands
{
    public class MappolCommands : SkBaseCommandModule
    {
        private IMappoolService mappoolService;

        public MappolCommands(IMappoolService mappoolService)
        {
            ModuleName = "W.w.W mappool";

            this.mappoolService = mappoolService;
        }

        [Command("mappool"), Description("Получить список всех предложенных карт для категории, в которой вы состоите")]
        public async Task GetMappol(CommandContext ctx)
        {

        }
    }
}
