using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
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
            var tempFile = Path.Combine(tempDir, Guid.NewGuid() + ".png");
            Console.WriteLine($"Generating file {tempFile}");
            using var webClient = new WebClient();
            var bytes = webClient.DownloadData(url);
            using var avatar = Image.Load(bytes);

            avatar.Mutate(x => x.Resize(512, 512));

            using var banner = avatar.Clone(x => x.GaussianBlur(12f)
                                                  .Opacity(0.33f)
                                                  .Crop(avatar.Width, (int)(avatar.Height * 0.333)));

            avatar.Mutate(x => x.Resize(banner.Height, banner.Height));
            banner.Mutate(x => x.DrawImage(avatar, 1f));

            var fontCollection = new FontCollection();
            if (!fontCollection.TryFind("Tahoma", out FontFamily? fontFamily))
            {
                fontFamily = fontCollection.Install("./Assets/tahoma.ttf");
            }

            var headline = $"Level {level}";
            var description = GetLevelDesc(level);

            var font = fontFamily.CreateFont(24);
            var descFont = fontFamily.CreateFont(20);
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

            var position = new PointF((int)((banner.Width + avatar.Width - measure.Width) / 2), (int)(banner.Height * 0.2));
            var descPosition = new PointF((int)((banner.Width + avatar.Width - descMeasure.Width) / 2), (int)(banner.Height * 0.6));
            banner.Mutate(x => x.DrawText(headline, font, Color.White, position).DrawText(description, descFont, Color.White, descPosition));
            banner.Save(tempFile);
            return tempFile;
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
