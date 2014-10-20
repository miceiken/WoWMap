using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MVER : IChunk
    {
        public uint Version;
        
        public void Read(ChunkHeader header, BinaryReader br)
        {
            Version = br.ReadUInt32();
        }
    }
}
