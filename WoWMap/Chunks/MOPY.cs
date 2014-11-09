using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MOPY : ChunkReader
    {
        public MOPY(Chunk c, uint h) : base(c, h) { }
        public MOPY(Chunk c) : base(c, c.Size) { }

        public MOPYEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MOPYEntry[Chunk.Size / 2];
            for (int i = 0; i < Entries.Length; i++)
                Entries[i] = MOPYEntry.Read(br);
        }

        public class MOPYEntry
        {
            public MOPYFlags Flags;
            public byte MaterialId;

            [Flags]
            public enum MOPYFlags : byte
            {
                NoCamCollide = 0x01,
                Detail = 0x02,
                Collision = 0x04,
                Hint = 0x08,
                Render = 0x10,
                CollideHit = 0x20,
                WallSurface = 0x40,
            };

            public static MOPYEntry Read(BinaryReader br)
            {
                return new MOPYEntry
                {
                    Flags = (MOPYFlags)br.ReadByte(),
                    MaterialId = br.ReadByte(),
                };
            }
        }
    }
}
