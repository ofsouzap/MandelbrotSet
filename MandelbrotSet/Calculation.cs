using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MandelbrotSet
{
    public static class Calculation
    {

        /// <summary>
        /// The radius at which a point can be assumed to be certain to tend towards infinity
        /// </summary>
        public const ushort giveUpOriginRadius = 2;

        public static ushort GenerateValue(ComplexNumber C,
            ulong maximumRecursionDepth)
        {
            
            //Start at C instead of 0 since second value of Z is always C
            ComplexNumber Z = new ComplexNumber(C.r, C.i);

            ushort steps = 0;
            bool completed = false;

            for (ushort iteration = 0; iteration < maximumRecursionDepth; iteration++)
            {

                steps++;
                Z = Z.Squared + C;

                if (Z.OriginRadius > giveUpOriginRadius)
                {
                    completed = true;
                    break;
                }

            }

            return completed ? steps : (ushort)0;

        }

        /// <param name="bufferSize">Maximum number of values to store before saving to file</param>
        public static void GenerateValues(
            double minR,
            double maxR,
            double stepR,
            double minI,
            double maxI,
            double stepI,
            ushort maximumRecursionDepth,
            string outputFileName,
            ulong bufferSize = ulong.MaxValue)
        {

            ulong rowLength = 0;
            for (double r = minR; r <= maxR; r += stepR)
                rowLength++;

            Visualising.ResetFile(outputFileName, rowLength);

            for (double i = minI; i <= maxI; i += stepI)
            {

                List<ushort> current = new List<ushort>();

                for (double r = minR; r <= maxR; r += stepR)
                {

                    current.Add(GenerateValue(
                        new ComplexNumber(r, i),
                        maximumRecursionDepth
                        ));

                    if ((ulong)current.Count >= bufferSize)
                    {
                        SaveValuesToFile(current.ToArray(), outputFileName);
                        current.Clear();
                    }

                }

                if (current.Count > 0)
                {
                    SaveValuesToFile(current.ToArray(), outputFileName);
                    current.Clear();
                }

            }

        }

        private static void SaveValuesToFile(ushort[] values,
            string outputFileName)
        {
            Visualising.WritePartialData(values, outputFileName);
        }

    }
}
