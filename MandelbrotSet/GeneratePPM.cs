using System;
using System.IO;
using System.Text;

namespace MandelbrotSet
{
    public static class GeneratePPM
    {

        public static void Generate(string dataFileName,
            string outputFileName)
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

                        Color valueColor = NormalisedValueToColor(normalisedValue);
                        byte[] colorBytes = valueColor.Bytes;

                        stream.Write(colorBytes, 0, colorBytes.Length);

                    }

            }

        }

        private struct Color
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

        private static Color NormalisedValueToColor(double value)
        {

            if (value < 0 || value > 1)
                throw new ArgumentException("Provided value out of range [0,1]");

            if (value == 0)
                return Color.Black;

            //TODO - replace below later
            return new Color(NormalisedValueToByte(value),
                NormalisedValueToByte(value),
                NormalisedValueToByte(value));

        }

        private static byte NormalisedValueToByte(double value)
        {

            if (value < 0 || value > 1)
                throw new ArgumentException("Provided value out of range [0,1]");

            return (byte)Math.Floor(value * 255);

        }

    }
}
