using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MTEX : IChunk
    {
        public string[] Filenames;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            var chunk = br.ReadBytes((int)header.Size);
            Filenames = Helpers.SplitStrings(chunk).ToArray();
        }
    }
}
