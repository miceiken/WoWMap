using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MFBO
    {
        public Plane Maximum;
        public Plane Minimum;

        public void Read(BinaryReader br)
        {
            Maximum = new Plane();
            Maximum.Read(br);

            Minimum = new Plane();
            Minimum.Read(br);
        }


        public class Plane
        {
            short[,] Height;

            public void Read(BinaryReader br)
            {
                Height = new short[3,3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        Height[i, j] = br.ReadInt16();
            }
        }
    }
}
