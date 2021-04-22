using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using OsuParsers.Replays;
using OsuParsers.Decoders;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using WAV_Bot_DSharp.Configurations;
using System.IO;

namespace WAV_Bot_DSharp.Commands
{
    public class OsuCommands : SkBaseCommandModule
    {
        private ILogger<OsuCommands> logger;

        private DiscordChannel wavScoresChannel;
        private WebClient webClient;

        public OsuCommands(ILogger<OsuCommands> logger, DiscordClient client)
        {
            ModuleName = "Osu commands";

            this.logger = logger;
            this.wavScoresChannel = client.GetChannelAsync(829466881353711647).Result;
            this.webClient = new WebClient();


            logger.LogInformation("OsuCommands loaded");
        }

        [Command("submit"), RequireDirectMessage]
        public async Task SubmitScore(CommandContext commandContext)
        {
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Id);

            if (Settings.KOSTYL.IgnoreDMList.Contains(msg.Author.Id))
            {
                await commandContext.RespondAsync("Извините, но вы были внесены в черный список бота.");
                return;
            }

            if (msg.Attachments.Count == 0)
            {
                await commandContext.RespondAsync("Вы не прикрепили к сообщению никаких файлов.");
                return;
            }

            if (msg.Attachments.Count > 1)
            {
                await commandContext.RespondAsync("К сообщению можно прикрепить только один файл.");
                return;
            }

            DiscordAttachment attachment = msg.Attachments.First();

            if (!attachment.FileName.EndsWith("osr"))
            {
                await commandContext.RespondAsync("Файл не является реплеем.");
                return;
            }

            Replay replay = null;

            try
            {
                string fileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
                webClient.DownloadFile(attachment.Url, $"downloads/{fileName}");

                replay = ReplayDecoder.Decode($"downloads/{fileName}");
            }

            catch (Exception e)
            {
                logger.LogCritical(e, "Exception while parsing score");
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Discord nickname: `{msg.Author.Username}`");
            sb.AppendLine($"Osu nickname: `{replay.PlayerName}`");
            sb.AppendLine($"Score: `{replay.ReplayScore}`");
            sb.AppendLine($"Mods: `{replay.Mods}`");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithAuthor(msg.Author.Username, iconUrl: msg.Author.AvatarUrl)
                                                                 .WithTitle($"Added replay {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}")
                                                                 .WithUrl(attachment.Url)
                                                                 .WithDescription(sb.ToString())
                                                                 .AddField("OSR Link:", attachment.Url)
                                                                 .AddField("File name:", $"`{attachment.FileName}`")
                                                                 .WithTimestamp(DateTime.Now);

            await wavScoresChannel.SendMessageAsync(embed: embed);
            await commandContext.RespondAsync("Ваш скор был отправлен на рассмотрение. Спасибо за участие!");
        }
    }
}
