using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCLQ : IChunk
    {
        private short unk0;
        private short unk1;
        public float Height;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            unk0 = br.ReadInt16();
            unk1 = br.ReadInt16();
            Height = br.ReadSingle();
        }
    }
}
