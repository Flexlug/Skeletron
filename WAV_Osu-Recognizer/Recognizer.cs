using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using RestSharp;

using CSharpOsu;
using CSharpOsu.Module;

using System.DrawingCore;

using Tesseract;

namespace WAV_Osu_Recognizer
{
    /// <summary>
    /// Класс, отвечающий за распознование карт
    /// </summary>
    public class Recognizer
    {
        private const string osu_token = "uOmXVzHEZmKIN7McvyqW5A8JbKS8SYzu134QhIe7";
        private IRestClient RestClient;
        private OsuClient OsuClient;

        private TesseractEngine ocr;

        public Recognizer()
        {
            //RestClient = new RestClient("https://osusearch.com/search/");
            //OsuClient = new OsuClient(osu_token);

            ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            ocr.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-[]!.?\'\"()~:_");
        }

        /// <summary>
        /// Найти карту osu! по заданным известным полям
        /// </summary>
        /// <returns></returns>
        public string RecognizeBeatmap(Image image, string path)
        {
            Bitmap bmp = new Bitmap(image);
            ToGrayScale(bmp);

            bmp.Save($"{path}_BW.jpg");

            byte[] byteImg = null;
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.DrawingCore.Imaging.ImageFormat.Tiff);
                byteImg = ms.ToArray();
            }
            //ImageConverter converter = new ImageConverter();
            //= (byte[])converter.ConvertTo(image, typeof(byte[]));

            Pix img = Pix.LoadTiffFromMemory(byteImg);

            Page pageName = ocr.Process(img, new Rect(1, 1, 1200, 135));
            string mapName = pageName.GetText();
            pageName.Dispose();

            //Page pageMapper = ocr.Process(img, new Rect(175, 45, 700, 35));
            //string mapperName = pageMapper.GetText();
            //pageMapper.Dispose();

            return mapName;
        }

        /// <summary>
        /// Перевечти картинку в ЧБ
        /// </summary>
        /// <param name="Bmp">Картинка</param>
        public void ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            System.DrawingCore.Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = Math.Round(.299 * c.R + .587 * c.G + .114 * c.B) < 50 ? 1 : 255;
                    Bmp.SetPixel(x, y, System.DrawingCore.Color.FromArgb(rgb, rgb, rgb));
                }
        }

        /// <summary>
        /// Открыть картинку через указанный путь на диске
        /// </summary>
        /// <param name="path">Путь к картинке</param>
        /// <returns></returns>
        public Image LoadFromFile(string path)
        {
            return Image.FromFile(path);
        }
    }
}
