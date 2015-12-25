using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;
using WoWMap.Geometry;
using OpenTK;

namespace WoWMap.Chunks
{
    public class MDDF : ChunkReader
    {
        public MDDF(Chunk c, uint h) : base(c, h) { }
        public MDDF(Chunk c) : base(c, c.Size) { }

        public MDDFEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MDDFEntry[Chunk.Size / 36];
            for (var i = 0; i < Entries.Length; i++)
                Entries[i] = MDDFEntry.Read(br);
        }

        public class MDDFEntry
        {
            public uint MMIDEntryIndex;
            public uint UniqueId;
            public Vector3 Position;
            public Vector3 Rotation;
            public ushort Scale;
            public MDDFFlags Flags;

            public static MDDFEntry Read(BinaryReader br)
            {
                return new MDDFEntry
                {
                    MMIDEntryIndex = br.ReadUInt32(),
                    UniqueId = br.ReadUInt32(),
                    Position = br.ReadVector3(),
                    Rotation = br.ReadVector3(),
                    Scale = br.ReadUInt16(),
                    Flags = (MDDFFlags)br.ReadUInt16(),
                };
            }
        }

        [Flags]
        public enum MDDFFlags : ushort
        {
            Biodome = 1, // this sets internal flags to | 0x800 (WDOODADDEF.var0xC).
            Shrubbery = 2 // the actual meaning of these is unknown to me. maybe biodome is for really big M2s.
        };
    }
}
