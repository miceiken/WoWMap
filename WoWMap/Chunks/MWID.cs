using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MWID : IChunk
    {
        public uint[] Offsets;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            Offsets = new uint[header.Size / 4];
            for (int i = 0; i < Offsets.Length; i++)
                Offsets[i] = br.ReadUInt32();
        }
    }
}
