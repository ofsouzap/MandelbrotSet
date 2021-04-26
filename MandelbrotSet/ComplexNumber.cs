using System;
using System.Collections.Generic;

namespace MandelbrotSet
{
    public struct ComplexNumber
    {

        public double r;
        public double i;

        public ComplexNumber(double r,
            double i)
        {
            this.r = r;
            this.i = i;
        }

        public static ComplexNumber[][] GenerateComplexNumbersFromRange(
            double minR,
            double maxR,
            double stepR,
            double minI,
            double maxI,
            double stepI)
        {

            if (maxR < minR)
            {
                throw new ArgumentException("Maximum r value is lesser than minimum value");
            }

            if (maxI < minI)
            {
                throw new ArgumentException("Maximum i value is lesser than minimum value");
            }

            List<ComplexNumber[]> output = new List<ComplexNumber[]>();

            for (double i = minI; i < maxI; i += stepI)
            {

                List<ComplexNumber> current = new List<ComplexNumber>();

                for (double r = minR; r < maxR; r += stepR)
                {
                    current.Add(new ComplexNumber(r, i));
                }

                output.Add(current.ToArray());

            }

            return output.ToArray();

        }

        public string StringRepresentation
            => $"{r}+{i}i";

        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
        {

            return new ComplexNumber(
                a.r + b.r,
                a.i + b.i
                );

        }

        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
        {

            return new ComplexNumber(
                (a.r * b.r) - (a.i * b.i),
                (a.r * b.i) + (a.i * b.r)
                );

        }

        public ComplexNumber Squared => this * this;

        public double OriginRadius => Math.Sqrt((r * r) + (i * i));

    }
}
