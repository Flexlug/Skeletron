using System;
using System.IO;
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
using WAV_Bot_DSharp.Services.Interfaces;
using WAV_Osu_NetApi.Models;

namespace WAV_Bot_DSharp.Commands
{
    [Group("wmw")]
    public class CompititionCommands : SkBaseCommandModule
    {
        private ILogger<CompititionCommands> logger;

        private OsuEmbed osuEmbeds;
        private OsuEnums osuEnums;

        private WebClient webClient;

        private DiscordChannel wavScoresChannel;
        private DiscordGuild guild;

        private readonly ulong WAV_UID = 708860200341471264;

        private IWAVCompitProvider wavCompit;
        private IWAVMembersProvider wavMembers;
        private ICompititionService compititionService;

        public CompititionCommands(ILogger<CompititionCommands> logger,
                                   OsuEmbed osuEmbeds,
                                   OsuEnums osuEnums,
                                   DiscordClient client,
                                   IWAVCompitProvider wavCompit,
                                   ICompititionService compititionService)
        {
            this.osuEmbeds = osuEmbeds;
            this.osuEnums = osuEnums;
            this.logger = logger;
            this.wavCompit = wavCompit;
            this.wavMembers = wavMembers;
            this.compititionService = compititionService;

            this.guild = client.GetGuildAsync(WAV_UID).Result;
            this.wavScoresChannel = client.GetChannelAsync(829466881353711647).Result;

            this.logger.LogInformation("CompititionCommands loaded");
        }

        [Command("register-manual"), Description("Зарегистрировать другого участника в конкурсе"), RequirePermissions(Permissions.Administrator)]
        public async Task RegisterUser(CommandContext commandContext,
            [Description("Регистрируемый участник")] string strMember,
            [Description("Сервер, на котором находится основной osu! профиль")] string strServer)
        {
            DiscordMember dmember = guild.Members.FirstOrDefault(x => x.Value.Username == strMember).Value;
            if (dmember is null)
            {
                await commandContext.RespondAsync("Не удалось найти такого пользователя.");
                return;
            }

            WAVMember member = wavMembers.GetMember(dmember.Id);

            if (member.OsuServers.Count == 0)
            {
                await commandContext.RespondAsync("К Вашему профилю ещё нет привязаных osu! профилей. Привяжите свой профиль через `osuset`.");
                return;
            }

            OsuServer? mbServer = osuEnums.StringToOsuServer(strServer);
            if (mbServer is null)
            {
                await commandContext.RespondAsync($"Не удалось распознать название сервера {strServer}.");
                return;
            }

            OsuServer server = (OsuServer)mbServer;
            WAVMemberOsuProfileInfo profileInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);
            if (profileInfo is null)
            {
                await commandContext.RespondAsync($"К Вашему профилю не привязанного профиля сервера {osuEnums.OsuServerToString(server)}.");
                return;
            }

            try
            {
                await compititionService.RegisterMember(commandContext.Member, profileInfo);
                await commandContext.RespondAsync($"Регистрация прошла успешно.");
            }
            catch (Exception e)
            {
                await commandContext.RespondAsync(e.Message);
            }
        }

        [Command("register-manual"), Description("Зарегистрировать другого участника в конкурсе"), RequirePermissions(Permissions.Administrator)]
        public async Task RegisterUser(CommandContext commandContext,
            [Description("Регистрируемый участник")] DiscordMember dmember,
            [Description("Сервер, на котором находится основной osu! профиль")] string strServer)
        {
            WAVMember member = wavMembers.GetMember(dmember.Id);

            if (member.OsuServers.Count == 0)
            {
                await commandContext.RespondAsync("К Вашему профилю ещё нет привязаных osu! профилей. Привяжите свой профиль через `osuset`.");
                return;
            }

            OsuServer? mbServer = osuEnums.StringToOsuServer(strServer);
            if (mbServer is null)
            {
                await commandContext.RespondAsync($"Не удалось распознать название сервера {strServer}.");
                return;
            }

            OsuServer server = (OsuServer)mbServer;
            WAVMemberOsuProfileInfo profileInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);
            if (profileInfo is null)
            {
                await commandContext.RespondAsync($"К Вашему профилю не привязанного профиля сервера {osuEnums.OsuServerToString(server)}.");
                return;
            }

            try
            {
                await compititionService.RegisterMember(commandContext.Member, profileInfo);
                await commandContext.RespondAsync($"Регистрация прошла успешно.");
            }
            catch (Exception e)
            {
                await commandContext.RespondAsync(e.Message);
            }
        }

        [Command("register"), Description("Зарегистрироваться в конкурсе W.m.W и получить категорию. Зарегистрироваться можно только один раз. Средний PP будет время от времени пересчитываться.")]
        public async Task Register(CommandContext commandContext,
            [Description("Сервер, на котором находится основной osu! профиль")] string strServer)
        {
            WAVMember member = wavMembers.GetMember(commandContext.Member.Id);
            if (member.CompitionInfo is not null)
            {
                await commandContext.RespondAsync("Вы уже зарегистрированы. В случае ошибки обратитесь к администрации сервера.");
                return;
            }

            if (member.OsuServers.Count == 0)
            {
                await commandContext.RespondAsync("К Вашему профилю ещё нет привязаных osu! профилей. Привяжите свой профиль через `osuset`.");
                return;
            }

            OsuServer? mbServer = osuEnums.StringToOsuServer(strServer);
            if (mbServer is null)
            {
                await commandContext.RespondAsync($"Не удалось распознать название сервера {strServer}.");
                return;
            }

            OsuServer server = (OsuServer)mbServer;
            WAVMemberOsuProfileInfo profileInfo = member.OsuServers.FirstOrDefault(x => x.Server == server);
            if (profileInfo is null)
            {
                await commandContext.RespondAsync($"К Вашему профилю не привязанного профиля сервера {osuEnums.OsuServerToString(server)}.");
                return;
            }

            try
            {
                await compititionService.RegisterMember(commandContext.Member, profileInfo);
                await commandContext.RespondAsync($"Регистрация прошла успешно.");
            }
            catch(Exception e)
            {
                await commandContext.RespondAsync(e.Message);
            }
        }

        [Command("submit"), Description("Отправить свой скор (к сообщению необходимо прикрепить свой реплей в формате .osr)."), RequireDirectMessage]
        public async Task SubmitScore(CommandContext commandContext)
        {
            DiscordMessage msg = await commandContext.Channel.GetMessageAsync(commandContext.Message.Id);
            logger.LogInformation($"DM {msg.Author}: {msg.Content} : {msg.Attachments.Count}");

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

            CompitInfo compitInfo = compititionService.GetCompitInfo();

            if (compitInfo.IsRunning)
            {
                await commandContext.RespondAsync("На данный момент конкурс ещё не запущен. Следите за обновлениями.");
                return;
            }

            WAVMemberCompitProfile compitProfile = wavCompit.GetCompitProfile(commandContext.Member.Id);

            if (compitProfile.NonGrata)
            {
                await commandContext.RespondAsync("Извините, но вы не можете принять участие в данном конкурсе, т.к. внесены в черный список.");
                return;
            }

            Replay replay = null;

            string fileName = string.Empty;
            try
            {
                fileName = $"{DateTime.Now.Ticks}-{attachment.FileName}";
                webClient.DownloadFile(attachment.Url, $"downloads/{fileName}");

                replay = ReplayDecoder.Decode($"downloads/{fileName}");
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Exception while parsing score");
            }

            if (replay.Mods != 0)
            {
                const int allowedMods = (int)(OsuParsers.Enums.Mods.NoFail | OsuParsers.Enums.Mods.Perfect | OsuParsers.Enums.Mods.SuddenDeath);
                if (((int)replay.Mods | allowedMods) != allowedMods)
                {
                    await commandContext.RespondAsync("Мы не можем принять данный скор по причине того, что он поставлен с запрещенными на W.M.W модами. \nРазрешенные на W.M.W моды - `NF`, `SD`, `PF`\nСкор система: V1");
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

            bool correctMap = false;

            switch (compitProfile.Category)
            {
                case CompitCategories.Beginner:
                    if (replay.BeatmapMD5Hash == compitInfo.BeginnerMapHash)
                        correctMap = true;
                    break;

                case CompitCategories.Alpha:
                    if (replay.BeatmapMD5Hash == compitInfo.AlphaMapHash)
                        correctMap = true;
                    break;

                case CompitCategories.Beta:
                    if (replay.BeatmapMD5Hash == compitInfo.BetaMapHash)
                        correctMap = true;
                    break;

                case CompitCategories.Gamma:
                    if (replay.BeatmapMD5Hash == compitInfo.GammaMapHash)
                        correctMap = true;
                    break;

                case CompitCategories.Delta:
                    if (replay.BeatmapMD5Hash == compitInfo.DeltaMapHash)
                        correctMap = true;
                    break;

                case CompitCategories.Epsilon:
                    if (replay.BeatmapMD5Hash == compitInfo.EpsilonMapHash)
                        correctMap = true;
                    break;
            }

            if (!correctMap)
            {
                await commandContext.RespondAsync($"Мы не можем принять данный скор по причине того, что он поставлен не на той карте, которая выдана вашей категории {osuEnums.CategoryToString(compitProfile.Category)}");
                return;
            }

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

            DiscordMessage scoreMessage = await wavScoresChannel.SendMessageAsync(new DiscordMessageBuilder()
                                                            .WithFile(new FileStream($"downloads/{fileName}", FileMode.Open))
                                                            .WithEmbed(embed));

            wavCompit.SubmitScore(new CompitScore()
            {
                Player = commandContext.User.Id,
                Score = replay.ReplayScore,
                ScoreUrl = scoreMessage.Attachments.FirstOrDefault()?.Url
            });

            await commandContext.RespondAsync("Ваш скор был отправлен на рассмотрение. Спасибо за участие!");
        }
    }

}
