using System;
using System.DrawingCore;
using System.IO;
using WAV_Osu_Recognizer;

namespace WAV_Osu_Recognizer_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Recognizer rec = new Recognizer();

            string[] dirFiles = Directory.GetFiles(@"./testImages/");

            foreach (string path in dirFiles)
            {
                Image img = rec.LoadFromFile(path);
                Console.WriteLine(rec.RecognizeBeatmap(img, path));
            }
        }
    }
}
