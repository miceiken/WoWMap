using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MVER : ChunkReader
    {
        public MVER(Chunk c, uint h) : base(c, h) { }
        public MVER(Chunk c) : base(c, c.Size) { }

        public uint Version;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Version = br.ReadUInt32();
        }
    }
}
