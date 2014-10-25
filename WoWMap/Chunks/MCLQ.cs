using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCLQ : ChunkReader
    {
        public MCLQ(Chunk c, uint h) : base(c, h) { }
        public MCLQ(Chunk c) : base(c, c.Size) { }

        private short unk0;
        private short unk1;
        public float Height;

        public override void Read()
        {
            var br = Chunk.GetReader();

            unk0 = br.ReadInt16();
            unk1 = br.ReadInt16();
            Height = br.ReadSingle();
        }
    }
}
