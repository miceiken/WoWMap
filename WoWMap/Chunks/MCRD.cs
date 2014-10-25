using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCRD : ChunkReader
    {
        public MCRD(Chunk c, uint h) : base(c, h) { }
        public MCRD(Chunk c) : base(c, c.Size) { }

        public uint[] MDDFEntries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            MDDFEntries = new uint[Chunk.Size / 4];
            for (int i = 0; i < MDDFEntries.Length; i++)
                MDDFEntries[i] = br.ReadUInt32();
        }
    }
}
