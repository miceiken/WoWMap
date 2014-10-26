using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MODN : ChunkReader
    {
        public MODN(Chunk c, uint h) : base(c, h) { }
        public MODN(Chunk c) : base(c, c.Size) { }

        public Dictionary<uint, string> Filenames;

        public override void Read()
        {
            var br = Chunk.GetReader();

            var chunk = br.ReadBytes((int)Chunk.Size);
            Filenames = Helpers.GetIndexedStrings(chunk);
        }
    }
}
