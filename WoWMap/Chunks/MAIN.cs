using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MAIN
    {
        public MAINEntry[,] Entries;

        public void Read(BinaryReader br)
        {
            Entries = new MAINEntry[64, 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    var entry = new MAINEntry();
                    entry.Read(br);
                    Entries[x, y] = entry;
                }
            }
        }

        public class MAINEntry
        {
            public MAINFlags Flags;
            public uint Area; // Set during runtime

            public void Read(BinaryReader br)
            {
                Flags = (MAINFlags)br.ReadUInt32();
                Area = br.ReadUInt32();
            }
        }

        public enum MAINFlags : uint
        {
            HasADT = 1,
            AllWater = 2 // >= Cata Allwater else, Loaded
        };
    }
}
