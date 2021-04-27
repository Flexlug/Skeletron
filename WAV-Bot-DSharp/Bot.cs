using System;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using WAV_Osu_NetApi;

using WAV_Bot_DSharp.Commands;
using WAV_Bot_DSharp.Services;
using WAV_Bot_DSharp.Services.Entities;
using WAV_Bot_DSharp.Configurations;
using WAV_Bot_DSharp.Converters;
using WAV_Bot_DSharp.Databases.Contexts;
using WAV_Bot_DSharp.Databases.Interfaces;
using WAV_Bot_DSharp.Databases.Entities;
using WAV_Bot_DSharp.Services.Interfaces;

using Serilog;
using Microsoft.Extensions.Logging;

namespace WAV_Bot_DSharp
{
    public class Bot : IDisposable
    {
        private CommandsNextExtension CommandsNext { get; set; }
        private DiscordClient Discord { get; }
        private Settings Settings { get; }
        private IServiceProvider Services { get; set; }
        private bool IsDisposed { get; set; }
        private bool IsRunning { get; set; }

        ILoggerFactory logFactory;

        public Bot(Settings settings)
        {
            Settings = settings;
            Settings.KOSTYL = settings;

            logFactory = new LoggerFactory().AddSerilog();

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
                //.AddDbContext<TrackedUserContext>()
                .AddSingleton(Settings)
                .AddSingleton(Discord)
                .AddSingleton<OsuEmoji>()
                .AddSingleton<OsuUtils>()
                .AddSingleton(new BanchoApi(Settings.ClientId, Settings.Secret))
                .AddSingleton(new GatariApi())
                //.AddSingleton<IRecognizerService, RecognizerService>()
                //.AddSingleton<ITrackedUsersDbService, TrackedUsersDbService>()
                //.AddSingleton<ITrackService, TrackService>()
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
            //CommandsNext.RegisterCommands<UserCommands>();
            //CommandsNext.RegisterCommands<AdminCommands>();
            //CommandsNext.RegisterCommands<DemonstrationCommands>();
            //CommandsNext.RegisterCommands<RecognizerCommands>();
            //CommandsNext.RegisterCommands<FunCommands>();
            CommandsNext.RegisterCommands<OsuCommands>();
            //CommandsNext.RegisterCommands<TrackCommands>();

            // Registering OnCommandError method for the CommandErrored event
            CommandsNext.CommandErrored += OnCommandError;
        }

        private void RegisterEvents()
        {
            Log.Logger.Debug("Registering events");
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

        private async Task OnReady(DiscordClient client, ReadyEventArgs e)
        {
            await Discord.UpdateStatusAsync(new DSharpPlus.Entities.DiscordActivity("тебе в душу", DSharpPlus.Entities.ActivityType.Watching), DSharpPlus.Entities.UserStatus.Online);

            Log.Logger.Information("The bot is online");
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
