using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;

namespace WoWMap
{
    public interface IChunkReader
    {
        void Read(ChunkHeader header, BinaryReader br);
    }
}
