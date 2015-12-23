using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using WoWMap.Readers;
using OpenTK;

namespace WoWMap.Chunks
{
    public class MODF : ChunkReader
    {
        public MODF(Chunk c, uint h) : base(c, h) { }
        public MODF(Chunk c) : base(c, c.Size) { }

        public MODFEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MODFEntry[Chunk.Size / 64];
            for (int i = 0; i < Entries.Length; i++)
                Entries[i] = MODFEntry.Read(br);
        }

        public class MODFEntry
        {
            public uint MWIDEntryIndex;
            public uint UniqueId;
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 LowerBounds;
            public Vector3 UpperBounds;
            public MODFFlags Flags;
            public ushort DoodadSet;
            public ushort NameSet;
            private ushort padding;

            [Flags]
            public enum MODFFlags : ushort
            {
                Destroyable = 1
            };

            public static MODFEntry Read(BinaryReader br)
            {
                return new MODFEntry
                {
                    MWIDEntryIndex = br.ReadUInt32(),
                    UniqueId = br.ReadUInt32(),
                    Position = br.ReadVector3(),
                    Rotation = br.ReadVector3(),
                    LowerBounds = br.ReadVector3(),
                    UpperBounds = br.ReadVector3(),
                    Flags = (MODFFlags)br.ReadUInt16(),
                    DoodadSet = br.ReadUInt16(),
                    NameSet = br.ReadUInt16(),
                    padding = br.ReadUInt16(),
                };
            }
        }
    }
}
