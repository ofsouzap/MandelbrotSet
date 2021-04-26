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
         * Afterwards, file is many 4-byte values to represent the data.
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
        public static void WritePartialData(ushort[] data, string outputFileName)
        {

            using (FileStream file = File.Open(outputFileName, FileMode.Append))
            {

                foreach (ushort value in data)
                {

                    byte[] valueBytes = BitConverter.GetBytes(value);
                    file.Write(valueBytes, 0, 2);

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
