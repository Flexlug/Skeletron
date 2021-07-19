using System;
using System.IO;
using System.Reflection;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

using WAV_Bot_DSharp.Configurations;

namespace WAV_Bot_DSharp
{
    class Program
    {
        public static DateTime StartTime { get; private set; }
        public static DateTime? LastFailure { get; private set; }
        public static int Failures { get; private set; }
        public static string BuildString { get; private set; }

        static void Main(string[] args)
        {
            StartTime = DateTime.Now;
            LastFailure = null;
            Failures = 0;

            var settingsService = new SettingsLoader();

            Log.Logger = new LoggerConfiguration()
                //.WriteTo.Console(new ExpressionTemplate ("{@t:HH:mm:ss} [{@l:u3}] [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}"),
                //                 theme: AnsiConsoleTheme.Literate)
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}",
                                 theme: SystemConsoleTheme.Colored)
                .WriteTo.File($"logs/log-{DateTime.Now.Ticks}-", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();

            BuildString = $"{Assembly.GetEntryAssembly().GetName().Version} {File.GetCreationTime(Assembly.GetCallingAssembly().Location)} .NET {System.Environment.Version}";
            Log.Logger.Information($"WAV-Bot-DSharp: {Assembly.GetEntryAssembly().GetName().Version} (builded {File.GetCreationTime(Assembly.GetCallingAssembly().Location)}");

            while (true)
            {
                try
                {
                    using (var bot = new Bot(settingsService.LoadFromFile()))
                        bot.RunAsync().GetAwaiter().GetResult();
                }
                catch(Exception ex)
                {
                    Log.Logger.Fatal(ex, "Bot failed");
                    LastFailure = DateTime.Now;
                    Failures++;
                }
            }
        }
    }
}
