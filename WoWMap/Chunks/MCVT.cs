using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCVT
    {
        public float[,] HeightMap;

        public void Read(BinaryReader br)
        {
            var heights = new float[145];
            for (int i = 0; i < 145; i++)
                heights[i] = br.ReadSingle();

            HeightMap = new float[9, 9];
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    HeightMap[r, c] = heights[r * 17 + c];
        }
    }
}
