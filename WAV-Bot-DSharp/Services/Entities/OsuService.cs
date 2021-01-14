using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using NLog;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using System.Threading;

namespace WAV_Bot_DSharp.Services.Entities
{
    /// <summary>
    /// Реализация сервиса, который будет отслеживать скриншоты и скоры из osu!
    /// </summary>
    public class OsuService : IOsuService
    {
        private DiscordEmoji[] _pollEmojiCache;
        private DiscordClient client;
        private ILogger logger;

        public OsuService(DiscordClient client, ILogger logger)
        {
            this.client = client;
            this.logger = logger;

            logger.Debug("Osu service started");
            ConfigureFilesInterceptor(client);
        }

        private void ConfigureFilesInterceptor(DiscordClient client)
        {
            client.MessageCreated += Client_OnMessageCreated;
        }

        private async Task Client_OnMessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            logger.Debug("Client_OnMessageCreated invoked");
            IReadOnlyCollection<DiscordAttachment> attachments = e.Message.Attachments;

            // Skip messages with no attachments
            if (attachments.Count == 0)
            {
                logger.Debug("Client_OnMessageCreated skipped");
                return;
            }

            logger.Debug($"Attachments count: {attachments.Count}");

            foreach (DiscordAttachment attachment in attachments)
                if (attachment.Width != 0 && attachment.Height != 0)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state) { ExecuteMessageTrack(e.Message, attachment); }));
                    break;
                }
        }

        /// <summary>
        /// Начинает отслеживание реакций сообщения с картинкой.
        /// </summary>
        /// <param name="message">Отслеживаемое изображение</param>
        /// <param name="attachment">Картинка</param>
        private void ExecuteMessageTrack(DiscordMessage message, DiscordAttachment attachment)
        {
            var interactivity = client.GetInteractivity();
            if (_pollEmojiCache == null)
            {
                _pollEmojiCache = new[] {
                        DiscordEmoji.FromName(client, ":grey_question:")
                    };
            }

            TimeSpan duration = TimeSpan.FromSeconds(5);

            // DoPollAsync adds automatically emojis out from an emoji array to a special message and waits for the "duration" of time to calculate results.
            var pollResult = interactivity.DoPollAsync(message, _pollEmojiCache, PollBehaviour.DeleteEmojis, duration);
            var questions = pollResult.Result[0].Total;

            if (questions > 0)
            {
                DownloadAndRecognizeImage(attachment);
            }
        }

        /// <summary>
        /// Начинает распознавание картинки
        /// </summary>
        /// <param name="attachment">Картинка</param>
        private void DownloadAndRecognizeImage(DiscordAttachment attachment)
        {

        }
    }
}