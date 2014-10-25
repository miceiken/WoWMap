using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MMDX : ChunkReader
    {
        public MMDX(Chunk c, uint h) : base(c, h) { }
        public MMDX(Chunk c) : base(c, c.Size) { }

        public string[] Filenames;

        public override void Read()
        {
            var br = Chunk.GetReader();

            var chunk = br.ReadBytes((int)Chunk.Size);
            Filenames = Helpers.SplitStrings(chunk).ToArray();
        }
    }
}
