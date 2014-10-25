using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCIN : ChunkReader
    {
        public MCIN(Chunk c, uint h) : base(c, h) { }
        public MCIN(Chunk c) : base(c, c.Size) { }

        public MCINEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MCINEntry[256];
            for (int i = 0; i < 256; i++) // 16*16
                Entries[i] = MCINEntry.Read(br);
        }

        public class MCINEntry
        {
            public uint ofsMCNK;    // Absolute
            public uint Size;       // Size of MCNK chunk
            public uint Flags;      // Always 0
            public uint AsyncId;

            public static MCINEntry Read(BinaryReader br)
            {
                return new MCINEntry
                {
                    ofsMCNK = br.ReadUInt32(),
                    Size = br.ReadUInt32(),
                    Flags = br.ReadUInt32(),
                    AsyncId = br.ReadUInt32(),
                };
            }
        }
    }
}
