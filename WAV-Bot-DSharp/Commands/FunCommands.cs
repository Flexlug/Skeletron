using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WAV_Bot_DSharp.Commands
{
    public sealed class FunCommands : SkBaseCommandModule
    {
        DiscordClient client;

        public FunCommands(DiscordClient client)
        {
            ModuleName = "Fun commands";

            this.client = client;
        }

        /// <summary>
        /// Prints out the latency between the bot and discord api servers.
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <returns></returns>
        [Command("hi"), Description("Greet the bot")]
        public async Task PingAsync(CommandContext commandContext)
        {
            await commandContext.RespondAsync($"{DiscordEmoji.FromName(client, ":skull:")}");
        }
    }
}
