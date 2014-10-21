using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCIN
    {
        public MCINEntry[] Entries;

        public void Read(BinaryReader br)
        {
            Entries = new MCINEntry[256];
            for (int i = 0; i < 256; i++) // 16*16
            {
                var entry = new MCINEntry();
                entry.Read(br);
                Entries[i] = entry;
            }
        }

        public class MCINEntry
        {
            public uint ofsMCNK;    // Absolute
            public uint Size;       // Size of MCNK chunk
            public uint Flags;      // Always 0
            public uint AsyncId;

            public void Read(BinaryReader br)
            {
                ofsMCNK = br.ReadUInt32();
                Size = br.ReadUInt32();
                Flags = br.ReadUInt32();
                AsyncId = br.ReadUInt32();
            }
        }
    }
}
