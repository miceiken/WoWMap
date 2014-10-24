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
        public float[] Heights;

        //public static readonly int ChunkSize = 580;

        public void Read(BinaryReader br)
        {
            Heights = new float[145];
            for (int i = 0; i < 145; i++)
                Heights[i] = br.ReadSingle();
        }
    }
}
