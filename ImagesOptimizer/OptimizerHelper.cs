using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImagesOptimizer;

namespace ImagesOptmizer
{
    public static class OptimizerHelper
    {
        private const int DefaultQuality = 75;
        private static readonly IEnumerable<string> Extensions = new List<string>() { ".webp", ".png", ".jpeg", ".jpg" };

        public static async Task OptimizeImages(string workDir, bool useWebp = false, bool useJpeg = false)
        {
            var files = Directory.GetFiles(workDir);

            for (int i = 0; i < files.Length; i++)
            {
                var extension = Path.GetExtension(files[i]).ToLower();
                if (Extensions.Contains(extension))
                {
                    string fileName = Path.GetFileNameWithoutExtension(files[i]).ToLower().Replace(" ", "-");
                    string destinatation = Path.Combine(workDir, fileName);

                    Directory.CreateDirectory(destinatation);
                    await Optmize(files[i], destinatation, useWebp, useJpeg);
                }
            }
        }

        private static async Task Optmize(string src, string dst, bool useWebp = false, bool useJpeg = false, ImageSizes imgWidth = default)
        {
            string filename = string.Empty;
            try
            {
                filename = Path.GetFileName(src);
                string extension = Path.GetExtension(src).ToLower();
                ImwriteFlags flag;
                int compressionQuality = DefaultQuality;
                FileInfo fi = new FileInfo(src);

                using (var img = CvInvoke.Imread(src, ImreadModes.Unchanged).ToImage<Bgra, byte>())
                {
                    int width = img.Width;

                    switch (extension)
                    {
                        case ".webp":
                            flag = ImwriteFlags.WebpQuality;
                            break;
                        case ".png":
                            flag = ImwriteFlags.PngCompression;
                            compressionQuality = 9;
                            break;
                        default:
                            flag = ImwriteFlags.JpegQuality;
                            break;
                    }

                    string folder = dst.Substring(dst.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                    foreach (int size in Enum.GetValues(typeof(ImageSizes)))
                    {
                        width = imgWidth != default ? (int)imgWidth : size;
                        double scale = width / (double)img.Width;
                        //double aspectRatio = img.Height / (double)img.Width;
                        //int height = (int)Math.Ceiling(img.Height * scale);



                        if (width <= img.Width)
                        {
                            await ResizeImage(img, flag, compressionQuality, fi.Length, scale, width, dst, extension, useWebp, useJpeg);
                        }
                        else
                        {
                            Console.WriteLine($"{folder}/{width}w{extension}: Could not resize because the original image is smaller!");
                        }

                        if (imgWidth != default) break;
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"Occour an error on process image {filename}: {error.Message}");
            }
        }

        private static async Task ResizeImage(Image<Bgra, byte> img, ImwriteFlags flag, int compressionQuality, long fileSize, double scale, int width, string dst, string extension, bool useWebp, bool useJpeg)
        {
            try
            {
                await Task.Yield();

                using (Image<Bgra, byte> imgResized = img.Resize(scale, Inter.Area))
                {
                    if (flag != ImwriteFlags.PngCompression && (ImageSizes)width < ImageSizes.Laptop)
                        compressionQuality = 85;

                    bool success = CvInvoke.Imwrite(Path.Combine(dst, $"{width}w{extension}"), imgResized, new KeyValuePair<ImwriteFlags, int>(flag, compressionQuality));
                    LogInfo(success, fileSize, width, dst, extension);

                    if ((ImageSizes)width < ImageSizes.Laptop)
                        compressionQuality = 85;

                    if (useJpeg && !extension.Equals(".jpeg") && !extension.Equals(".jpg"))
                    {
                        extension = ".jpg";
                        success = CvInvoke.Imwrite(Path.Combine(dst, $"{width}w{extension}"), imgResized, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, compressionQuality));
                        LogInfo(success, fileSize, width, dst, extension);
                    }

                    if (useWebp && !extension.Equals(".webp"))
                    {
                        extension = ".webp";
                        success = CvInvoke.Imwrite(Path.Combine(dst, $"{width}w{extension}"), imgResized, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.WebpQuality, compressionQuality));
                        LogInfo(success, fileSize, width, dst, extension);
                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine($"{width}w{extension}: Occour an error on resize image: {error.Message}");
            }
        }

        private static void LogInfo(bool success, long fileSize, int width, string dst, string extension)
        {
            string folder = dst.Substring(dst.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            FileInfo fi = new FileInfo(Path.Combine(dst, $"{width}w{extension}"));
            double reduction = fileSize > 0 ? (fi.Length < fileSize ? -1 : 1) * Math.Abs((fileSize - fi.Length) / (double)fileSize) : 0;
            Console.WriteLine($"{folder}/{width}w{extension}: {(success ? $"successful saved from {(fileSize > 0 ? fileSize / 1024 : 0)}kB to {(fi.Length > 0 ? fi.Length / 1024 : 0)}kB ({reduction.ToString("P")})" : "Failed to save")}");
        }
    }
}
