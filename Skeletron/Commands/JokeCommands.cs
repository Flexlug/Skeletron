using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Skeletron.Services;
using Skeletron.Services.Interfaces;

namespace Skeletron.Commands;

public class JokeCommands : SkBaseCommandModule
{
    private ILogger<JokeCommands> _logger;
    private IJokeService _jokeService;
    
    public JokeCommands(ILogger<JokeCommands> logger, IJokeService jokeService)
    {
        _logger = logger;
        _jokeService = jokeService;
        
        ModuleName = "Jokes";
    }
    
    [Command("joke")]
    [Description("Выдать рандомный анекдот")]
    public async Task RandomJoke(CommandContext ctx)
    {
        if (ctx.Channel.Name.Contains("politics"))
        {
            var joke = _jokeService.GetRandomPoliticalJoke();
            await ctx.RespondAsync(joke);
            return;
        }

        await ctx.RespondAsync("Я не знаю подходящих шуток, которые будут уместны в этом канале.");
    }
}