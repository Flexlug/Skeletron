using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Skeletron.Configurations;
using Skeletron.Converters;
using Skeletron.Services.Interfaces;
using VkNet;
using VkNet.Model;

namespace Skeletron.Services
{
    internal class VkPostToMessageService : IVkPostToMessageService
    {
        private VkApi _api;
        private VkRegex _regex;
        private readonly DiscordEmoji _redCrossEmoji;
        private ILogger<VkPostToMessageService> _logger;

        public VkPostToMessageService(Settings settings,
                         VkRegex regex,
                         DiscordClient client,
                         OsuEmoji emoji,
                         ILogger<VkPostToMessageService> logger)
        {
            _api = new VkApi();
            _api.Authorize(new ApiAuthParams() { AccessToken = settings.VkSecret,  });

            _regex = regex;
            _logger = logger;
            _redCrossEmoji = emoji.MissEmoji();

            client.MessageCreated += Client_MessageCreated;

            _logger.LogInformation($"{nameof(VkPostToMessageService)} loaded");
        }
        private async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            string id;

            id = _regex.TryGetGroupPostIdFromExportUrl(e.Message.Content);
            if (!string.IsNullOrWhiteSpace(id))
                await ParseGroupPost(id, e.Message, e.Channel);

            id = _regex.TryGetGroupPostIdFromRegularUrl(e.Message.Content);
            if (!string.IsNullOrWhiteSpace(id))
                await ParseGroupPost(id, e.Message, e.Channel);
        }

        private async Task ParseGroupPost(string post_id, DiscordMessage originalMessage, DiscordChannel channel)
        {
            // Group id starts from -
            bool isGroup = post_id[0] == '-';
            
            #region Validation
            WallGetObject post = null;
            try
            {
                post = _api.Wall.GetById(new string[] { post_id }, true);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Ошибка парсинга поста из группы VK. id: {post_id}, exception: {e.Message} {e.Source} {e.StackTrace}");
                return;
            }

            if (post is null || post.WallPosts is null || post.WallPosts.Count == 0)
            {
                _logger.LogDebug($"Не удалось получить пост VK по ссылке. id: {post_id}");
                return;
            }

            Post p = post.WallPosts.FirstOrDefault();
            Post source_post = p;
            if (p is null)
            {
                _logger.LogDebug($"Не удалось получить пост VK из коллекции WallPosts. id: {post_id}");
                return;
            }
            #endregion

            #region Main content

            var postMessage = p.Text;

            var repostInfo = new StringBuilder();
            
            // Repost handle
            if (p.CopyHistory is not null && p.CopyHistory.Count != 0)
            {
                var historyGroups = await _api.Groups.GetByIdAsync(
                    p.CopyHistory.Select(x => Math.Abs((long)x.OwnerId).ToString())
                        .Append(source_post.FromId.ToString().Replace("-", string.Empty))
                        .ToList()
                    , null, null);

                repostInfo.AppendLine($"{(string.IsNullOrEmpty(p.Text) ? string.Empty : p.Text)}");

                for (int i = 0; i < p.CopyHistory.Count; i++)
                {
                    Post repost = p.CopyHistory[i];
                    repostInfo.AppendLine(
                        $"{new string('➦', i + 1)} *repost from [**{historyGroups.ElementAt(i).Name}**](http://vk.com/wall{repost.FromId}_{repost.Id})*");
                    if (!string.IsNullOrEmpty(repost.Text))
                        repostInfo.AppendLine(repost.Text);
                    repostInfo.AppendLine();
                }

                p = p.CopyHistory.Last();
            }

            #endregion

            #region Author

            string authorName, authorUrl, authorIconUrl;
            
            if (isGroup)
            {
                var group = post.Groups.FirstOrDefault();

                if (group is null)
                {
                    _logger.LogInformation($"Не удалось получить информацию об авторе поста VK {p.FromId}");
                    return;
                }

                authorName = group.Name;
                authorUrl = $"http://vk.com/wall{source_post.OwnerId}_{source_post.Id}";
                authorIconUrl = group.Photo50.AbsoluteUri;
            }
            else
            {
                var user = post.Profiles.FirstOrDefault();

                if (user is null)
                {
                    _logger.LogInformation($"Не удалось получить информацию об авторе поста VK {p.FromId}");
                    return;
                }

                authorName = user.FirstName + " " + user.LastName;
                authorUrl = $"http://vk.com/wall{source_post.OwnerId}_{source_post.Id}";
                authorIconUrl = user.Photo50.AbsoluteUri;
            }

            #endregion

            #region Attachments
            
            var hasImage = false;
            var imgCount = 0;
            var imageUrls = new List<string>();
            var fields = new List<(string, string)>();

            var videoUrls = new StringBuilder();

            foreach (var a in p.Attachments)
            {
                switch (a.Instance)
                {
                    case Photo photo:
                        var size = photo.Sizes.First(x => x.Width == photo.Sizes.Max(x => x.Width) && x.Height == photo.Sizes.Max(x => x.Height));
                        imageUrls.Add(size.Url.AbsoluteUri);
                        break;

                    case Video video:
                        videoUrls.Append($"[[**видео**](https://vk.com/video{video.OwnerId}_{video.Id})] ");
                        break;

                    case Poll poll:
                        (string, string) strPoll = new(
                            poll.Question,
                            string.Join(' ',
                                poll.Answers
                                    .Select(x => $"**{x.Text}** - {x.Votes} ({x.Rate:#.##}%)\n")));

                        fields.Add(strPoll);
                        break;

                    default:
                        _logger.LogDebug($"Неизвестное vk вложение: {a.Type.Name}");
                        break;
                }
            }
            #endregion

            #region Constructing message
            
            List<DiscordMessageBuilder> messages = new();
            List<DiscordEmbedBuilder> finalEmbeds = new();

            int firstEmbedWithImageIndex = 0; // also if this index is NOT 0 the message is potentially long 
            
            if (repostInfo.Length + postMessage.Length + videoUrls.Length > 4096)
            {
                // Split message in 3 embeds, where:
                // 1 embed: author and repost info
                // 2 embed: content
                // 3 embed: attachments, likes, reposts and other info
                
                // 1-st embed
                if (repostInfo.Length != 0)
                {
                    finalEmbeds.Add(new DiscordEmbedBuilder()
                        .WithDescription(repostInfo.ToString()));
                }
                
                // 2-nd embed can be splitted in more embeds. Each one contains post text, splitted in chunks
                var messageChunks = new List<string>();
                
                if (postMessage.Length >= 4096)
                {   
                    // Split post text in chunks by spaces
                    int startIndex = 0,
                        endIndex = 0;

                    do
                    {
                        endIndex += 4096;

                        do
                        {
                            endIndex--;
                        } while (postMessage[endIndex] != ' ');

                        var strChunk = postMessage.Substring(startIndex, endIndex);
                        messageChunks.Add(strChunk);

                        startIndex = endIndex;
                    } while (postMessage.Length - endIndex > 4096);

                    var finalStrChunk = postMessage.Substring(endIndex);
                    messageChunks.Add(finalStrChunk);
                }
                else
                {
                    messageChunks.Add(postMessage.ToString());
                }
                
                foreach (var chunk in messageChunks)
                {
                    finalEmbeds.Add(new DiscordEmbedBuilder()
                        .WithDescription(chunk));
                }
                
                // 3rd embed
                if (videoUrls.Length != 0)
                {
                    finalEmbeds.Add(new DiscordEmbedBuilder()
                        .WithDescription(videoUrls.ToString()));
                }

                firstEmbedWithImageIndex = finalEmbeds.Count - 1;
            }
            else
            {
                var concatedPostMessage = new StringBuilder();

                if (repostInfo.Length != 0)
                {
                    concatedPostMessage.Append(repostInfo);
                }

                concatedPostMessage.Append(postMessage);

                if (videoUrls.Length != 0)
                {
                    concatedPostMessage.Append(videoUrls);
                }
                
                finalEmbeds.Add(new DiscordEmbedBuilder()
                    .WithDescription(concatedPostMessage.ToString()));
            }

            // In case, when post length is less than 4096 symbols topBuilder and bottomBuilder will be the same 
            var topBuilder = finalEmbeds.First();
            topBuilder.WithAuthor(authorName, authorUrl, authorIconUrl);

            if (imageUrls.Count != 0)
            {
                finalEmbeds[firstEmbedWithImageIndex]
                    .WithImageUrl(imageUrls[0])
                    .WithUrl($"http://vk.com/wall{post_id}");
                
                // If embed contains more then 1 image, the top embed should contain image url too
                for (int i = 1; i < imageUrls.Count; i++)
                    finalEmbeds.Add(new DiscordEmbedBuilder()
                        .WithImageUrl(imageUrls[i])
                        .WithUrl($"http://vk.com/wall{post_id}"));
            }

            // bottomBuilder is last one, if:
            // - there's more then 4 images
            // - message is potentially very long
            var bottomBuilder = finalEmbeds.Count > 4 || firstEmbedWithImageIndex != 0 ? 
                    finalEmbeds.Last() :
                    finalEmbeds.First();
            
            if (fields.Count != 0)
                foreach (var field in fields)
                    bottomBuilder.AddField(field.Item1, field.Item2);

            bottomBuilder
                .WithFooter($"Лайков: {source_post.Likes.Count}, Репостов: {source_post.Reposts.Count}, Просмотров: {source_post.Views.Count}", @"https://vk.com/images/icons/favicons/fav_logo.ico")
                .WithTimestamp(source_post.Date);

            // The sum characters limit per message (including embed description, fields etc.) is 6000.
            // We'll just post potentially long embeds in seperate messages
            // If there's a repost info - we'll merge it with first msg
            if (firstEmbedWithImageIndex != 0)
            {
                var startIndex = 0;
                
                if (repostInfo.Length != 0)
                {
                    messages.Add(new DiscordMessageBuilder()
                        .AddEmbeds(finalEmbeds.Take(2).Select(x => x.Build()).ToList()));

                    startIndex = 2;
                }

                for (int i = startIndex; i < firstEmbedWithImageIndex; i++)
                {
                    messages.Add(new DiscordMessageBuilder()
                        .AddEmbed(finalEmbeds[i].Build()));
                }
            }

            for (int i = firstEmbedWithImageIndex; i < finalEmbeds.Count; i += 4)
                messages.Add(new DiscordMessageBuilder()
                    .AddEmbeds(finalEmbeds.Skip(i).Take(4).Select(x => x.Build()).ToList()));
            
            bool firstMsg = true;
            foreach (var sendingMessage in messages)
            {
                if (firstMsg)
                {
                    var sentMesage = await originalMessage.RespondAsync(sendingMessage);
                    
                    // Добавить реакцию, чтобы сообщение можно было удалить через MessageDeleteService
                    await sentMesage.CreateReactionAsync(_redCrossEmoji);
                    firstMsg = false;
                    continue;
                }
                
                await sendingMessage.SendAsync(channel);
            }

            #endregion
        }
    }
}
