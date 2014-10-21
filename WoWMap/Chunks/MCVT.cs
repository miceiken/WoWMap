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
        public float[] Height;

        public void Read(BinaryReader br)
        {
            Height = new float[145];
            for (int i = 0; i < 145; i++)
                Height[i] = br.ReadSingle();
        }
    }
}
