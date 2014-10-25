using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCRW : ChunkReader
    {
        public MCRW(Chunk c, uint h) : base(c, h) { }
        public MCRW(Chunk c) : base(c, c.Size) { }

        public uint[] MODFEntries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            MODFEntries = new uint[Chunk.Size / 4];
            for (int i = 0; i < MODFEntries.Length; i++)
                MODFEntries[i] = br.ReadUInt32();
        }
    }
}
