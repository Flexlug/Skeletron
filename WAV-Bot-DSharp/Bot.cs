using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;

using WAV_Bot_DSharp.Commands;
using WAV_Bot_DSharp.Services;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Configurations;

using NLog;
using System.Globalization;
using WAV_Bot_DSharp.Services.Interfaces;
using DSharpPlus.Interactivity.Extensions;

namespace WAV_Bot_DSharp
{
    public class Bot : IDisposable
    {
        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private CommandsNextExtension CommandsNext { get; set; }
        private DiscordClient Discord { get; }
        private Settings Settings { get; }
        private IServiceProvider Services { get; set; }
        private bool IsDisposed { get; set; }
        private bool IsRunning { get; set; }

        public Bot(Settings settings)
        {
            Settings = settings;

            Discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Settings.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information
            });

            // Activating Interactivity module for the DiscordClient
            Discord.UseInteractivity(new InteractivityConfiguration());
            // Activating VoiceNext module
            Discord.UseVoiceNext();

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
            Logger.Debug("Configuring services");
            Services = new ServiceCollection()
                .AddDbContext<UsersContext>()
                .AddDbContext<TrackedUserContext>()
                .AddSingleton(Settings)
                .AddSingleton<ILogger>(Logger)
                .AddSingleton(Discord)
                .AddSingleton<IRecognizerService, RecognizerService>()
                //.AddSingleton<IActivityService, ActivityService>()
                .AddSingleton<ITrackService, TrackService>()
                .BuildServiceProvider();
        }

        private void RegisterCommands()
        {
            Logger.Debug("Registering commands");
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
            CommandsNext.RegisterCommands<OsuCommands>();
            //CommandsNext.RegisterCommands<VoiceCommands>();
            //CommandsNext.RegisterCommands<ActivityCommands>();
            CommandsNext.RegisterCommands<TrackCommands>();

            // Registering OnCommandError method for the CommandErrored event
            CommandsNext.CommandErrored += OnCommandError;
        }

        private void RegisterEvents()
        {
            Logger.Debug("Registering events");
            Discord.Ready += OnReady;
            //Discord.DebugLogger
            //Discord.Logger.Log += OnLogMessageReceived;

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

        private Task OnReady(DiscordClient client, ReadyEventArgs e)
        {
            Logger.Info("The bot is online");
            return Task.CompletedTask;
        }

        ///// <summary>
        ///// Logs DSharpPlus internal errors with NLog
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e">DebugLogMessageEventArgs object</param>
        //private void OnLogMessageReceived(object sender, DebugLogMessageEventArgs e)
        //{
        //    var message = $"{e.Application}: {e.Message}";
        //    switch (e.Level)
        //    {
        //        case LogLevel.Debug:
        //            Logger.Debug(e.Exception, message);
        //            break;
        //        case LogLevel.Info:
        //            Logger.Info(e.Exception, message);
        //            break;
        //        case LogLevel.Warning:
        //            Logger.Warn(e.Exception, message);
        //            break;
        //        case LogLevel.Error:
        //            Logger.Error(e.Exception, message);
        //            break;
        //        case LogLevel.Critical:
        //            Logger.Fatal(e.Exception, message);
        //            break;
        //    }
        //}

        private Task OnCommandError(object sender, CommandErrorEventArgs e)
        {
            // Send command error message as response.
            e.Context.RespondAsync(e.Exception.Message);
            return Task.CompletedTask;
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
