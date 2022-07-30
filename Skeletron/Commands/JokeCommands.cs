using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Skeletron.Services;

namespace Skeletron.Commands;

public class JokeCommands : SkBaseCommandModule
{
    private ILogger<JokeCommands> _logger;
    private JokeService _jokeService;
    
    public JokeCommands(ILogger<JokeCommands> logger, JokeService jokeService)
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

        await ctx.RespondAsync("Я не знаю подходящих шуток для этого канала.");
    }
}