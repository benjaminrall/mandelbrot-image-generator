 using System;
 using System.Drawing;

namespace MandelbrotSetGenerator
{
    public static class FractalColourGeneration
    {
        private static readonly double[] ObservedX = {-0.1425, 0, 0.16, 0.42, 0.6425, 0.8575, 1, 1.16};

        private static readonly double[] ObservedRed = {0, 0, 32, 237, 255, 0, 0, 32};
        private static readonly double[] ObservedGreen = {2, 7, 107, 255, 170, 2, 0, 107};
        private static readonly double[] ObservedBlue = {0, 100, 203, 255, 0, 0, 0, 203};

        private static double[] _red;
        private static double[] _green;
        private static double[] _blue;
        
        public static void Initialise(int scale)
        {
            double[] xRange = new double[scale];
            for (double x = 0; x < scale; x++)
            {
                xRange[(int) x] = x / scale;
            }

            _red = Interpolate(ObservedX, ObservedRed, xRange);
            _green = Interpolate(ObservedX, ObservedGreen, xRange);
            _blue = Interpolate(ObservedX, ObservedBlue, xRange);
        }

        private static double[] Interpolate(double[] xs, double[] ys, double[] xRange)
        {
            int length = xs.Length;
            
            // Get consecutive differences and slopes
            double[] dxs = new double[length - 1];
            double[] dys = new double[length - 1];
            double[] gradients = new double[length - 1];
            
            for (int i = 0; i < length - 1; i++)
            {
                double dx = xs[i + 1] - xs[i];
                double dy = ys[i + 1] - ys[i];

                dxs[i] = dx;
                dys[i] = dy;
                gradients[i] = dy / dx;
            }
            
            // Get degree-1 coefficients
            double[] firstCoefficients = new double[length];
            firstCoefficients[0] = gradients[0];
            for (int i = 0; i < dxs.Length - 1; i++)
            {
                double m = gradients[i];
                double mNext = gradients[i + 1];
                if (m * mNext <= 0)
                {
                    firstCoefficients[i + 1] = 0;
                }
                else
                {
                    double dx = dxs[i];
                    double dxNext = dxs[i + 1];
                    double common = dx + dxNext;
                    firstCoefficients[i + 1] = 3 * common / ((common + dxNext) / m + (common + dx) / mNext);
                }
            }
            firstCoefficients[^1] = gradients[^1];
            
            // Get degree-2 and degree-3 coefficients
            double[] secondCoefficients = new double[length - 1];
            double[] thirdCoefficients = new double[length - 1];
            for (int i = 0; i < length - 1; i++)
            {
                double c1 = firstCoefficients[i];
                double m = gradients[i];
                double invDx = 1 / dxs[i];
                double common = c1 + firstCoefficients[i + 1] - m - m;
                secondCoefficients[i] = (m - c1 - common) * invDx;
                thirdCoefficients[i] = common * invDx * invDx;
            }

            double[] yRange = new double[xRange.Length];
            for (int i = 0; i < xRange.Length; i++)
            {
                double x = xRange[i];
                if (Math.Abs(x - xs[^1]) < 0.0000000001)
                {
                    yRange[i] = ys[i];
                    continue;
                }

                int low = 0;
                int high = thirdCoefficients.Length - 1;
                bool found = false;
                while (low <= high)
                {
                    int mid = (int) Math.Floor(0.5 * (low + high));
                    double xHere = xs[mid];
                    if (xHere < x)
                    {
                        low = mid + 1;
                    }
                    else if (xHere > x)
                    {
                        high = mid - 1;
                    }
                    else
                    {
                        found = true;
                        yRange[i] = ys[mid];
                        break;
                    }
                }
                
                if (found) continue;

                int index = Math.Max(0, high);
                double diff = x - xs[index];
                double diffSq = diff * diff;

                yRange[i] = ys[index] + firstCoefficients[index] * diff + secondCoefficients[index] * diffSq + thirdCoefficients[index] * diff * diffSq;
            }

            return yRange;
        }

        public static Color GetColour(int position) =>
            Color.FromArgb((int) _red[position], (int) _green[position], (int) _blue[position]);
    }
}