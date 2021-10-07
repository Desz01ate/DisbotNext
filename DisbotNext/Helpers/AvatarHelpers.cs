using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Net;

namespace DisbotNext.Helpers
{
    public static class AvatarHelpers
    {
        public static string GetLevelUpAvatarPath(string url, int level)
        {
            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            Directory.CreateDirectory(tempDir);
            using var webClient = new WebClient();
            var bytes = webClient.DownloadData(url);
            using var avatar = Image.Load(bytes);

            var tempFile = Path.Combine(tempDir, Guid.NewGuid() + (avatar.Frames.Count == 1 ? ".png" : ".gif"));
            using var banner = ProcessImage(avatar, level);
            banner.SaveAsGif(tempFile);
            return tempFile;
        }

        private static Image ProcessImage(Image image, int level)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            image.Mutate(x => x.Resize(512, 512));

            Action<IImageProcessingContext> bannerProcessor;
            if (image.Frames.Count == 1)
            {
                bannerProcessor = x => x.GaussianBlur(5f)
                                           .Opacity(0.33f)
                                           .Crop(image.Width, (int)(image.Height * 0.333));
            }
            else
            {
                bannerProcessor = x => x.Opacity(0.33f)
                                        .Crop(image.Width, (int)(image.Height * 0.333));
            }

            var banner = image.Clone(bannerProcessor);

            image.Mutate(x => x.Resize(banner.Height, banner.Height));
            banner.Mutate(x => x.DrawImage(image, 1f));

            var fontCollection = new FontCollection();
            if (!fontCollection.TryFind("Tahoma", out FontFamily? fontFamily))
            {
                fontFamily = fontCollection.Install("./Assets/tahoma.ttf");
            }

            var headline = $"Level {level}";
            var description = GetLevelDesc(level);

            var font = fontFamily.CreateFont(30);
            var descFont = fontFamily.CreateFont(25);
            var scalingFactor = Math.Min(banner.Width / banner.Width, banner.Height / banner.Height);

            var scaledFont = new Font(font, scalingFactor * font.Size);
            var scaledDescFont = new Font(descFont, scalingFactor * descFont.Size);
            var measure = TextMeasurer.Measure(headline, new RendererOptions(scaledFont));
            var descMeasure = TextMeasurer.Measure(description, new RendererOptions(scaledDescFont));
            var textGraphicOptions = new TextGraphicsOptions()
            {
                TextOptions =
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                }
            };

            var fontBrush = Brushes.Solid(Color.White);
            var fontBorder = Pens.Solid(Color.Black, 0.5f);

            var position = new PointF((int)((banner.Width + image.Width - measure.Width) / 2), (int)(banner.Height * 0.2));
            var descPosition = new PointF((int)((banner.Width + image.Width - descMeasure.Width) / 2), (int)(banner.Height * 0.6));
            banner.Mutate(x => x.DrawText(headline, font, fontBrush, fontBorder, position)
                                .DrawText(description, descFont, fontBrush, fontBorder, descPosition));
            return banner;
        }

        private static string GetLevelDesc(int level)
        {
            return GetLevelDesc((uint)level);
        }

        private static string GetLevelDesc(uint level)
        {
            return level switch
            {
                var x when x < 10 => "'เด็กฝึกงาน'",
                var x when x < 20 => "'พนักงานประจำ'",
                var x when x < 30 => "'ผู้จัดการ'",
                var x when x < 40 => "'ผู้อำนวยการ'",
                _ => "'เจ้าของบริษัท'"
            };
        }
    }
}
