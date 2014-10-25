using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MWID : ChunkReader
    {
        public MWID(Chunk c, uint h) : base(c, h) { }
        public MWID(Chunk c) : base(c, c.Size) { }

        public uint[] Offsets;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Offsets = new uint[Chunk.Size / 4];
            for (int i = 0; i < Offsets.Length; i++)
                Offsets[i] = br.ReadUInt32();
        }
    }
}
