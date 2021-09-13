using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.Extensions.Logging;

using NumbersAPI.NET;
using NumbersAPI.NET.Exceptions;

namespace Skeletron.Commands
{
    [RequireGuild]
    public sealed class FunCommands : SkBaseCommandModule
    {
        private DiscordClient client;
        private ILogger<FunCommands> logger;

        private NumbersApi numbersApi;
        private const string NUMBERS_IMAGE_URL = @"https://cdn.discordapp.com/attachments/839633777491574785/862815944114831360/hVrxsnLy39c.png";

        public FunCommands(DiscordClient client, ILogger<FunCommands> logger, NumbersApi api)
        {
            ModuleName = "Развлечения";

            this.logger = logger;
            this.client = client;
            this.client.MessageCreated += Client_DetectSayHi;

            this.numbersApi = api;

            logger.LogInformation("FunCommands loaded");
        }

        private async Task Client_DetectSayHi(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            string msg = e.Message.Content.ToLower();

            if (msg.Contains("привет") && msg.Contains("скелетик"))
            {
                await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/776568856167972904/836541954779119616/4a5b505b4026b6fe30376b0b79d3e108fa755e07r1-540-540_hq.gif");
                return;
            }

            if (msg.Contains("вставай припадочный"))
            {
                await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/776568856167972904/838014941884579880/JeRWf8iDd_4.png");
                return;
            }

            if (msg.Contains("привет") && (msg.Contains("виталий") || msg.Contains("припадочный") || msg.Contains("виталя")))
            {
                await e.Message.RespondAsync(":skull:");
                return;
            }
        }

        /// <summary>
        /// Prints out the latency between the bot and discord api servers.
        /// </summary>
        /// <param name="commandContext">CommandContext from the message that has executed this command.</param>
        /// <returns></returns>
        [Command("hi"), Description("Greet the bot")]
        public async Task PingAsync(CommandContext commandContext)
        {
            await commandContext.Message.RespondAsync("https://cdn.discordapp.com/attachments/776568856167972904/836541954779119616/4a5b505b4026b6fe30376b0b79d3e108fa755e07r1-540-540_hq.gif");
        }

        [Command("ntrivia"), Description("Получить факт о случайном числе")]
        public async Task RandomNumberTriviaFact(CommandContext commandContext)
        {
            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle("Random trivia fact")
                .WithDescription(await numbersApi.RandomTriviaAsync())
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("nyear"), Description("Получить факт о случайном годе")]
        public async Task RandomNumberYearFact(CommandContext commandContext)
        {
            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle("Random year fact")
                .WithDescription(await numbersApi.RandomYearAsync())
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("ndate"), Description("Получить факт о случайной дате")]
        public async Task RandomNumberDateFact(CommandContext commandContext)
        {
            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle("Random date fact")
                .WithDescription(await numbersApi.RandomDateAsync())
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("nmath"), Description("Получить факт о случайном числе в контексте математики")]
        public async Task RandomNumberMathFact(CommandContext commandContext)
        {
            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle("Random math fact")
                .WithDescription(await numbersApi.RandomMathAsync())
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("ntrivia"), Description("Получить факт о заданном числе")]
        public async Task NumberTriviaFact(CommandContext commandContext,
                                           [Description("Число, о котором необходимо получить интересный факт")] int number)
        {
            string fact = string.Empty;
            try
            {
                fact = await numbersApi.TriviaAsync(number);
            }
            catch (NumbersNotFoundException)
            {
                await commandContext.RespondAsync("К сожалению никаких интересных фактов об этом числе нет");
                return;
            }

            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle($"Trivia fact about {number}")
                .WithDescription(fact)
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("nyear"), Description("Получить факт о заданном годе")]
        public async Task NumberYearFact(CommandContext commandContext,
                                         [Description("Год, о котором необходимо получить интересный факт")] int year)
        {
            string fact = string.Empty;
            try
            {
                fact = await numbersApi.YearAsync(year);
            }
            catch (NumbersNotFoundException)
            {
                await commandContext.RespondAsync("К сожалению никаких интересных фактов об этом годе нет");
                return;
            }

            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle($"Year fact about {year}")
                .WithDescription(fact)
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("ndate"), Description("Получить факт о заданной дате")]
        public async Task NumberDateFact(CommandContext commandContext,
                                               DateTime date)
        {
            string fact = string.Empty;
            try
            {
                fact = await numbersApi.DateAsync(date);
            }
            catch (NumbersNotFoundException)
            {
                await commandContext.RespondAsync("К сожалению никаких интересных фактов об этой дате нет");
                return;
            }

            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle($"Date fact about {date.Month}/{date.Day}")
                .WithDescription(fact)
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }

        [Command("nmath"), Description("Получить факт о заданном числе в контексте математики")]
        public async Task NumberMathFact(CommandContext commandContext,
                                         int number)
        {
            string fact = string.Empty;
            try
            {
                fact = await numbersApi.MathAsync(number);
            }
            catch (NumbersNotFoundException)
            {
                await commandContext.RespondAsync("К сожалению никаких интересных фактов об этом числе нет");
                return;
            }

            await commandContext.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithTitle($"Math fact about {number}")
                .WithDescription(fact)
                .WithThumbnail(NUMBERS_IMAGE_URL)
                .Build());
        }
    }
}
