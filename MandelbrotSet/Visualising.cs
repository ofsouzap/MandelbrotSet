using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace MandelbrotSet
{
    public static class Visualising
    {

        /* Data file format:
         * First 8 bytes - number of rows in file
         * Afterwards, file is many 2-byte uint16 to represent the data.
         */

        public const string visualisationProgramFileName = "visualise-mandelbrot.py";

        public static void ResetFile(string outputFileName, ulong rowLength)
        {
            using (FileStream file = File.Create(outputFileName))
            {

                byte[] rowLengthBytes = BitConverter.GetBytes(rowLength);
                file.Write(rowLengthBytes, 0, 8);

            }
        }

        // Data must all be for a single row
        public static void WritePartialData(float[] data, string outputFileName)
        {

            using (FileStream file = File.Open(outputFileName, FileMode.Append))
            {

                foreach (float value in data)
                {

                    if (value < 0 || value > 1)
                        throw new ArgumentException("Provided data contains value outside range [0,1]");

                    ushort valueToWrite = (ushort)Math.Floor(ushort.MaxValue * value);

                    byte[] valueBytes = BitConverter.GetBytes(valueToWrite);
                    file.Write(valueBytes, 0, valueBytes.Length);

                }

            }

        }

        public static void CallVisualisationProgram(string dataFileName)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo(@"C:\Python39\python",
                visualisationProgramFileName + " " + dataFileName)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            };

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {

                    string result = reader.ReadToEnd();
                    Console.Write(result);

                }
            }

        }

    }
}
