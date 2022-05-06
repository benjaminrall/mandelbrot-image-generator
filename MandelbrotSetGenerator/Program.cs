using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Security.Cryptography.X509Certificates;

namespace MandelbrotSetGenerator
{
    public static class Program
    {

        private const int MaxIteration = 100;
        private const int ResolutionScale = 95;
        
        private static readonly (int width, int height) Resolution = (247 * ResolutionScale, 224 * ResolutionScale);
        private static readonly double ScaleFactor = Math.Min(Resolution.width, Resolution.height);

        private static void Main()
        {
            Bitmap bmp = new(Resolution.width, Resolution.height);

            FractalColourGeneration.Initialise(MaxIteration);
            
            Console.WriteLine("Setup colours");

            int[,] iterationCounts = new int[Resolution.width, Resolution.height];
            double[,] smoothedIterationCounts = new double[Resolution.width, Resolution.height];
            int[] numIterationsPerPixel = new int[MaxIteration];
            double total = 0;
            
            for (int x = 0; x < Resolution.width; x++)
            {
                for (int y = 0; y < Resolution.height; y++)
                {
                    (int i, double si) = CalculateEscapeTime(x, y);
                    iterationCounts[x, y] = (int) Math.Floor(si);
                    smoothedIterationCounts[x, y] = si;
                    numIterationsPerPixel[(int) Math.Floor(si) - 1]++;
                }
                Console.WriteLine($"Calculated column {x + 1}/{Resolution.width}");
            }
            
            Console.WriteLine("Finished iteration calculations.");

            for (int i = 0; i < MaxIteration; i++)
            {
                total += numIterationsPerPixel[i];
            }

            for (int x = 0; x < Resolution.width; x++)
            {
                for (int y = 0; y < Resolution.height; y++)
                {
                    int iteration = iterationCounts[x, y];
                    double smoothedIteration = smoothedIterationCounts[x, y];
                    double hue = 0;
                    for (int i = 0; i < iteration; i++)
                    {
                        hue += numIterationsPerPixel[i];
                    }
                    
                    hue /= total;

                    Color color1 = FractalColourGeneration.GetColour((int) Math.Floor(smoothedIteration) - 1);
                    Color color2 = FractalColourGeneration.GetColour(Math.Min((int) Math.Floor(smoothedIteration), MaxIteration - 1));

                    Color color = lerpColour(color1, color2, smoothedIteration % 1);
                    
                    bmp.SetPixel(x, y, color);
                }
                Console.WriteLine($"Drawn column {x + 1}/{Resolution.width}");
            }
            
            
            Console.WriteLine("Completed set.");

            bmp.Save("output.bmp");
            
            Console.WriteLine("Saved set.");
        }

        private static (double, double) ScalePixel(double px, double py) =>
            (px / Resolution.width * 2.47 - 2, py / Resolution.height * 2.24 - 1.12);

        private static Color lerpColour(Color color1, Color color2, double t)
        {
            double lerpedRed = color1.R + t * (color2.R - color1.R);
            double lerpedGreen = color1.G + t * (color2.G - color1.G);
            double lerpedBlue = color1.B + t * (color2.B - color1.B);

            return Color.FromArgb((int) lerpedRed, (int) lerpedGreen, (int) lerpedBlue);
        }
        
        private static (int, double) CalculateEscapeTime(int px, int py)
        {
            (double x0, double y0) = ScalePixel(px, py);
            
            double x = 0;
            double y = 0;
            double x2 = 0;
            double y2 = 0;
            
            int iteration = 0;

            while (x2 + y2 <= 1 << 16 && iteration < MaxIteration)
            {
                y = (x + x) * y + y0;
                x = x2 - y2 + x0;
                x2 = x * x;
                y2 = y * y;
                iteration++;
            }

            double smoothedIteration = iteration;
            
            if (!(smoothedIteration < MaxIteration)) return (iteration, smoothedIteration);
            
            double logZn = Math.Log(x * x + y * y) / 2;
            double nu = Math.Log(logZn / Math.Log(2)) / Math.Log(2);
            smoothedIteration = smoothedIteration + 1 - nu;
            return (iteration, smoothedIteration);
        }
    }
}