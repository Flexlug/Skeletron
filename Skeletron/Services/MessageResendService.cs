using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Skeletron.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using Skeletron.Converters;

namespace Skeletron.Services
{
    public class MessageResendService : IMessageResendService
    {
        private readonly DiscordClient _client;
        private readonly Regex _messagePattern = new(@"(?<!\\)https?:\/\/(?:ptb\.|canary\.)?discord\.com\/channels\/(\d+)\/(\d+)\/(\d+)", RegexOptions.Compiled);
        private readonly DiscordEmoji _redCrossEmoji;
        private readonly ILogger<MessageResendService> _logger;

        public MessageResendService(DiscordClient client, OsuEmoji emoji, ILogger<MessageResendService> logger)
        {
            _client = client;
            _logger = logger;
            _redCrossEmoji = emoji.MissEmoji();

            _client.MessageCreated += MessageResender;
            _client.MessageReactionAdded += CheckResendedMessageDeletion;

            _logger.LogInformation("UtilityService loaded");
        }

        private async Task CheckResendedMessageDeletion(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.User.Id == Bot.SKELETRON_UID)
                return;

            if (e.Emoji.Id != _redCrossEmoji.Id)
                return;

            var currentMessage = e.Message;
            if (!currentMessage.Reactions.Any(x => x.Emoji == _redCrossEmoji && x.IsMe))
                return;
            
            var currentTextChannel = currentMessage.Channel;
            var currentMessageId = currentMessage.Id;
            var allMessagesAfterCurrent = await currentTextChannel.GetMessagesBeforeAsync(currentMessageId, 5);

            var deletingMessages = new List<DiscordMessage>();
            deletingMessages.Add(e.Message);

            foreach (var message in allMessagesAfterCurrent)
            {
                if (message.Author.Id != Bot.SKELETRON_UID)
                {
                    break;
                }
                
                deletingMessages.Add(message);
            }

            await currentTextChannel.DeleteMessagesAsync(deletingMessages);
        }

        public Tuple<ulong, ulong, ulong> GetMessageUrl(string msg)
        {
            Match match = _messagePattern.Match(msg);

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
                DiscordGuild guild = await _client.GetGuildAsync(msgParams.Item1);
                DiscordChannel ch = guild.GetChannel(msgParams.Item2);
                msg = await ch.GetMessageAsync(msgParams.Item3);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while parsing message link: {ex.Message} {ex.StackTrace}");
                return;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: msg.Author.Username, iconUrl: msg.Author.AvatarUrl)
                .WithDescription(msg.Content)
                .WithFooter($"Guild: {msg.Channel.Guild.Name}, Channel: {msg.Channel.Name}, Time: {msg.CreationTimestamp}");

            DiscordMessage lastMessage = await e.Channel.SendMessageAsync(embed);
            
            if (msg.Attachments is not null && msg.Attachments.Count != 0)
            {
                DiscordMessageBuilder mainMsg = new DiscordMessageBuilder();
                StringBuilder sb = new StringBuilder();

                foreach (var attachment in msg.Attachments)
                    sb.AppendLine(attachment.Url);

                mainMsg.WithContent(sb.ToString());

                lastMessage = await e.Channel.SendMessageAsync(mainMsg);
            }

            if (msg.Embeds is not null && msg.Embeds.Count != 0)
            {
                DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder()
                    .AddEmbeds(msg.Embeds);

                lastMessage = await e.Channel.SendMessageAsync(msgBuilder);
            }

            await lastMessage.CreateReactionAsync(_redCrossEmoji);
        }
    }
}
