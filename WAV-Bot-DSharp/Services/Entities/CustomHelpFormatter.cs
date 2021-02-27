using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WAV_Bot_DSharp.Commands;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
             _embed = new DiscordEmbedBuilder();

            // Help formatters do support dependency injection.
            // Any required services can be specified by declaring constructor parameters. 

            // Other required initialization here ...
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
             _embed.AddField(command.Name, command.Description);            
            // _strBuilder.AppendLine($"{command.Name} - {command.Description}");

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            Dictionary<string, List<string>> comsDict = new Dictionary<string, List<string>>();

            foreach (var commands in cmds)
            {
                if (commands.Module is null)
                    continue;

                if (!(commands.Module is SingletonCommandModule))
                    continue;

                SkBaseCommandModule skModule = (commands.Module as SingletonCommandModule).Instance as SkBaseCommandModule;

                if (string.IsNullOrEmpty(skModule.ModuleName))
                    continue;

                if (!comsDict.ContainsKey(skModule.ModuleName))
                    comsDict.Add(skModule.ModuleName, new List<string>());

                comsDict[skModule.ModuleName].Add($"`{commands.Name}` : {commands.Description}");
            }

            foreach (var kvp in comsDict)
                _embed.AddField(kvp.Key, string.Join('\n', kvp.Value));
            _embed.WithTitle("Bot help");

            return this;
        }

        public override CommandHelpMessage Build()
        {
             return new CommandHelpMessage(embed: _embed);
        }
    }
}
