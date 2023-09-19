using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.Logging;

namespace Skeletron.Commands
{
    /// <summary>
    /// Class with demonstration of possibilities.
    /// Disclaimer: The code shouldn't be used exactly this way as it is, it's just there to give you some ideas.
    /// </summary>
    [Hidden, RequireGuild]
    public sealed class DemonstrationCommands : SkBaseCommandModule
    {
        private ILogger<DemonstrationCommands> logger;
        private DiscordClient client;

        public DemonstrationCommands(ILogger<DemonstrationCommands> logger,
                                     DiscordClient client)
        {
            this.logger = logger;
            this.client = client;

            logger.LogInformation("DemostrationCommands loaded");
        }

        /// <summary>
        /// With this command you can send a message to any discord server (Guild) which the bot is a part of,
        /// as long the Bot is on the server and got enough permissions to send a message to the targeted channel.
        /// <see cref="https://support.discordapp.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID-"/>
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="guildId"></param>
        /// <param name="channelId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [Command("sendtoguildchannel"), Aliases("stgc"), Description("Send a message to a specified channel in a special guild"), RequireOwner]
        public async Task SendToChannelAsync(CommandContext commandContext,
            [Description("Id of the target guild")] ulong guildId,
            [Description("Id of the target channel")] ulong channelId,
            [Description("Message to send"), RemainingText] string message)
        {
            var guild = await commandContext.Client.GetGuildAsync(guildId);
            var channel = guild.GetChannel(channelId);
            await channel.SendMessageAsync(message);
        }
    }
}
