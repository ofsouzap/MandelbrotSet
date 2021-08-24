using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace MandelbrotSet
{
    public static class ImageSearching
    {

        private const ushort imageSearchingGenerationDefinition = 256;

        public static class SuitabilityChecks
        {

            public static readonly Predicate<ushort> medianCondition = m => (512 <= m) && (m <= 65023);
            public static readonly Predicate<ushort> iqrCondition = iqr => (512 <= iqr) && (iqr <= 65023);

        }

        public static List<ushort> GetDataFileValues(string dataFileName)
        {

            List<ushort> values = new List<ushort>();

            using (BinaryReader stream = new BinaryReader(File.OpenRead(dataFileName)))
            {

                //First uint64 of data file is row length which should be discarded for this method
                stream.ReadUInt64();

                while (stream.BaseStream.Position != stream.BaseStream.Length)
                {
                    values.Add(stream.ReadUInt16());
                }

            }

            return values;

        }

        public static bool CheckImageSuitability(string dataFileName)
            => CheckImageSuitability(dataFileName, out _);

        /// <summary>
        /// Determines whether an image is suitable based on hard-coded conditions
        /// </summary>
        public static bool CheckImageSuitability(string dataFileName,
            out string failureMessage)
        {

            ushort median, iqr;

            List<ushort> values = GetDataFileValues(dataFileName);

            //N.B. median and IQR are used instead of mean and standard deviation as...
            //...calculating the mean requires summing a large number of values which could easily cause an overflow

            //Median Check

            median = GetValuesMedian(values.ToArray());

            if (!SuitabilityChecks.medianCondition(median))
            {
                failureMessage = "Bad median";
                return false;
            }

            //IQR Check

            iqr = GetValuesIQR(values.ToArray());

            if (!SuitabilityChecks.iqrCondition(iqr))
            {
                failureMessage = "Bad IQR";
                return false;
            }

            failureMessage = default;
            return true;

        }

        public static ushort GetValuesMedian(ushort[] values)
        {

            List<ushort> sortedVals = new List<ushort>(values);

            sortedVals.Sort();

            return sortedVals[sortedVals.Count / 2];

        }

        public static ushort GetValuesIQR(ushort[] values)
        {

            List<ushort> sortedVals = new List<ushort>(values);

            sortedVals.Sort();

            return (ushort)(values[sortedVals.Count * 3 / 4] - values[sortedVals.Count / 4]);

        }

        private static void RecordSuitableImage(string pythonExecutable,
            string dataFileName,
            double centerR,
            double centerI,
            double range,
            ulong maximumRecursionDepth)
        {

            //TODO - record the position and range (perhaps as text in a file or perhaps as a saved image)

            //TODO - remove below once done testing
            Console.WriteLine($"Suitable image found: {centerR}+{centerI}i {range} {maximumRecursionDepth}");
            GeneratePPM.Generate(dataFileName, $"tmp_image.ppm", "jet-inverse");
            Process.Start(pythonExecutable,
                Program.saveImageAsProgramFileName
                + " tmp_image.ppm "
                + $"\"found {centerR},{centerI} range {range} mrd {maximumRecursionDepth}.png\"");

        }

        public static ulong GenerateMaximumRecursionDepthFromRange(double range)
        {
            if (range > 0.001)
                return 1024;
            else if (range > 0.00001)
                return 2048;
            else if (range > 0.0000001)
                return 4096;
            else
                return 8192;
        }

        private static readonly ulong[] rejectedCenterValues = new ulong[] { 0, ulong.MaxValue };

        private static void GetRandomImageParameters(Random random,
            out double centerR,
            out double centerI,
            out double range,
            out ulong maximumRecursionDepth,
            double randomCenterMaxRange = 0,
            double randomRangeMax = 0)
        {

            const double defaultRandomCenterMaxRange = 0.5;
            const double defaultRandomRangeMax = 0.01;

            double maxCenterRange = randomCenterMaxRange == 0 ? defaultRandomCenterMaxRange : randomCenterMaxRange;
            double maxRange = randomRangeMax == 0 ? defaultRandomRangeMax : randomRangeMax;

            range = random.NextDouble() * maxRange;

            maximumRecursionDepth = GenerateMaximumRecursionDepthFromRange(range);

            //This block keeps looking for center positions until one is found that isn't part of the Mandelbrot Set
            //    This helps to increase program efficiency
            do
            {

                centerR = (random.NextDouble() - 0.5) * 2 * maxCenterRange;
                centerI = (random.NextDouble() - 0.5) * 2 * maxCenterRange;

            }
            while (rejectedCenterValues.Contains(Calculation.GenerateValue(new ComplexNumber(centerR, centerI), maximumRecursionDepth)));

        }

        /// <summary>
        /// Automatically searches for images that should look appealing
        /// </summary>
        /// <param name="maximumResults">The maximum number of results to return. If non-positive, won't stop</param>
        /// <param name="dataFileName">The name of the file that the data for calculated images is stored in</param>
        public static void SearchForImages(string pythonExecutable,
            int seed,
            int maximumResults,
            string dataFileName,
            double randomCenterMaxRange = 0,
            double randomRangeMax = 0)
        {

            Console.WriteLine("Starting image searching");

            Random random = new Random(seed);

            uint resultsFound = 0;

            //Sometime, this task could be split for multi-core image searching but would require other changes to effect

            while (true)
            {

                GetRandomImageParameters(random,
                    out double centerR,
                    out double centerI,
                    out double range,
                    out ulong maximumRecursionDepth);

                Program.GetGenerationParametersFromCenterRange(centerR: centerR,
                    centerI: centerI,
                    range: range,
                    definition: imageSearchingGenerationDefinition,
                    out double minR,
                    out double maxR,
                    out double stepR,
                    out double minI,
                    out double maxI,
                    out double stepI);

                Program.Run_Standard(minR, maxR, stepR, minI, maxI, stepI, maximumRecursionDepth, bufferSize: ulong.MaxValue, false);

                if (CheckImageSuitability(dataFileName))
                {

                    RecordSuitableImage(pythonExecutable, dataFileName, centerR, centerI, range, maximumRecursionDepth);
                    resultsFound++;

                    if (maximumResults > 0 && resultsFound >= maximumResults)
                        break;

                }

            }

            Console.WriteLine("Image searching completed");

        }

    }
}
