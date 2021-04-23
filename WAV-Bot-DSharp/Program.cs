using System;
using System.Reflection;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates;

using WAV_Bot_DSharp.Configurations;

namespace WAV_Bot_DSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var settingsService = new SettingsLoader();

            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console(new ExpressionTemplate ("{@t:HH:mm:ss} [{@l:u3}] [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}"))
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}")
                .WriteTo.File($"logs/log-{DateTime.Now.Ticks}-", rollingInterval: RollingInterval.Hour)
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Logger.Information($"WAV-Bot-DSharp: {Assembly.GetEntryAssembly().GetName().Version}");

            using (var bot = new Bot(settingsService.LoadFromFile()))
            {
                bot.RunAsync().GetAwaiter().GetResult();
            }
        }
    }
}
