using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCLY : ChunkReader
    {
        public MCLY(Chunk c, uint h) : base(c, h) { }
        public MCLY(Chunk c) : base(c, c.Size) { }

        public struct MCLYEntry
        {
            public uint TextureId;
            public MCLYFlags Flags;
            public uint ofsMCAL;
            public short EffectId;

            public bool HasFlag(MCLYFlags flags)
            {
                return (Flags & flags) != 0;
            }
        }

        public MCLYEntry[] Entries;

        [Flags]
        public enum MCLYFlags : uint
        {
            Rotate45 = 0x1,
            Rotate90 = 0x2,
            Rotate180 = 0x4,
            Fast = 0x8,
            Faster = 0x10,
            Fastest = 0x20,
            Animate = 0x40,
            Brighter = 0x80,
            AlphaMap = 0x100,
            CompressedAlphaMap = 0x200,
            SkyboxReflection = 0x400
        };

        public override void Read()
        {
            var br = Chunk.GetReader();
            if (Chunk.Size == 0)
                return;

            var recordCount = Chunk.Size / 16;
            Entries = new MCLYEntry[recordCount];
            for (var i = 0; i < recordCount; ++i)
            {
                var entry = new MCLYEntry();
                entry.TextureId = br.ReadUInt32();
                entry.Flags = (MCLYFlags)br.ReadUInt32();
                entry.ofsMCAL = br.ReadUInt32();
                entry.EffectId = br.ReadInt16();
                br.ReadInt16(); // "Padding"
                Entries[i] = entry;
            }
        }
    }
}
