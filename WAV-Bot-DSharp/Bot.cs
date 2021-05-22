using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using WAV_Osu_NetApi;

using WAV_Bot_DSharp.Commands;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Services.Interfaces;

using Serilog;
using Microsoft.Extensions.Logging;
using WAV_Bot_DSharp.SlashCommands;
using WAV_Bot_DSharp.Services;

namespace WAV_Bot_DSharp
{
    public class Bot : IDisposable
    {
        private readonly ulong WAV_UID = 708860200341471264;

        private CommandsNextExtension CommandsNext { get; set; }
        private SlashCommandsExtension SlashCommands { get; set; }
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
                LoggerFactory = logFactory
            });

            // Activating Interactivity module for the DiscordClient
            Discord.UseInteractivity(new InteractivityConfiguration());
            // Activating VoiceNext module
            Discord.UseVoiceNext();

            Discord.ClientErrored += Discord_ClientErrored;

            // For correct datetime recognizing
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            ConfigureServices();

            RegisterCommands();
            RegisterEvents();
        }

        ~Bot()
        {
            Dispose(false);
        }

        public void ConfigureServices()
        {
            Log.Logger.Debug("Configuring services");
            Services = new ServiceCollection()
                .AddLogging(conf => conf.AddSerilog(dispose: true))
                .AddSingleton(Settings)
                .AddSingleton(Discord)
                .AddSingleton<OsuEmoji>()
                .AddSingleton<OsuUtils>()
                .AddSingleton(new BanchoApi(Settings.ClientId, Settings.Secret))
                .AddSingleton(new GatariApi())
                .AddSingleton<ShedulerService>()
                .AddSingleton<IRecognizerService, RecognizerService>()
                .AddSingleton<DocumentStoreProvider>()
                .AddSingleton<WAVMembersProvider>()
                .BuildServiceProvider();
        }

        private void RegisterCommands()
        {
            Log.Logger.Debug("Registering commands");
            var commandsNextConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = Settings.Prefixes,
                Services = Services
            };
            CommandsNext = Discord.UseCommandsNext(commandsNextConfiguration);
            CommandsNext.SetHelpFormatter<CustomHelpFormatter>();

            // Registering command classes
            CommandsNext.RegisterCommands<UserCommands>();
            CommandsNext.RegisterCommands<AdminCommands>();
            CommandsNext.RegisterCommands<DemonstrationCommands>();
            CommandsNext.RegisterCommands<RecognizerCommands>();
            CommandsNext.RegisterCommands<FunCommands>();
            CommandsNext.RegisterCommands<OsuCommands>();

            var slashCommandsConfiguration = new SlashCommandsConfiguration()
            {
                Services = Services
            };

            SlashCommands = Discord.UseSlashCommands(slashCommandsConfiguration);

            // Register slash commands modules
            SlashCommands.RegisterCommands<OsuSlashCommands>(WAV_UID);

            // Registering OnCommandError method for the CommandErrored event
            CommandsNext.CommandErrored += OnCommandError;
        }

        private void RegisterEvents()
        {
            Log.Logger.Debug("Registering events");
            Discord.Ready += OnReady;
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

            Log.Logger.Information("The bot is online");
        }

        private Task OnCommandError(object sender, CommandErrorEventArgs e)
        {
            // Send command error message as response.
            e.Context.RespondAsync(e.Exception.Message);
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
