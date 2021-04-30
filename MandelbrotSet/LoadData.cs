using System;
using System.Collections.Generic;
using System.IO;

namespace MandelbrotSet
{
    public static class LoadData
    {

        public static float[][] Load(string filename)
        {

            if (!File.Exists(filename))
                throw new ArgumentException("Provided filename doesn't exist");

            ulong rowLength;
            List<float[]> dataList = new List<float[]>();

            using (BinaryReader stream = new BinaryReader(File.Open(filename, FileMode.Open)))
            {

                rowLength = stream.ReadUInt64() - 1;

                List<float> currentRow = new List<float>();

                ulong rowIndex = 0;

                while (stream.BaseStream.Position != stream.BaseStream.Length)
                {

                    ushort fileValue = stream.ReadUInt16();
                    float value = (float)fileValue / ushort.MaxValue;

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
