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
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _embed.WithTitle("Command description");

            CommandOverload commandOverload = command.Overloads.First();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"`sk!{command.Name} {string.Join(' ', commandOverload.Arguments.Select(x => $"[{ x.Name}]").ToList())}`\n{command.Description}");
            sb.AppendLine();

            if (command.Aliases?.Count != 0)
            {
                sb.AppendLine("**Aliases:**");
                foreach (string alias in command.Aliases)
                    sb.AppendLine(alias);

                sb.AppendLine();
            }

            sb.AppendLine("**Arguments:**");
            foreach (var c in command.Overloads.First().Arguments)
                sb.AppendLine($"`{c.Name}`: {c.Description}");
            sb.AppendLine();

            if (command.ExecutionChecks?.Count != 0)
            {
                sb.AppendLine("**Execution checks:**");
                sb.AppendLine(string.Join(' ', command.ExecutionChecks.Select(x => x.ToString().Split('.').Last())));
            }

            _embed.WithDescription(sb.ToString());            

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
            _embed.WithTitle("Commands overview");

            return this;
        }

        public override CommandHelpMessage Build()
        {
             return new CommandHelpMessage(embed: _embed);
        }
    }
}
