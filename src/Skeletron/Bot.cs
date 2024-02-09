using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using OsuNET_Api;

using Skeletron.Commands;
using Skeletron.Services.Entities;
using Skeletron.Configurations;
using Skeletron.Converters;
using Skeletron.Services.Interfaces;

using Skeletron.Services;

using Serilog;
using System.Net.WebSockets;
using System.Net.NetworkInformation;

namespace Skeletron
{
    public class Bot : IDisposable
    {
        private volatile bool isRestart = false;

        private CommandsNextExtension CommandsNext { get; set; }
        private DiscordClient Discord { get; }
        private Settings Settings { get; }
        private IServiceProvider Services { get; set; }
        private bool IsDisposed { get; set; }
        private bool IsRunning { get; set; }

        private ILogger<Bot> logger { get; set; }

        ILoggerFactory logFactory;

        public Bot(Settings settings)
        {
            Settings = settings;

            logFactory = new LoggerFactory().AddSerilog();
            logger = logFactory.CreateLogger<Bot>();

            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Settings.Token,
                TokenType = TokenType.Bot,
                LoggerFactory = logFactory,
                Intents = DiscordIntents.All
            });

            // Activating Interactivity module for the DiscordClient
            Discord.UseInteractivity(new InteractivityConfiguration());

            // For correct datetime recognizing
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");
            
            Discord.ClientErrored += Discord_ClientErrored;
            Discord.Ready += OnReady;
            Discord.GuildDownloadCompleted += async (sender, args) =>
            {
                ConfigureBot();
                ConfigureServices();
                RegisterCommands();

                Log.Logger.Information("Ready");
            };
            Discord.SocketErrored += Discord_SocketErrored;
        }

        private Task Discord_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            // this usually happens when the Internet is disconnected or a connection error occurs
            if (e.Exception is WebSocketException)
                isRestart = !Check();

            return Task.CompletedTask;

            bool Check()
            {
                try
                {
                    logger.LogInformation("Checking the connection");
                    Ping ping = new Ping();
                    PingReply pingReply = ping.Send(Settings.PingTheHost);
                    return pingReply.Status == IPStatus.Success;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    return false;
                }
            }
        }

        ~Bot()
        {
            Dispose(false);
        }

        private void ConfigureBot()
        {
            if (!Directory.Exists("downloads"))
                Directory.CreateDirectory("downloads");

            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");
        }

        public void ConfigureServices()
        {
            Log.Logger.Debug("Configuring services");
            Services = new ServiceCollection()
                .AddLogging(conf => conf.AddSerilog(dispose: true))
                .AddSingleton(Settings)
                .AddSingleton(Discord)
                .AddSingleton<IJokeService, JokeService>()
                .AddSingleton<OsuEmoji>()
                .AddSingleton<OsuEmbed>()
                .AddSingleton<OsuEnums>()
                .AddSingleton<OsuRegex>()
                .AddSingleton<VkRegex>()
                .AddSingleton<EmojiUtlis>()
                .AddSingleton(new BanchoApi(Settings.BanchoClientId, Settings.BanchoSecret))
                .AddSingleton(new GatariApi())
                .AddSingleton<IShedulerService, ShedulerService>()
                .AddSingleton<IOsuService, OsuService>()
                .AddSingleton<IVkPostToMessageService, VkPostToMessageService>()
                .AddSingleton<IMessageResendService, MessageResendService>()
                .AddSingleton<IMessageDeleteService, MessageDeleteService>()
                .BuildServiceProvider();
        }

        private void RegisterCommands()
        {
            Log.Logger.Debug("Registering commands");
            var commandsNextConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = new[] { "sk!" },
                Services = Services
            };
            CommandsNext = Discord.UseCommandsNext(commandsNextConfiguration);
            CommandsNext.SetHelpFormatter<CustomHelpFormatter>();

            // Registering command classes
            CommandsNext.RegisterCommands<UserCommands>();
            CommandsNext.RegisterCommands<AdminCommands>();
            CommandsNext.RegisterCommands<FunCommands>();
            CommandsNext.RegisterCommands<OsuCommands>();
            CommandsNext.RegisterCommands<VkCommands>();
            CommandsNext.RegisterCommands<JokeCommands>();
            
            CommandsNext.CommandErrored += OnCommandError;
        }
        
        public async Task RunAsync()
        {
            if (IsRunning)
            {
                throw new MethodAccessException("The bot is already running");
            }

            await Discord.ConnectAsync();
            IsRunning = true;
            while (IsRunning)
            {
                await Task.Delay(200);
            }
        }

        private async Task OnReady(DiscordClient client, ReadyEventArgs e)
        {
            await Discord.UpdateStatusAsync(new DSharpPlus.Entities.DiscordActivity("тебе в душу", DSharpPlus.Entities.ActivityType.Watching), DSharpPlus.Entities.UserStatus.Online);

            Log.Logger.Information($"The bot is online. Bot profile name: {client.CurrentUser.Username}, profile id: {client.CurrentUser.Id}");
        }

        private Task OnCommandError(object sender, CommandErrorEventArgs e)
        {
            if (e.Exception is ArgumentException)
            {
                e.Context.RespondAsync($"Не удалось вызвать команду `sk!{e.Command.QualifiedName}` с заданными аргументами. Используйте `sk!help`, чтобы проверить правильность вызова команды.");
                return Task.CompletedTask;
            }

            if (e.Exception is DSharpPlus.CommandsNext.Exceptions.CommandNotFoundException)
            {
                e.Context.RespondAsync($"Не удалось найти данную команду.");
                return Task.CompletedTask;
            }

            DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithTitle("Error")
                .WithDescription($"StackTrace: {e.Exception.StackTrace}")
                .AddField("Command", e.Command?.Name ?? "-")
                .AddField("Overload", e.Context.Overload.Arguments.Count == 0 ?
                                      "-" :
                                      string.Join(' ', e.Context.Overload.Arguments.Select(x => x.Name)?.ToArray()))
                .AddField("Exception", e.Exception.GetType().ToString())
                .AddField("Exception msg", e.Exception.Message)
                .AddField("Inner exception", e.Exception.InnerException?.Message ?? "-")
                .AddField("Channel", e.Context.Channel.Name)
                .AddField("Author", e.Context.Member.Username)
                .Build();

            e.Context.RespondAsync($"Error: ", embed: embed);
            return Task.CompletedTask;
        }

        private Task Discord_ClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            logger.LogError(e.Exception, "Discord_ClientErrored");
            return Task.CompletedTask; ;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                Discord.Dispose();
            }
            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
