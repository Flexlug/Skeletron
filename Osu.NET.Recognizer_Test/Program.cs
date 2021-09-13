using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using OsuNET_Api;

using OsuNET_Recognizer;

using Newtonsoft.Json;
using OsuNET_Api.Models.Bancho;

namespace WAV_Osu_Recognizer_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Recognizer rec = new Recognizer();

            Settings settings;
            using (StreamReader sr = new StreamReader("credentials.json"))
                settings = JsonConvert.DeserializeObject<Settings>(sr.ReadToEnd());

            BanchoApi api = new BanchoApi(settings.ClientId, settings.Secret);
            Console.WriteLine(api.ReloadToken());

            string[] dirFiles = Directory.GetFiles(@"./testImages/");

            foreach (string path in dirFiles)
            {
                Image img = rec.LoadFromFile(path);

                string[] recedText = rec.RecognizeTopText(img).Split('\n');
                List<Beatmapset> bms = api.Search(recedText.First(), MapType.Any);

                foreach (string s in recedText)
                    Console.WriteLine(s);

                Beatmapset bm = bms?.First();

                if (bm is null)
                    Console.WriteLine("No beatmap found");
                else
                    Console.WriteLine($"{bm.artist} - {bm.title}\n{bm.creator}");
            }
        }
    }
}
