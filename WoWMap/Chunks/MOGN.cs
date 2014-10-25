using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MOGN : ChunkReader
    {
        public MOGN(Chunk c) : base(c, c.Size) { }
        public MOGN(Chunk c, uint h) : base(c, h) { }

        public string[] GroupNames;

        public override void Read()
        {
            var br = Chunk.GetReader();

            var chunk = br.ReadBytes((int)Chunk.Size);
            GroupNames = Helpers.SplitStrings(chunk).ToArray();
        }
    }
}
