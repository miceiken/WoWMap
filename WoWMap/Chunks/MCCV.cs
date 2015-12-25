using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCCV : ChunkReader
    {
        public MCCV(Chunk c, uint h) : base(c, h) { }
        public MCCV(Chunk c) : base(c, c.Size) { }

        public MCCVEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MCCVEntry[9 * 9 + 8 * 8];
            for (int i = 0; i < 9 * 9 + 8 * 8; i++)
                Entries[i] = MCCVEntry.Read(br);
        }

        public class MCCVEntry
        {
            public byte Red;
            public byte Green;
            public byte Blue;
            public byte Alpha;

            public static MCCVEntry Read(BinaryReader br)
            {
                return new MCCVEntry
                {
                    Red = br.ReadByte(),
                    Green = br.ReadByte(),
                    Blue = br.ReadByte(),
                    Alpha = br.ReadByte(),
                };
            }
        }
    }
}
