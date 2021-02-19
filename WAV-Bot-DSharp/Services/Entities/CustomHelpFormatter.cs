using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace WAV_Bot_DSharp.Services.Entities
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;
        // protected StringBuilder _strBuilder;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
             _embed = new DiscordEmbedBuilder();
            // _strBuilder = new StringBuilder();

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
            foreach (var cmd in cmds)
            {
                 _embed.AddField(cmd.Name, cmd.Description);            
                // _strBuilder.AppendLine($"{cmd.Name} - {cmd.Description}");
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
             return new CommandHelpMessage(embed: _embed);
            // return new CommandHelpMessage(content: _strBuilder.ToString());
        }
    }
}
