using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DisbotNext.Helpers
{
    public static class AvatarHelpers
    {
        public static async Task<string> GetLevelUpAvatarPathAsync(string url, int level)
        {
            var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "Temp");

            Directory.CreateDirectory(tempDir);

            var extension = GetFileExtensionFromUrl(url);

            var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + extension);

            using var httpClient = new HttpClient();

            using var response = await httpClient.GetAsync(url);

            using var fileArray = await response.Content.ReadAsStreamAsync();

            using var ms = new MemoryStream();

            await fileArray.CopyToAsync(ms);

            var imageArray = ms.ToArray();

            using var avatar = Image.Load(imageArray);

            using var banner = ProcessImage(avatar, level);

            switch (extension)
            {
                case ".gif":
                    await banner.SaveAsGifAsync(tempFile);
                    break;
                case ".png":
                    await banner.SaveAsPngAsync(tempFile);
                    break;
                case ".jpg":
                case ".jpeg":
                default:
                    await banner.SaveAsJpegAsync(tempFile);
                    break;
            }

            return tempFile;
        }

        private static Image ProcessImage(Image image, int level)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            image.Mutate(x => x.Resize(512, 512));

            var banner = image.Clone(x => x.Crop(image.Width, (int)(image.Height * 0.333)));

            banner.Mutate(x => x.BoxBlur());
            banner.Mutate(x => x.Fill(new GraphicsOptions
            {
                BlendPercentage = 0.5f,
            }, Color.Black));
            image.Mutate(x => x.Resize(banner.Height, banner.Height));
            banner.Mutate(x => x.DrawImage(image, 1f));

            var fontCollection = new FontCollection();
            if (!fontCollection.TryFind("THSarabunNew", out FontFamily? fontFamily))
            {
                fontFamily = fontCollection.Install("./Assets/THSarabunNew.ttf");
            }

            var headline = $"Level {level}";
            var description = GetLevelDesc(level);

            var font = fontFamily.CreateFont(55);
            var descFont = fontFamily.CreateFont(45);
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
            var fontBorder = Pens.Solid(Color.White, 0.1f);

            var position = new PointF((int)((banner.Width + image.Width - measure.Width) / 2), (int)(banner.Height * 0.1));
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

        public static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }
    }
}
