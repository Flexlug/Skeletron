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

            _client.MessageCreated += ResendMessage;

            _logger.LogInformation("MessageResendService loaded");
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

        private async Task ResendMessage(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            var messageWithLink = e.Message;
            var msgParams = GetMessageUrl(e.Message.Content);

            if (msgParams is null)
                return;

            var guild = await _client.GetGuildAsync(msgParams.Item1);
            var currentChannel = guild.GetChannel(msgParams.Item2);
            var resendingMessage = await currentChannel.GetMessageAsync(msgParams.Item3);

            DiscordEmbedBuilder resentMessageBuilder = new DiscordEmbedBuilder()
                .WithAuthor(name: resendingMessage.Author.Username, iconUrl: resendingMessage.Author.AvatarUrl)
                .WithDescription(resendingMessage.Content)
                .WithFooter(
                    $"Guild: {resendingMessage.Channel.Guild.Name}, Channel: {resendingMessage.Channel.Name}, Time: {resendingMessage.CreationTimestamp}");

            DiscordMessage resentMessage = await messageWithLink.RespondAsync(resentMessageBuilder);
            
            // Добавить реакцию, чтобы сообщение можно было удалить через MessageDeleteService
            await resentMessage.CreateReactionAsync(_redCrossEmoji);

            if (resendingMessage.Attachments is not null && resendingMessage.Attachments.Count != 0)
            {
                DiscordMessageBuilder mainMsg = new DiscordMessageBuilder();
                StringBuilder sb = new StringBuilder();

                foreach (var attachment in resendingMessage.Attachments)
                    sb.AppendLine(attachment.Url);

                mainMsg.WithContent(sb.ToString());
                
                await e.Channel.SendMessageAsync(mainMsg);
            }

            if (resendingMessage.Embeds is not null && resendingMessage.Embeds.Count != 0)
            {
                DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder()
                    .AddEmbeds(resendingMessage.Embeds);

                await e.Channel.SendMessageAsync(msgBuilder);
            }
        }
    }
}
