using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCVT : ChunkReader
    {
        public MCVT(Chunk c, uint h) : base(c, h) { }
        public MCVT(Chunk c) : base(c, c.Size) { }

        public float[] Heights;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Heights = new float[145];
            for (int i = 0; i < 145; i++)
                Heights[i] = br.ReadSingle();
        }
    }
}
