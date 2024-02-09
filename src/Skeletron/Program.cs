using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

using Skeletron.Configurations;
using Skeletron.Exceptions;

namespace Skeletron
{
    class Program
    {
        public static DateTime StartTime { get; private set; }
        public static DateTime? LastFailure { get; private set; }
        public static int Failures { get; private set; }
        public static string BuildString { get; private set; }

        static async Task Main(string[] args)
        {
            StartTime = DateTime.Now;
            LastFailure = null;
            Failures = 0;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}",
                    theme: SystemConsoleTheme.Colored)
                .WriteTo.File($"logs/log-{DateTime.Now.Ticks}-", rollingInterval: RollingInterval.Day)
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .CreateLogger();

            BuildString =
                $"{Assembly.GetEntryAssembly().GetName().Version} {File.GetCreationTime(Assembly.GetCallingAssembly().Location)} .NET {System.Environment.Version}";
            Log.Logger.Information(
                $"WAV-Bot-DSharp: {Assembly.GetEntryAssembly().GetName().Version} (builded {File.GetCreationTime(Assembly.GetCallingAssembly().Location)}");

            bool isShutdown = false;
            while (!isShutdown)
            {
                try
                {
                    using var bot = new Bot(new Settings());
                    await bot.RunAsync();
                    isShutdown = true;
                }
                catch (NeedRestartException ex)
                {
                    Log.Logger.Fatal(ex, "Bot failed");
                    LastFailure = DateTime.Now;
                    Failures++;
                }
            }
        }
    }
}
