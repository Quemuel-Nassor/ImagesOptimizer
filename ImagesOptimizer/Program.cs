using Emgu.CV.DepthAI;
using ImagesOptmizer;

namespace ImagesOptimizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(args[i]))
                    args[i] = args[i].ToLower();
            }

            bool showHelp = args.Contains("h") || args.Length > 2;

            if (showHelp)
            {
                Console.WriteLine("Generates resized images based on bootstrap screen breakpoints for each image file in the current folder:" +
                    "\n\n   imoptmizer [j] [w] [h]\n\n" +
                    "   j - Save image as .JPEG\n" +
                    "   w - Save image as .WebP\n" +
                    "   h - Show this menu helper\n" +
                    "   Default option save resized images on original format");

                return;
            }

            bool useJpeg = args.Contains("j");
            bool useWebp = args.Contains("w");

            string workDir = Directory.GetCurrentDirectory();
            Task.WaitAll(Task.Run(async () => await OptimizerHelper.OptimizeImages(workDir, useWebp, useJpeg)));
        }
    }
}