using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace MandelbrotSet
{
    public static class GeneratePPM
    {

        public const string colormapScriptName = "colormap.py";

        public static void Generate(string dataFileName,
            string outputFileName,
            string colorGradientName)
        {

            if (!File.Exists(dataFileName))
                throw new ArgumentException("Provided data file name doesn't exist");

            double[][] data = LoadData.Load(dataFileName);

            long rows = data.Length;
            long columns = data[0].Length;

            foreach (double[] column in data)
                if (column.Length != columns)
                    Console.WriteLine("Inconsistent data row count");

            double maxValue = 0;

            foreach (double[] column in data)
                foreach (double value in column)
                    if (value > maxValue)
                        maxValue = value;

            using (FileStream stream = File.Open(outputFileName, FileMode.Create))
            {

                byte[] header = Encoding.ASCII.GetBytes($"P6\n{columns} {rows}\n255\n");

                stream.Write(header, 0, header.Length);

                foreach (double[] column in data)
                    foreach (double value in column)
                    {

                        double normalisedValue = maxValue != 0
                            ? value / maxValue
                            : value == 0 ? 0 : 1;

                        Color valueColor = NormalisedValueToColor(normalisedValue, colorGradientName);
                        byte[] colorBytes = valueColor.Bytes;

                        stream.Write(colorBytes, 0, colorBytes.Length);

                    }

            }

        }

        public struct Color
        {

            public byte r;
            public byte g;
            public byte b;

            public Color(byte r, byte g, byte b)
            {
                this.r = r;
                this.g = g;
                this.b = b;
            }

            public byte[] Bytes => new byte[] { r, g, b };

            public static Color Black = new Color(0, 0, 0);

        }

        private static double Lerp(double a, double b, double f)
            => ((b - a) * f) + a;

        #region Color Gradients

        /// <summary>
        /// A struct for linearly-interpolated color gradients specifying anchor points for r, g and b values.
        /// Instances of this are more-than-likely copied from matplotlib's colormaps (https://matplotlib.org/)
        /// </summary>
        public struct ColorGradient
        {

            public struct ColorPoint
            {

                public double position;
                public double factor;

                public ColorPoint(double position, double factor)
                {
                    this.position = position;
                    this.factor = factor;
                }

            }

            //All ColorPoint[] points should be sorted
            private ColorPoint[] rPoints;
            private ColorPoint[] gPoints;
            private ColorPoint[] bPoints;

            public ColorGradient(ColorPoint[] rPoints,
                ColorPoint[] gPoints,
                ColorPoint[] bPoints)
            {

                this.rPoints = rPoints;
                Array.Sort(this.rPoints, (a, b) => a.position.CompareTo(b.position));

                this.gPoints = gPoints;
                Array.Sort(this.gPoints, (a, b) => a.position.CompareTo(b.position));

                this.bPoints = bPoints;
                Array.Sort(this.bPoints, (a, b) => a.position.CompareTo(b.position));

            }

            private double GetChannelFactor(ColorPoint[] channelPoints, double position)
            {

                if (channelPoints.Length < 1)
                    throw new ArgumentException("channelPoints must have any elements");

                ColorPoint startPoint = channelPoints[0];
                ColorPoint endPoint = channelPoints[channelPoints.Length - 1];

                foreach (ColorPoint point in channelPoints)
                {
                    if (position > point.position)
                    {
                        startPoint = point;
                    }
                    else
                    {
                        endPoint = point;
                        break;
                    }
                }

                double positionFactor = (position - startPoint.position) / (endPoint.position - startPoint.position);

                return Lerp(startPoint.factor, endPoint.factor, positionFactor);

            }

            public Color GetColor(double position)
            {

                return new Color(
                    NormalisedValueToByte(GetChannelFactor(rPoints, position)),
                    NormalisedValueToByte(GetChannelFactor(gPoints, position)),
                    NormalisedValueToByte(GetChannelFactor(bPoints, position))
                    );

            }

            #region Hard-Coded Color Gradients

            public static ColorGradient jet = new ColorGradient(
                new ColorPoint[]
                {
                    new ColorPoint(0.00, 0),
                    new ColorPoint(0.35, 0),
                    new ColorPoint(0.66, 1),
                    new ColorPoint(0.89, 1),
                    new ColorPoint(1, 0.5)
                },
                new ColorPoint[]
                {
                    new ColorPoint(0, 0),
                    new ColorPoint(0.125, 0),
                    new ColorPoint(0.375, 1),
                    new ColorPoint(0.640, 1),
                    new ColorPoint(0.910, 0),
                    new ColorPoint(1, 0)
                },
                new ColorPoint[]
                {
                    new ColorPoint(0, 0.5),
                    new ColorPoint(0.11, 1),
                    new ColorPoint(0.34, 1),
                    new ColorPoint(0.65, 0),
                    new ColorPoint(1, 0)
                }
                );

            #endregion
        }

        private static Color GreyscaleGradient(double value)
        {

            return new Color(NormalisedValueToByte(value),
                NormalisedValueToByte(value),
                NormalisedValueToByte(value));

        }

        private static Color RGBGradient(double value)
        {

            if (value < 0 || value > 1)
                throw new ArgumentException("Provided value out of range [0,1]");

            byte r, g, b;

            if (value < 0.5)
            {
                r = (byte)Math.Floor(Lerp(255, 0, value));
                g = (byte)Math.Floor(Lerp(0, 255, value));
                b = 0;
            }
            else
            {
                r = 0;
                g = (byte)Math.Floor(Lerp(255, 0, value));
                b = (byte)Math.Floor(Lerp(0, 255, value));
            }

            return new Color(r, g, b);

        }

        private static readonly Dictionary<string, Func<double, Color>> colorGradientFunctions = new Dictionary<string, Func<double, Color>>()
        {
            { "rgb", x => RGBGradient(x) },
            { "greyscale", x => GreyscaleGradient(x) },
            { "jet", x => ColorGradient.jet.GetColor(x) },
            { "jet-inverse", x => ColorGradient.jet.GetColor( 1 - x ) }
        };

        #endregion

        private static Color NormalisedValueToColor(double value, string colorGradientName)
        {

            if (value < 0 || value > 1)
                throw new ArgumentException("Provided value out of range [0,1]");

            if (value == 0)
                return Color.Black;
            else
            {
                return colorGradientFunctions[colorGradientName](value);
            }

        }

        private static byte NormalisedValueToByte(double value)
        {

            if (value < 0 || value > 1)
                throw new ArgumentException("Provided value out of range [0,1]");

            return (byte)Math.Floor(value * 255);

        }

    }
}
