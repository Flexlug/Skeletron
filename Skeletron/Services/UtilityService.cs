﻿using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Skeletron.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Skeletron.Services
{
    public class UtilityService : IUtilityService
    {
        private DiscordClient client;

        private Regex messagePattern = new Regex(@"(?<!\\)https?:\/\/(?:ptb\.|canary\.)?discord\.com\/channels\/(\d+)\/(\d+)\/(\d+)");

        private ILogger<UtilityService> logger;

        public UtilityService(DiscordClient client, ILogger<UtilityService> logger)
        {
            this.client = client;
            this.logger = logger;

            client.MessageCreated += MessageResender;

            logger.LogInformation("UtilityService loaded");
        }

        public Tuple<ulong, ulong, ulong> GetMessageUrl(string msg)
        {
            Match match = messagePattern.Match(msg);

            if (match is null || match.Groups.Count != 4)
                return null;

            ulong guildId, channelId, messageId;

            if (ulong.TryParse(match.Groups[1].Value, out guildId) &&
                ulong.TryParse(match.Groups[2].Value, out channelId) &&
                ulong.TryParse(match.Groups[3].Value, out messageId))
                return Tuple.Create(guildId, channelId, messageId);

            return null;
        }

        private async Task MessageResender(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var msgParams = GetMessageUrl(e.Message.Content);

            if (msgParams is null)
                return;

            DiscordMessage msg = null;

            try
            {
                DiscordGuild guild = await client.GetGuildAsync(msgParams.Item1);
                DiscordChannel ch = guild.GetChannel(msgParams.Item2);
                msg = await ch.GetMessageAsync(msgParams.Item3);
            }
            catch(Exception ex)
            {
                logger.LogError($"Error while parsing message link: {ex.Message} {ex.StackTrace}");
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: msg.Author.Username, iconUrl: msg.Author.AvatarUrl)
                .WithDescription(msg.Content)
                .WithFooter($"Guild: {msg.Channel.Guild.Name}, Channel: {msg.Channel.Name}, Time: {msg.CreationTimestamp}");

            try
            {
                await e.Channel.SendMessageAsync(embed);

                if (msg.Attachments is not null && msg.Attachments.Count != 0)
                {
                    DiscordMessageBuilder mainMsg = new DiscordMessageBuilder();
                    StringBuilder sb = new StringBuilder();

                    foreach (var attachment in msg.Attachments)
                        sb.AppendLine(attachment.Url);

                    mainMsg.WithContent(sb.ToString());

                    await e.Channel.SendMessageAsync(mainMsg);
                }

                if (msg.Embeds is not null && msg.Embeds.Count != 0)
                {
                    DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder()
                        .AddEmbeds(msg.Embeds);

                    await e.Channel.SendMessageAsync(msgBuilder);
                }


            }
            catch (Exception ex)
            {
                logger.LogError($"{ex.Message} {ex.StackTrace}");
            }
        }
    }
}
