using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCSH : ChunkReader
    {
        public MCSH(Chunk c, uint h) : base(c, h) { }
        public MCSH(Chunk c) : base(c, c.Size) { }

        public BitArray[] ShadowMap { get; private set; } // Inefficient. Sue me.

        public override void Read()
        {
            var br = Chunk.GetReader();

            ShadowMap = new BitArray[64];
            for (var i = 0; i < 64; ++i)
                ShadowMap[i] = new BitArray(br.ReadBytes(64));
        }
    }
}
