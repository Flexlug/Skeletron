using System;
using System.IO;
using System.DrawingCore;
using System.Collections.Generic;

using RestSharp;

using Tesseract;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;

namespace WAV_Osu_Recognizer
{
    /// <summary>
    /// Класс, отвечающий за распознование карт
    /// </summary>
    public class Recognizer
    {
        private TesseractEngine ocr;

        public Recognizer()
        {
            ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            //ocr.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-[]!.?\'\"()~:_");
            ocr.SetVariable("classify_enable_learning", false);
        }

        /// <summary>
        /// Распознать на изображении шапку скора
        /// </summary>
        /// <returns></returns>
        public string RecognizeTopText(Image image)
        {
            Image bmp = new Bitmap(image);

            Bitmap bbmp;
            if (bmp.Width < 2400 || bmp.Height < 1500)
                bbmp = ResizeImage(bmp, bmp.Width * 3, bmp.Height * 3);
            else
                bbmp = new Bitmap(bmp);


            ToGrayScale(bbmp);
            bbmp = AddTopWhiteSpace(bbmp);

            string fileName = $"temp/{DateTime.Now.Ticks}_BW.jpg";
            bbmp.Save(fileName);

            Pix img = Pix.LoadFromFile(fileName);

            Page pageName = ocr.Process(img, new Rect(1, 1, img.Width - 10, (int)(img.Height * 0.13)));
            string mapName = pageName.GetText();
            pageName.Dispose();

            bmp.Dispose();
            bbmp.Dispose();

            //if (File.Exists(fileName))
                //File.Delete(fileName);

            return mapName;
        }

        /// <summary>
        /// Перевеcти картинку в ЧБ
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
                    rgb = Math.Round(.299 * c.R + .587 * c.G + .114 * c.B) < 120 ? 255 : 1;
                    Bmp.SetPixel(x, y, System.DrawingCore.Color.FromArgb(rgb, rgb, rgb));
                }
        }

        /// <summary>
        /// Изменить размеры картинки
        /// </summary>
        /// <param name="image">Изменяемая картинка</param>
        /// <param name="width">Новая ширина</param>
        /// <param name="height">Новая длина</param>
        /// <returns></returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public Bitmap AddTopWhiteSpace(Bitmap input)
        {
            Bitmap newBtmp = new Bitmap(input.Width, input.Height + 10);
            Graphics g = Graphics.FromImage(newBtmp);

            g.FillRectangle(Brushes.White, System.DrawingCore.Rectangle.FromLTRB(1, 1, newBtmp.Width - 1, newBtmp.Height - 1));
            g.DrawImageUnscaled(input, 1, 10);

            g.Flush();
            return newBtmp;
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
