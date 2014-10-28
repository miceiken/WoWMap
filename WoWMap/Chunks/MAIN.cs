using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MAIN : ChunkReader
    {
        public MAIN(Chunk c, uint h) : base(c, h) { }
        public MAIN(Chunk c) : base(c, c.Size) { }

        public MAINEntry[,] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MAINEntry[64, 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    Entries[x, y] = MAINEntry.Read(br);
        }

        public class MAINEntry
        {
            public MAINFlags Flags;
            public uint Area; // Set during runtime

            public static MAINEntry Read(BinaryReader br)
            {
                return new MAINEntry
                {
                    Flags = (MAINFlags)br.ReadUInt32(),
                    Area = br.ReadUInt32(),
                };
            }
        }

        [Flags]
        public enum MAINFlags : uint
        {
            HasADT = 1,
            AllWater = 2 // >= Cata Allwater else, Loaded
        };
    }
}
