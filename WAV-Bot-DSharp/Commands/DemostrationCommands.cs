using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp.Commands
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

        [Command("button-example"), Hidden, RequireOwner]
        public async Task ButtonDemo(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            var buttons = new List<DiscordButtonComponent>(new[] { new DiscordButtonComponent(ButtonStyle.Primary, "primaryAdd", "+1"),
                                                                   new DiscordButtonComponent(ButtonStyle.Danger, "dangerAdd", "", emoji: new DiscordComponentEmoji("⚠"))});

            int primary = 0,
                danger = 0;

            var msg = await new DiscordMessageBuilder()
                .AddComponents(buttons)
                .WithContent($"Primary: {primary}\nDanger: {danger}")
                .SendAsync(ctx.Channel);

            while (true)
            {
                var resp = await interactivity.WaitForButtonAsync(msg, buttons, TimeSpan.FromSeconds(10));

                if (resp.TimedOut)
                {
                    await msg.ModifyAsync(new DiscordMessageBuilder()
                        .WithContent($"RESULT:\n\nPrimary: {primary}\nDanger: {danger}"));
                    break;
                }

                switch (resp.Result.Id)
                {
                    case "primaryAdd":
                        primary++;
                        break;
                    case "dangerAdd":
                        danger++;
                        break;
                    default:
                        break;
                        
                }

                await resp.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                                                                                            .WithContent($"Primary: {primary}\nDanger: {danger}")
                                                                                                            .AddComponents(buttons));
            }

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
