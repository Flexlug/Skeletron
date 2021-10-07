using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            api.Authorize(new ApiAuthParams() { ClientSecret = settings.VkSecret });

            this.regex = regex;
            this.logger = logger;

            client.MessageCreated += Client_MessageCreated;
        }

        private async Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string id = regex.TryGetGroupPostId(e.Message.Content);
            if (!string.IsNullOrWhiteSpace(id))
                await ParseGroupPost(id, e.Channel);
        }

        private async Task ParseGroupPost(string post_id, DiscordChannel channel)
        {
            WallGetObject post = null;
            try
            {
                post = api.Wall.GetById(new string[] { post_id });
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

            await channel.SendMessageAsync(new DiscordEmbedBuilder()
                .WithDescription(p.Text)
                .WithFooter($"Лайков: {p.Likes}, Репостов: {p.Reposts}, Просмотров: {p.Views}"));
        }
    }
}
