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
        public static string GetLevelupAvatar(string url, uint level)
        {
            return GetLevelupAvatar(url, (int)level);
        }
        public static string GetLevelupAvatar(string url, int level)
        {
            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            Directory.CreateDirectory(tempDir);
            var tempFile = Path.Combine(tempDir, Guid.NewGuid() + ".jpg");
            Console.WriteLine($"Generating file {tempFile}");
            using var webClient = new WebClient();
            var bytes = webClient.DownloadData(url);
            using var stream = new MemoryStream(bytes);
            using var avatar = Image.Load(stream);
            var fontCollection = new FontCollection();
            if (!fontCollection.TryFind("Tahoma", out FontFamily? fontFamily))
            {
                fontFamily = fontCollection.Install("./Assets/tahoma.ttf");
            }

            var headline = $"Level {level}";
            var description = GetLevelDesc(level);

            var font = fontFamily.CreateFont(25);
            var descFont = fontFamily.CreateFont(15);
            var scalingFactor = Math.Min(avatar.Width / avatar.Width, avatar.Height / avatar.Height);

            var scaledFont = new Font(font, scalingFactor * font.Size);
            var scaledDescFont = new Font(descFont, scalingFactor * descFont.Size);
            var measure = TextMeasurer.Measure(headline, new RendererOptions(scaledFont));
            var descMeasure = TextMeasurer.Measure(description, new RendererOptions(scaledDescFont));
            var textGraphicOptions = new TextGraphicsOptions()
            {
                TextOptions = {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    },
            };

            var position = new PointF((int)((avatar.Width - measure.Width) / 2), (int)(avatar.Height * 0.75));
            var descPosition = new PointF((int)((avatar.Width - descMeasure.Width) / 2), (int)(avatar.Height * 0.87));
            var backgroundRect = new RectangleF(0, avatar.Height * 0.7f, avatar.Width, avatar.Height * 0.3f);
            avatar.Mutate(x => x.Fill(Color.Black, backgroundRect));
            avatar.Mutate(x => x.DrawText(headline, font, Color.Gold, position));
            avatar.Mutate(x => x.DrawText(description, descFont, Color.White, descPosition));
            avatar.Save(tempFile);
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
