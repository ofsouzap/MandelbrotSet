using System;
using System.Collections.Generic;

namespace MandelbrotSet
{
    public struct Workload
    {

        public double minR;
        public double maxR;
        public double stepR;
        public double[] iValues;
        public ushort maximumRecursionDepth;

        public byte[] Bytes
        {
            get
            {
                return new byte[0];
                //TODO
            }
        }

        public Workload(double minR,
            double maxR,
            double stepR,
            double[] iValues,
            ushort maximumRecursionDepth)
        {
            this.minR = minR;
            this.maxR = maxR;
            this.stepR = stepR;
            this.iValues = iValues;
            this.maximumRecursionDepth = maximumRecursionDepth;
        }

        public static Workload[] SplitIntoWorkloads(double minR,
            double maxR,
            double stepR,
            double minI,
            double maxI,
            double stepI,
            ushort maximumRecursionDepth,
            uint count)
        {

            List<double>[] iValueBins = new List<double>[count];
            for (long index = 0; index < iValueBins.Length; index++)
                iValueBins[index] = new List<double>();

            long binIndex = 0;

            for (double i = minI; i <= maxI; i += stepI)
            {

                iValueBins[binIndex].Add(i);

                binIndex = (binIndex + 1) % iValueBins.Length;

            }

            Workload[] workloads = new Workload[count];

            for (long index = 0; index < iValueBins.Length; index++)
                workloads[index] = new Workload(
                    minR, maxR, stepR,
                    iValueBins[index].ToArray(),
                    maximumRecursionDepth);

            return workloads;

        }

        //TODO - continue. (this is partially for distributed computation)

    }
}
