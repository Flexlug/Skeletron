using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using OsuParsers.Decoders;
using OsuParsers.Replays;

using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Database.Interfaces;
using WAV_Bot_DSharp.Database.Models;

namespace WAV_Bot_DSharp.Commands
{
    public class CompititionCommands : SkBaseCommandModule
    {
        private ILogger<CompititionCommands> logger;

        private OsuEmbed osuEmbeds;
        private OsuEnums osuEnums;

        private WebClient webClient;

        private DiscordChannel wavScoresChannel;
        private DiscordGuild guild;

        private readonly ulong WAV_UID = 708860200341471264;

        private IWAVMembersProvider wavMembers;
        private IWAVCompitProvider wavCompit;
        private ShedulerService sheduler;

        public CompititionCommands(ILogger<CompititionCommands> logger,
                                   OsuEmbed osuEmbeds,
                                   OsuEnums osuEnums,
                                   DiscordClient client,
                                   ShedulerService sheduler,
                                   IWAVMembersProvider wavMembers,
                                   IWAVCompitProvider wavCompit)
        {
            this.osuEmbeds = osuEmbeds;
            this.osuEnums = osuEnums;
            this.logger = logger;
            this.sheduler = sheduler;
            this.wavMembers = wavMembers;
            this.wavCompit = wavCompit;

            this.guild = client.GetGuildAsync(WAV_UID).Result;
            this.wavScoresChannel = client.GetChannelAsync(829466881353711647).Result;

            this.logger.LogInformation("CompititionCommands loaded");
        }

        [Command("submit"), RequireDirectMessage]
        public async Task SubmitScore(CommandContext commandContext)
        {
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Id);
            logger.LogInformation($"DM {msg.Author}: {msg.Content} : {msg.Attachments.Count}");

            WAVMemberCompitInfo compitInfo = wavCompit.GetParticipationInfo(commandContext.Member.Id);

            if (compitInfo.NonGrata)
            {
                await commandContext.RespondAsync("Извините, но вы не можете принять участие в данном конкурсе, т.к. внесены в черный список.");
                return;
            }

            if (compitInfo.ProvidedScore)
            {
                await commandContext.RespondAsync("Вы уже отправили скор.");
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
                sheduler.AddFileDeleteTask(fileName);
            }

            catch (Exception e)
            {
                logger.LogCritical(e, "Exception while parsing score");
            }

            if ((int)replay.Mods != 0)
            {
                const int b = (int)(OsuParsers.Enums.Mods.NoFail | OsuParsers.Enums.Mods.Perfect | OsuParsers.Enums.Mods.SuddenDeath);
                if (((int)replay.Mods | b) != b)
                {
                    await commandContext.RespondAsync("Мы не можем принять данный скор по причине того, что он поставлен с запрещенными на W.w.W модами. \nРазрешенные на W.w.W моды - `NF`, `SD`, `PF`\nСкор система: V1");
                    return;
                }
            }

            DiscordMember member = await guild.GetMemberAsync(msg.Author.Id);
            string category = member.Roles.Select(x => x.Name)
                                          .FirstOrDefault((x) =>
                                          {
                                              foreach (var xx in (new string[] { "beginner", "alpha", "beta", "gamma", "delta", "epsilon" }))
                                                  if (x.Contains(xx))
                                                      return true;
                                              return false;
                                          });

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Osu nickname: `{replay.PlayerName}`");
            sb.AppendLine($"Discord nickname: `{msg.Author.Username}`");
            sb.AppendLine($"Score: `{replay.ReplayScore:N0}`"); // Format: 123456789 -> 123 456 789
            sb.AppendLine($"Category: `{category ?? "No category"}`");
            sb.AppendLine($"Mods: `{osuEnums.ModsToString((WAV_Osu_NetApi.Models.Bancho.Mods)replay.Mods)}`");

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
