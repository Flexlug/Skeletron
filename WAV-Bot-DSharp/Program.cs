using System;
using System.IO;
using System.Reflection;

using Serilog;
using Serilog.Templates;
using Serilog.Sinks.SystemConsole.Themes;

using WAV_Bot_DSharp.Configurations;

namespace WAV_Bot_DSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var settingsService = new SettingsLoader();


            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console(new ExpressionTemplate ("{@t:HH:mm:ss} [{@l:u3}] [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}"),
                //                 theme: AnsiConsoleTheme.Literate)
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}",
                                 theme: SystemConsoleTheme.Colored)
                .WriteTo.File($"logs/log-{DateTime.Now.Ticks}-", rollingInterval: RollingInterval.Hour)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();

            Log.Logger.Information($".NET Version: {System.Environment.Version}");
            Log.Logger.Information($"WAV-Bot-DSharp: {Assembly.GetEntryAssembly().GetName().Version} (builded {File.GetCreationTime(Assembly.GetCallingAssembly().Location)})");

            using (var bot = new Bot(settingsService.LoadFromFile()))
            {
                bot.RunAsync().GetAwaiter().GetResult();
            }
        }
    }
}
