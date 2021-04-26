using System;
using System.Collections.Generic;
using System.IO;

namespace MandelbrotSet
{
    public static class LoadData
    {

        public static double[][] Load(string filename)
        {

            if (!File.Exists(filename))
                throw new ArgumentException("Provided filename doesn't exist");

            ulong rowLength;
            List<double[]> dataList = new List<double[]>();

            using (BinaryReader stream = new BinaryReader(File.Open(filename, FileMode.Open)))
            {

                rowLength = stream.ReadUInt64() - 1;

                List<double> currentRow = new List<double>();

                ulong rowIndex = 0;

                while (stream.BaseStream.Position != stream.BaseStream.Length)
                {

                    ushort value = stream.ReadUInt16();

                    currentRow.Add(value);

                    if (rowIndex == rowLength)
                    {
                        rowIndex = 0;
                        dataList.Add(currentRow.ToArray());
                        currentRow.Clear();
                    }
                    else
                        rowIndex++;

                }

                return dataList.ToArray();

            }

        }

    }
}
