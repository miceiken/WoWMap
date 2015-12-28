using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public sealed class MAMP : ChunkReader
    {
        public MAMP(Chunk c, uint h) : base(c, h) { }
        public MAMP(Chunk c) : base(c, c.Size) { }

        public int TextureSize { get; private set; }

        public override void Read()
        {
            var br = Chunk.GetReader();

            TextureSize = (int)(64 / Math.Pow(2, br.ReadByte()));
        }
    }
}
