using System;
using System.IO;
using System.Collections.Generic;

using RestSharp;

using Tesseract;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace OsuNET_Recognizer
{
    /// <summary>
    /// Класс, отвечающий за распознование карт
    /// </summary>
    public class Recognizer
    {
        private TesseractEngine ocr;
        private TesseractEngine ocr_multi_lang;

        public Recognizer()
        {
            ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            //ocr.SetVariable("tessedit_char_whitelist", "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-[]!.?\'\"()~:_");
            ocr.SetVariable("classify_enable_learning", false);

            ocr_multi_lang = new TesseractEngine("./tessdata", "eng+rus", EngineMode.Default);
            ocr_multi_lang.SetVariable("classify_enable_learning", "false");
        }

        /// <summary>
        /// Распознать на изображении шапку скора
        /// </summary>
        /// <returns></returns>
        public string RecognizeTopText(Image image)
        {
            Bitmap bbmp;

            Rectangle s = new Rectangle(0, 0, (int)(image.Width * 0.98), (int)(image.Height * 0.13));

            bbmp = CropImage(image, s);
            bbmp = ResizeImage(bbmp, bbmp.Width * 3, bbmp.Height * 3);
            ToGrayScale(bbmp);
            bbmp = AddTopWhiteSpace(bbmp);

            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp/{DateTime.Now.Ticks}_BW.jpg");

            Pix img = PixConverter.ToPix(bbmp);
            bbmp.Save(fileName);

            Page pageName = ocr.Process(img);
            string mapName = pageName.GetText();
            pageName.Dispose();

            bbmp.Dispose();

            return mapName;
        }

        public string RecognizeWholeImage(Image image)
        {
            Bitmap bbmp = new Bitmap(image);

            bbmp = ResizeImage(bbmp, bbmp.Width * 3, bbmp.Height * 3);
            ToGrayScale(bbmp);
            bbmp = AddTopWhiteSpace(bbmp);

            string fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"temp/{DateTime.Now.Ticks}_BW.jpg");

            Pix img = PixConverter.ToPix(bbmp);
            bbmp.Save(fileName);

            Page pageName = ocr_multi_lang.Process(img);
            string mapName = pageName.GetText();
            pageName.Dispose();

            bbmp.Dispose();

            return mapName;
        }

        /// <summary>
        /// Перевеcти картинку в ЧБ
        /// </summary>
        /// <param name="Bmp">Картинка</param>
        public void ToGrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = Math.Round(.299 * c.R + .587 * c.G + .114 * c.B) < 120 ? 255 : 1;
                    Bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(rgb, rgb, rgb));
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

        public static Bitmap CropImage(Image src, Rectangle rect)
        {
            Bitmap target = new Bitmap(rect.Width, rect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, rect.Width, rect.Height),
                                 rect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }

        public Bitmap AddTopWhiteSpace(Bitmap input)
        {
            Bitmap newBtmp = new Bitmap(input.Width, input.Height + 10);
            Graphics g = Graphics.FromImage(newBtmp);

            g.FillRectangle(Brushes.White, Rectangle.FromLTRB(1, 1, newBtmp.Width - 1, newBtmp.Height - 1));
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
