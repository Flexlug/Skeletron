using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Skeletron.Converters;
using Skeletron.Services.Interfaces;

namespace Skeletron.Services;

public class MessageDeleteService : IMessageDeleteService
{
    private readonly DiscordEmoji _redCrossEmoji;
    private readonly ILogger<MessageDeleteService> _logger;
    
    public MessageDeleteService(DiscordClient client, OsuEmoji emoji, ILogger<MessageDeleteService> logger)
    {
        _redCrossEmoji = emoji.MissEmoji();
        _logger = logger;
        
        client.MessageReactionAdded += DeleteResentMessage;
        _logger.LogInformation($"{nameof(MessageDeleteService)} loaded");
    }

    private async Task DeleteResentMessage(DiscordClient sender, MessageReactionAddEventArgs reactionInfo)
    {
        if (reactionInfo.User.Id == Bot.SKELETRON_UID)
            return;

        if (reactionInfo.Emoji != _redCrossEmoji)
            return;

        var currentChannel = reactionInfo.Channel;
        var currentMessageId = reactionInfo.Message.Id;
        var currentMessage = await currentChannel.GetMessageAsync(currentMessageId);
        if (!currentMessage.Reactions.Any(x => x.Emoji == _redCrossEmoji && x.IsMe))
            return;

        var respondedMessage = currentMessage.ReferencedMessage;
        if (respondedMessage is null)
            return;

        if (respondedMessage.Author.Id != reactionInfo.User.Id)
            return;
        
        var allMessagesAfterCurrent = await currentChannel.GetMessagesAfterAsync(currentMessageId, 5);

        var deletingMessages = new List<DiscordMessage>();
        deletingMessages.Add(reactionInfo.Message);

        foreach (var message in allMessagesAfterCurrent)
        {
            if (message.Author.Id != Bot.SKELETRON_UID)
            {
                break;
            }
                
            deletingMessages.Add(message);
        }

        await currentChannel.DeleteMessagesAsync(deletingMessages);
    }
}