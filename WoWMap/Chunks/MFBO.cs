using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MFBO : IChunk
    {
        public Plane Maximum;
        public Plane Minimum;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            Maximum = new Plane();
            Maximum.Read(header, br);

            Minimum = new Plane();
            Minimum.Read(header, br);
        }


        public class Plane : IChunk
        {
            short[,] Height;

            public void Read(ChunkHeader header, BinaryReader br)
            {
                Height = new short[8,8];
                for (int i = 0; i < 8; i++)
                    for (int j = 0; j < 8; j++)
                        Height[i, j] = br.ReadInt16();
            }
        }
    }
}
