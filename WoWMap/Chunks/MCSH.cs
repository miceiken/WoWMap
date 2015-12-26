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

        private BitArray[] shadowMap; // Inefficient. Sue me.

        public override void Read()
        {
            var br = Chunk.GetReader();

            shadowMap = new BitArray[64];
            for (var i = 0; i < 64; ++i)
                shadowMap[i] = new BitArray(br.ReadBytes(64));
        }

        // TODO: Order swapped ?

        public bool White(int x, int y)
        {
            return shadowMap[x][y];
        }

        // No racism here.
        public bool Black(int x, int y)
        {
            return !White(x, y);
        }
    }
}
