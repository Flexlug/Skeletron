﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using Skeletron.Converters;
using Skeletron.Services.Interfaces;
using Skeletron.Configurations;

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
        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
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
                _logger.LogWarning($"Ошибка парсинга поста из группы VK. id: {post_id}, expetion: {e.Message} {e.Source} {e.StackTrace}");
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
                _logger.LogDebug($"Не удалось получить пост VK по из коллекции WallPosts. id: {post_id}");
                return;
            }
            #endregion

            #region Main content
            var sb = new StringBuilder();
            var builder = new DiscordEmbedBuilder();

            IReadOnlyCollection<Group> historyGroups = null;
            // Repost handle
            if (p.CopyHistory is not null && p.CopyHistory.Count != 0)
            {
                historyGroups = await _api.Groups.GetByIdAsync(
                    p.CopyHistory.Select(x => Math.Abs((long)x.OwnerId).ToString())
                                 .Append(source_post.FromId.ToString().Replace("-", string.Empty))
                                 .ToList()
                    , null, null);

                sb.AppendLine($"{(string.IsNullOrEmpty(p.Text) ? string.Empty : p.Text)}");

                for (int i = 0; i < p.CopyHistory.Count; i++)
                {
                    Post repost = p.CopyHistory[i];
                    sb.AppendLine($"{new string('➦', i + 1)} *repost from [**{historyGroups.ElementAt(i).Name}**](http://vk.com/wall{repost.FromId}_{repost.Id})*");
                    if (!string.IsNullOrEmpty(repost.Text))
                        sb.AppendLine(repost.Text);
                    sb.AppendLine();
                }

                p = p.CopyHistory.Last();
            }
            else
            {
                sb.AppendLine(p.Text);
                sb.AppendLine();
            }

            #endregion

            #region Author
            
            if (isGroup)
            {
                var group = post.Groups.FirstOrDefault();

                if (group is null)
                {
                    _logger.LogInformation($"Не удалось получить информацию об авторе поста VK {p.FromId}");
                    return;
                }
                
                builder.WithAuthor(group.Name, $"http://vk.com/wall{source_post.OwnerId}_{source_post.Id}", group.Photo50.AbsoluteUri);
            }
            else
            {
                var user = post.Profiles.FirstOrDefault();

                if (user is null)
                {
                    _logger.LogInformation($"Не удалось получить информацию об авторе поста VK {p.FromId}");
                    return;
                }
                builder.WithAuthor($"{user.FirstName} {user.LastName}", $"http://vk.com/wall{source_post.OwnerId}_{source_post.Id}", user.Photo50.AbsoluteUri);
            }

            #endregion

            #region Attachments
            bool hasImage = false;
            int imgCount = 0;
            List<string> imageUrls = new();
            List<(string, string)> fields = new();

            foreach (var a in p.Attachments)
            {
                switch (a.Type.FullName)
                {
                    case "VkNet.Model.Attachments.Photo":
                        Photo photo = a.Instance as Photo;

                        var size = photo.Sizes.First(x => x.Width == photo.Sizes.Max(x => x.Width) && x.Height == photo.Sizes.Max(x => x.Height));

                        imageUrls.Add(size.Url.AbsoluteUri);

                        break;

                    case "VkNet.Model.Attachments.Video":
                        Video video = a.Instance as Video;

                        sb.Append($"[[**видео**](https://vk.com/video{video.OwnerId}_{video.Id})] ");

                        break;

                    case "VkNet.Model.Attachments.Poll":
                        Poll poll = a.Instance as Poll;

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
            builder.WithDescription(sb.ToString());

            if (fields.Count != 0)
                foreach (var field in fields)
                    builder.AddField(field.Item1, field.Item2);

            if (imageUrls.Count != 0)
                builder.WithImageUrl(imageUrls[0])
                       .WithUrl($"http://vk.com/wall{post_id}");

            List<DiscordMessageBuilder> messages = new();

            List<DiscordEmbedBuilder> finalEmbeds = new();
            finalEmbeds.Add(builder);
            for (int i = 1; i < imageUrls.Count; i++)
                finalEmbeds.Add(new DiscordEmbedBuilder()
                    .WithImageUrl(imageUrls[i])
                    .WithUrl($"http://vk.com/wall{post_id}"));

            finalEmbeds
                [(finalEmbeds.Count - 1) / 4 * 4]
                .WithFooter($"Лайков: {source_post.Likes.Count}, Репостов: {source_post.Reposts.Count}, Просмотров: {source_post.Views.Count}", @"https://vk.com/images/icons/favicons/fav_logo.ico")
                .WithTimestamp(source_post.Date);
            
            for (int i = 0; i < finalEmbeds.Count; i += 4)
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