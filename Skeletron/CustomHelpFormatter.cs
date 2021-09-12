using System.Text;
using System.Linq;
using System.Collections.Generic;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

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
            _embed.WithTitle("Описание команды");

            StringBuilder sb = new StringBuilder();

            bool countOverloads = false;
            if (command.Overloads.Count > 1)
                countOverloads = true;

            for (int i = 0; i < command.Overloads.Count; i++)
            {
                CommandOverload commandOverload = command.Overloads[i];

                if (countOverloads)
                    sb.AppendLine($"**__Вариант {i + 1}__**");

                sb.AppendLine($"```\nsk!{command.QualifiedName} {string.Join(' ', commandOverload.Arguments.Select(x => $"[{ x.Name}]").ToList())}```{command.Description}");
                sb.AppendLine();

                if (command.Aliases?.Count != 0)
                {
                    sb.AppendLine("**Алиасы:**");
                    foreach (string alias in command.Aliases)
                        sb.AppendLine(alias);

                    sb.AppendLine();
                }

                if (commandOverload?.Arguments.Count != 0)
                {
                    sb.AppendLine("**Аргументы:**");
                    foreach (var c in commandOverload.Arguments)
                        sb.AppendLine($"`{c.Name}`: {c.Description}");
                    sb.AppendLine();
                }

                //if (command.ExecutionChecks?.Count != 0)
                //{
                //    sb.AppendLine("**Execution checks:**");
                //    sb.AppendLine(string.Join(' ', command.ExecutionChecks.Select(x => x.ToString().Split('.').Last())));
                //}
                //sb.AppendLine();
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

                comsDict[skModule.ModuleName].Add($"`{commands.Name}`");
            }

            foreach (var kvp in comsDict)
                _embed.AddField(kvp.Key, string.Join(' ', kvp.Value));
            _embed.WithTitle("Список команд");

            return this;
        }

        public override CommandHelpMessage Build()
        {
             return new CommandHelpMessage(embed: _embed);
        }
    }
}
