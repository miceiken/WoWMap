using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MODR : ChunkReader
    {
        public MODR(Chunk c, uint h) : base(c, h) { }
        public MODR(Chunk c) : base(c, c.Size) { }

        public ushort[] Offsets; // Indices to MODD

        public override void Read()
        {
            var br = Chunk.GetReader();

            Offsets = new ushort[Chunk.Size / 2];
            for (int i = 0; i < Offsets.Length; i++)
                Offsets[i] = br.ReadUInt16();
        }
    }
}
