using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MMID : IChunkReader
    {
        public uint[] Offsets;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            for (int i = 0; i < header.Size / 4; i++)
                Offsets[i] = br.ReadUInt32();
        }
    }
}
