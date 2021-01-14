using System;
using WAV_Osu_NetApi;

namespace WAV_Osu_NetApi_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            BanchoApi api = new BanchoApi(2962, "qsEuIBjTQBNc5wXbYETsGNYWZWe7acH6fC6pwRFj");
            Console.WriteLine(api.Authorize());
            api.GetUserRecentScores("6885792");

            Console.ReadKey();
        }
    }
}
