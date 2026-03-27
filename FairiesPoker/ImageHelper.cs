using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FairiesPoker
{
    /// <summary>
    /// 图片处理工具类
    /// </summary>
    public static class ImageHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        /// <summary>
        /// 验证图片格式
        /// </summary>
        public static bool IsValidImageFormat(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            string ext = System.IO.Path.GetExtension(filePath).ToLower();
            return Array.IndexOf(AllowedExtensions, ext) >= 0;
        }

        /// <summary>
        /// 验证图片格式（从字节流）
        /// </summary>
        public static bool IsValidImageData(byte[] data)
        {
            if (data == null || data.Length < 4)
                return false;

            // 检查常见图片文件头
            // JPEG: FF D8 FF
            // PNG: 89 50 4E 47
            // GIF: 47 49 46 38
            // BMP: 42 4D

            if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                return true; // JPEG
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                return true; // PNG
            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
                return true; // GIF
            if (data[0] == 0x42 && data[1] == 0x4D)
                return true; // BMP

            return false;
        }

        /// <summary>
        /// 裁切并压缩图片
        /// </summary>
        /// <param name="source">源图片</param>
        /// <param name="cropRect">裁切区域</param>
        /// <param name="outputSize">输出尺寸</param>
        /// <param name="quality">JPEG质量 (1-100)</param>
        /// <returns>压缩后的JPEG字节数组</returns>
        public static byte[] CropAndCompress(Image source, Rectangle cropRect, int outputSize, int quality = 85)
        {
            using var cropped = CropImage(source, cropRect, outputSize);
            return CompressToJpeg(cropped, quality);
        }

        /// <summary>
        /// 裁切图片
        /// </summary>
        public static Bitmap CropImage(Image source, Rectangle cropRect, int outputSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new Bitmap(outputSize, outputSize, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                // 填充白色背景
                g.Clear(Color.White);

                // 绘制裁切后的图片
                g.DrawImage(source,
                    new Rectangle(0, 0, outputSize, outputSize),
                    cropRect,
                    GraphicsUnit.Pixel);
            }
            return result;
        }

        /// <summary>
        /// 压缩图片为JPEG格式
        /// </summary>
        public static byte[] CompressToJpeg(Image image, int quality = 85)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            quality = Math.Clamp(quality, 1, 100);

            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);

            using var ms = new MemoryStream();
            image.Save(ms, jpegEncoder, encoderParams);
            return ms.ToArray();
        }

        /// <summary>
        /// 调整图片大小
        /// </summary>
        public static Bitmap ResizeImage(Image source, int maxSize)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int width, height;
            if (source.Width > source.Height)
            {
                width = maxSize;
                height = (int)(source.Height * (float)maxSize / source.Width);
            }
            else
            {
                height = maxSize;
                width = (int)(source.Width * (float)maxSize / source.Height);
            }

            var result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.DrawImage(source, 0, 0, width, height);
            }
            return result;
        }

        /// <summary>
        /// 从文件加载图片
        /// </summary>
        public static Image LoadImageFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在", filePath);

            // 使用这种方式加载，避免文件锁定
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var temp = Image.FromStream(fs);
            return new Bitmap(temp);
        }

        /// <summary>
        /// 获取图片编码器
        /// </summary>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        /// <summary>
        /// 计算适合的JPEG质量（根据目标大小自动调整）
        /// </summary>
        public static int CalculateQuality(int targetSizeKB, int outputSize)
        {
            // 根据输出尺寸估算基础质量
            // 200x200 约需要 quality 70-85 来达到 100KB 以内
            if (targetSizeKB <= 50)
                return 60;
            if (targetSizeKB <= 100)
                return 75;
            if (targetSizeKB <= 200)
                return 85;
            return 90;
        }
    }
}