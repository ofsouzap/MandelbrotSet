using System;
using System.IO;

namespace MandelbrotSet
{
    public class Program
    {

        private const string dataFileName = "tmp_data.dat";
        public const string saveImageAsProgramFileName = "saveimageas.py";

        enum Mode
        {
            Standard,
            DontVisualise,
            Distributed_Server,
            Distributed_Client,
            Generate_PPM,
            ImageSearching
        }

        public static void Main(string[] args)
        {

            Mode mode;
            double minR, maxR, stepR, minI, maxI, stepI;
            ulong maximumRecursionDepth;
            ulong bufferSize;

            if (args.Length == 3) //Image searching with less parameters
            {

                mode = ParseMode(args[0]);

                if (mode != Mode.ImageSearching)
                {
                    Console.WriteLine("Only use 3 arguments when trying to use image searching");
                    return;
                }

                if (!int.TryParse(args[1], out int seed))
                {
                    Console.WriteLine("Invalid seed provided");
                }

                if (!int.TryParse(args[2], out int maximumResults))
                {
                    Console.WriteLine("Invalid maximum results passed");
                }

                ImageSearching.SearchForImages(seed, maximumResults, dataFileName);

                return;

            }
            else if (args.Length == 5) //Image searching with more parameters
            {

                mode = ParseMode(args[0]);

                if (mode != Mode.ImageSearching)
                {
                    Console.WriteLine("Only use 5 arguments when trying to use image searching");
                    return;
                }

                if (!int.TryParse(args[1], out int seed))
                {
                    Console.WriteLine("Invalid seed provided");
                }

                if (!int.TryParse(args[2], out int maximumResults))
                {
                    Console.WriteLine("Invalid maximum results passed");
                }

                if (!double.TryParse(args[3], out double maxCenterRange))
                {
                    Console.WriteLine("Invalid maximum center range passed");
                }

                if (!double.TryParse(args[4], out double maxRange))
                {
                    Console.WriteLine("Invalid maximum range passed");
                }

                ImageSearching.SearchForImages(seed, maximumResults, dataFileName, maxCenterRange, maxRange);

                return;

            }
            else if (args.Length == 4)
            {

                mode = ParseMode(args[0]);

                if (mode != Mode.Generate_PPM)
                {
                    Console.WriteLine("Only use 4 arguments when trying to generate PPM");
                    return;
                }

                string dataFileName = args[1];

                if (!File.Exists(dataFileName))
                {
                    Console.WriteLine("Passed data file name doesn't exist");
                    return;
                }

                string outputFileName = args[2];
                string colorGradientName = args[3];

                Console.WriteLine("Starting PPM generation");

                GeneratePPM.Generate(dataFileName, outputFileName, colorGradientName);

                Console.WriteLine("PPM generation finished");

                return;

            }
            else if (args.Length == 9) //Specifying by corners and step size
            {

                mode = ParseMode(args[0]);
                minR = double.Parse(args[1]);
                maxR = double.Parse(args[2]);
                stepR = double.Parse(args[3]);
                minI = double.Parse(args[4]);
                maxI = double.Parse(args[5]);
                stepI = double.Parse(args[6]);
                maximumRecursionDepth = ulong.Parse(args[7]);
                bufferSize = GetBufferSizeFromInput(args[8]);

            }
            else if (args.Length == 7) //Specifying by center, range and definition
            {

                mode = ParseMode(args[0]);

                double centerR, centerI, range;
                ushort definition;

                centerR = double.Parse(args[1]);
                centerI = double.Parse(args[2]);
                range = double.Parse(args[3]);
                definition = ushort.Parse(args[4]);

                GetGenerationParametersFromCenterRange(
                    centerR,
                    centerI,
                    range,
                    definition,
                    out minR,
                    out maxR,
                    out stepR,
                    out minI,
                    out maxI,
                    out stepI
                    );

                maximumRecursionDepth = ulong.Parse(args[5]);
                bufferSize = GetBufferSizeFromInput(args[6]);

            }
            else
            {

                Console.Write("Mode> ");
                mode = ParseMode(Console.ReadLine());

                Console.Write("Min R> ");
                minR = double.Parse(Console.ReadLine());

                Console.Write("Max R> ");
                maxR = double.Parse(Console.ReadLine());

                Console.Write("Step R> ");
                stepR = double.Parse(Console.ReadLine());

                Console.Write("Min I> ");
                minI = double.Parse(Console.ReadLine());

                Console.Write("Max I> ");
                maxI = double.Parse(Console.ReadLine());

                Console.Write("Step I> ");
                stepI = double.Parse(Console.ReadLine());

                Console.Write("Maximum Recursion Depth> ");
                maximumRecursionDepth = ulong.Parse(Console.ReadLine());

                Console.Write("Buffer Size (Generic size = 65535)> ");
                string bufferSizeInput = Console.ReadLine();
                bufferSize = GetBufferSizeFromInput(bufferSizeInput);

            }

            Console.WriteLine("Estimated total file size: "
                + GetEstimatedFileSize(minR, maxR, stepR, minI, maxI, stepI).ToString()
                + "KB");

            switch (mode) {

                case Mode.Standard:
                    Run_Standard(minR, maxR, stepR, minI, maxI, stepI, maximumRecursionDepth, bufferSize, true);
                    break;

                case Mode.DontVisualise:
                    Run_Standard(minR, maxR, stepR, minI, maxI, stepI, maximumRecursionDepth, bufferSize, false);
                    break;

                case Mode.Distributed_Server:
                    DistributedComputation.Run_Server(minR, maxR, stepR, minI, maxI, stepI, maximumRecursionDepth, bufferSize);
                    break;

                case Mode.Distributed_Client:
                    DistributedComputation.Run_Client(minR, maxR, stepR, minI, maxI, stepI, maximumRecursionDepth, bufferSize);
                    break;

                default:
                    throw new Exception("Unknown mode - " + mode.ToString());

            }

        }

        #region Input Processing

        private static ulong GetBufferSizeFromInput(string input)
        {

            if (input == "max")
                return ulong.MaxValue;
            else if (input == "generic")
                return 65535;
            else
                return ulong.Parse(input);

        }

        public static void GetGenerationParametersFromCenterRange(double centerR,
            double centerI,
            double range,
            ushort definition, //How many calculated points per row/column
            out double minR,
            out double maxR,
            out double stepR,
            out double minI,
            out double maxI,
            out double stepI)
        {

            stepR = stepI = range / definition;

            minR = centerR - range;
            maxR = centerR + range;
            minI = centerI - range;
            maxI = centerI + range;

        }

        private static Mode ParseMode(string input)
        {
            switch (input)
            {

                case "0":
                case "standard":
                case "s":
                    return Mode.Standard;

                case "0-":
                case "dontvisualise":
                case "s-":
                case "dv":
                case "standard-":
                    return Mode.DontVisualise;

                case "1":
                case "server":
                case "distributedserver":
                case "distributed-server":
                case "distributed_server":
                case "ds":
                    return Mode.Distributed_Server;

                case "2":
                case "client":
                case "distributedclient":
                case "distributed-client":
                case "distributed_client":
                case "dc":
                    return Mode.Distributed_Server;

                case "ppm":
                case "generate-ppm":
                case "genppm":
                    return Mode.Generate_PPM;

                case "is":
                case "search":
                case "image-search":
                    return Mode.ImageSearching;

                default:
                    throw new ArgumentException("Invalid input format");

            }
        }

        #endregion

        /// <summary>
        /// Estimates the size of the file used to store the calculated data in kilobytes
        /// </summary>
        public static ulong GetEstimatedFileSize(double minR,
            double maxR,
            double stepR,
            double minI,
            double maxI,
            double stepI
            )
        {

            const byte valueLength = 2;
            ulong rCount = (ulong)Math.Floor((maxR - minR) / stepR);
            ulong iCount = (ulong)Math.Floor((maxI - minI) / stepI);

            //Extra 8 bytes are for row length meta data
            ulong size = 8 + (rCount * iCount * valueLength);

            return size / 1024; //Convert to kilobytes

        }

        public static void Run_Standard(double minR,
            double maxR,
            double stepR,
            double minI,
            double maxI,
            double stepI,
            ulong maximumRecursionDepth,
            ulong bufferSize,
            bool visualise)
        {

            Console.WriteLine("Starting calculation...");

            Calculation.GenerateValues(
                minR,
                maxR,
                stepR,
                minI,
                maxI,
                stepI,
                maximumRecursionDepth,
                dataFileName,
                bufferSize);

            Console.WriteLine("Calculation complete");

            if (visualise)
            {
                Console.WriteLine("Visualising...");
                Visualising.CallVisualisationProgram(dataFileName);
            }

        }

    }
}
