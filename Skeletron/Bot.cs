using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
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

using GoogleApi;

namespace Skeletron
{
    public class Bot : IDisposable
    {
        public const ulong GUILD_UID = 708860200341471264;
        public const ulong SKELETRON_UID = 750768015842345050; 

        private CommandsNextExtension CommandsNext { get; set; }
        private SlashCommandsExtension SlashCommands { get; set; }
        private DiscordClient Discord { get; }
        private DiscordGuild Guild { get; }
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

            Discord.ClientErrored += Discord_ClientErrored;

            // For correct datetime recognizing
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            Guild = Discord.GetGuildAsync(GUILD_UID).Result;

            ConfigureBot();
            ConfigureServices();

            RegisterCommands();
            RegisterEvents();
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
                //.AddSingleton(new DocumentStoreProvider(Settings))
                .AddSingleton(Discord)
                .AddSingleton(Guild)
                .AddSingleton<OsuEmoji>()
                .AddSingleton<OsuEmbed>()
                .AddSingleton<OsuEnums>()
                .AddSingleton<OsuRegex>()
                .AddSingleton<VkRegex>()
                //.AddSingleton<NumbersApi>()
                .AddSingleton<EmojiUtlis>()
                .AddSingleton(new BanchoApi(Settings.ClientId, Settings.Secret))
                .AddSingleton(new GatariApi())
                .AddSingleton(new GoogleSearch())
                //.AddSingleton<IWordsProvider, WordsProvider>()
                //.AddSingleton<ISheetGenerator, SheetGenerator>()
                .AddSingleton<IShedulerService, ShedulerService>()
                //.AddSingleton<IRecognizerService, RecognizerService>()
                //.AddSingleton<IMembersProvider, MembersProvider>()
                //.AddSingleton<ICompitProvider, CompitProvider>()
                //.AddSingleton<ICompititionService, CompititionService>()
                //.AddSingleton<IMappoolProvider, MappoolProvider>()
                //.AddSingleton<IMappoolService, MappoolService>()
                //.AddSingleton<IWordsService, WordsService>()
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
                StringPrefixes = Settings.Prefixes,
                Services = Services
            };
            CommandsNext = Discord.UseCommandsNext(commandsNextConfiguration);
            CommandsNext.SetHelpFormatter<CustomHelpFormatter>();

            // Registering command classes
            CommandsNext.RegisterCommands<UserCommands>();
            CommandsNext.RegisterCommands<AdminCommands>();
            //CommandsNext.RegisterCommands<DemonstrationCommands>();
            //CommandsNext.RegisterCommands<RecognizerCommands>();
            CommandsNext.RegisterCommands<FunCommands>();
            CommandsNext.RegisterCommands<OsuCommands>();
            CommandsNext.RegisterCommands<VkCommands>();
            //CommandsNext.RegisterCommands<CompititionCommands>();
            //CommandsNext.RegisterCommands<MappoolCommands>();

            // Registering OnCommandError method for the CommandErrored event
            CommandsNext.CommandErrored += OnCommandError;

            //var slashCommandsConfiguration = new SlashCommandsConfiguration()
            //{
            //    Services = Services
            //};

            //SlashCommands = Discord.UseSlashCommands(slashCommandsConfiguration);

            // Register slash commands modules
            //SlashCommands.RegisterCommands<OsuSlashCommands>(WAV_UID);
            //SlashCommands.RegisterCommands<UserSlashCommands>(WAV_UID);
            //SlashCommands.RegisterCommands<MappoolSlashCommands>(WAV_UID);
            //SlashCommands.RegisterCommands<AdminMappoolSlashCommands>(WAV_UID);

            //SlashCommands.SlashCommandErrored += SlashCommands_SlashCommandErrored;
        }

        private async Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            logger.LogError($"Error on executing slash command {e.Context.CommandName} - {e.Exception}");
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

            await Guild.GetAllMembersAsync();

            Log.Logger.Information("The bot is online");
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

            e.Context.RespondAsync($"{Guild.Owner.Mention}", embed: embed);
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
