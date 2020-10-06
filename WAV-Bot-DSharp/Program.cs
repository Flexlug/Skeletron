
using WAV_Bot_DSharp.Configurations;

namespace WAV_Bot_DSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var settingsService = new SettingsService();
            using (var bot = new Bot(settingsService.LoadFromFile()))
            {
                bot.RunAsync().GetAwaiter().GetResult();
            }
        }
    }
}
