using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using Skeletron.Converters;
using Skeletron.Services.Interfaces;
using Skeletron.Configurations;

namespace Skeletron.Services
{
    internal class VkService : IVkService
    {
        private VkApi api;
        private VkRegex regex;

        private ILogger<VkService> logger;

        public VkService(Settings settings,
                         VkRegex regex,
                         DiscordClient client,
                         ILogger<VkService> logger)
        {
            api = new VkApi();
            api.Authorize(new ApiAuthParams() { AccessToken = settings.VkSecret });

            this.regex = regex;
            this.logger = logger;

            client.MessageCreated += Client_MessageCreated;

            logger.LogInformation("VkService loaded");
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string id = string.Empty;

            id = regex.TryGetGroupPostIdFromExportUrl(e.Message.Content);
            if (!string.IsNullOrWhiteSpace(id))
                await ParseGroupPost(id, e.Channel);

            id = regex.TryGetGroupPostIdFromRegularUrl(e.Message.Content);
            if (!string.IsNullOrWhiteSpace(id))
                await ParseGroupPost(id, e.Channel);
        }

        private async Task ParseGroupPost(string post_id, DiscordChannel channel)
        {
            #region Validation
            WallGetObject post = null;
            try
            {
                post = await api.Wall.GetByIdAsync(new string[] { post_id });
            }
            catch (Exception e)
            {
                logger.LogWarning($"Ошибка парсинга поста из группы VK. id: {post_id}, expetion: {e.Message} {e.Source} {e.StackTrace}");
                return;
            }

            if (post is null || post.WallPosts is null || post.WallPosts.Count == 0)
            {
                logger.LogDebug($"Не удалось получить пост VK по ссылке. id: {post_id}");
                return;
            }

            Post p = post.WallPosts.FirstOrDefault();
            Post source_post = p;
            if (p is null)
            {
                logger.LogDebug($"Не удалось получить пост VK по из коллекции WallPosts. id: {post_id}");
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
                historyGroups = await api.Groups.GetByIdAsync(
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
            Group group = null;
            try
            {
                group = historyGroups?.Last() ?? (await api.Groups.GetByIdAsync(new string[] { source_post.FromId.ToString().Replace("-", string.Empty) }, null, null)).First();
                builder.WithAuthor(group.Name, $"http://vk.com/wall-{source_post.Id}_{source_post.Id}", group.Photo50.AbsoluteUri);
            }
            catch (Exception e)
            {
                logger.LogInformation($"Не удалось получить информацию об авторе поста VK {p.FromId}");
            }

            #endregion

            #region Attachments
            bool hasImage = false;
            int imgCount = 0;
            List<string> imageUrls = new();

            foreach (var a in p.Attachments)
            {
                switch (a.Type.FullName)
                {
                    case "VkNet.Model.Attachments.Photo":
                        Photo photo = a.Instance as Photo;

                        var size = photo.Sizes.First(x => x.Width == photo.Sizes.Max(x => x.Width) && x.Height == photo.Sizes.Max(x => x.Height));

                        imageUrls.Add(size.Url.AbsoluteUri);

                        //if (imgCount == 0)
                        //{
                        //    builder.WithImageUrl(size.Url.AbsoluteUri);
                        //    imgCount++;
                        //}
                        //else
                        //{
                        //    if (imgCount < 4)
                        //    {
                        //        imageUrls.Add(new DiscordEmbedBuilder()
                        //            .WithImageUrl(size.Url.AbsoluteUri)
                        //            .WithUrl($"http://vk.com/wall{post_id}"));
                        //        imgCount++;
                        //    }
                        //    else
                        //    {
                        //        sb.Append($"[Картинка{++imgCount}]({size.Url.AbsoluteUri}), ");
                        //    }
                        //}

                        break;

                    default:
                        logger.LogDebug($"Неизвестное vk вложение: {a.Type.Name}");
                        break;
                }
            }
            #endregion

            #region Constructing message
            builder.WithDescription(sb.ToString());

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
                .WithFooter($"Лайков: {source_post.Likes.Count}, Репостов: {source_post.Reposts.Count}, Просмотров: {source_post.Views.Count}", @"https://vk.com/images/icons/favicons/fav_logo.ico");

            for (int i = 0; i < finalEmbeds.Count; i += 4)
                messages.Add(new DiscordMessageBuilder()
                    .AddEmbeds(finalEmbeds.Skip(i).Take(4).Select(x => x.Build()).ToList()));

            foreach (var msg in messages)
                await msg.SendAsync(channel);

            #endregion
        }
    }
}
