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
    private readonly ILogger<MessageResendService> _logger;
    
    public MessageDeleteService(DiscordClient client, OsuEmoji emoji, ILogger<MessageResendService> logger)
    {
        _redCrossEmoji = emoji.MissEmoji();
        _logger = logger;
        
        client.MessageReactionAdded += DeleteResentMessage;
        _logger.LogInformation("MessageDeleteService loaded");
    }

    private async Task DeleteResentMessage(DiscordClient sender, MessageReactionAddEventArgs reactionInfo)
    {
        if (reactionInfo.User.Id == Bot.SKELETRON_UID)
            return;

        if (reactionInfo.Emoji != _redCrossEmoji)
            return;

        var currentMessage = reactionInfo.Message;
        if (!currentMessage.Reactions.Any(x => x.Emoji == _redCrossEmoji && x.IsMe))
            return;

        var respondedMessage = currentMessage.Reference;
        if (respondedMessage is null)
            return;

        if (respondedMessage.Message.Author.Id != reactionInfo.User.Id)
            return;
            
        var currentTextChannel = currentMessage.Channel;
        var currentMessageId = currentMessage.Id;
        var allMessagesAfterCurrent = await currentTextChannel.GetMessagesAfterAsync(currentMessageId, 5);

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

        await currentTextChannel.DeleteMessagesAsync(deletingMessages);
    }
}