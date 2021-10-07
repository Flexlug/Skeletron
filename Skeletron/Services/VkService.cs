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
            catch(Exception e)
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
            if (p is null)
            {
                logger.LogDebug($"Не удалось получить пост VK по из коллекции WallPosts. id: {post_id}");
                return;
            }
            #endregion

            #region Main content
            var sb = new StringBuilder();
            var builder = new DiscordEmbedBuilder()
                .WithFooter($"Лайков: {p.Likes.Count}, Репостов: {p.Reposts.Count}, Просмотров: {p.Views.Count}", @"https://vk.com/images/icons/favicons/fav_logo.ico")
                .WithUrl($"http://vk.com/wall{post_id}")
                .WithTitle(p.Date?.ToString() ?? "");
            
            sb.AppendLine(p.Text);
            sb.AppendLine();

            #endregion

            #region Author
            Group group = null;
            try
            {
                group = (await api.Groups.GetByIdAsync(new string[] { p.FromId.ToString().Replace("-", string.Empty) }, null, null)).First();

                builder.WithAuthor(group.Name, $"http://vk.com/club{group.Id}", group.Photo50.AbsoluteUri);
            }
            catch(Exception e)
            {
                logger.LogInformation($"Не удалось получить информацию об авторе поста VK {p.FromId}");
            }

            #endregion

            #region Attachments
            bool hasImage = false;
            int imgCount = 0;

            foreach (var a in p.Attachments)
            {
                switch (a.Type.FullName)
                {
                    case "VkNet.Model.Attachments.Photo":
                        Photo photo = a.Instance as Photo;

                        var size = photo.Sizes.First(x => x.Width == photo.Sizes.Max(x => x.Width) && x.Height == photo.Sizes.Max(x => x.Height));

                        if (hasImage)
                        {
                            sb.Append($"[Картинка{++imgCount}]({size.Url.AbsoluteUri}), ");
                        }
                        else
                        {
                            builder.WithImageUrl(size.Url);
                            hasImage = true;
                            sb.Append($"[Картинка{++imgCount}]({size.Url.AbsoluteUri}) ");
                        }

                        break;

                    default:
                        logger.LogDebug($"Неизвестное vk вложение: {a.Type.Name}");
                        break;
                }
            }
            #endregion

            builder.WithDescription(sb.ToString());

            await channel.SendMessageAsync(builder);
        }
    }
}
