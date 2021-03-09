﻿using DSharpPlus;
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
            this.client.MessageCreated += Client_DetectSayHi;
        }

        private async Task Client_DetectSayHi(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string msg = e.Message.Content.ToLower();
            if (msg.Contains("привет скелетик") || msg.Contains("скелетик привет") || msg.Contains("привет виталий") || msg.Contains("вставай припадочный") || msg.Contains("привет виталя"))
                await e.Message.RespondAsync(":skull:");
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