using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MDDF : IChunk
    {
        public MDDFEntry[] Entries;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            Entries = new MDDFEntry[header.Size / 12];
            for (int i = 0; i < Entries.Length; i++)
            {
                var entry = new MDDFEntry();
                entry.Read(header, br);
                Entries[i] = entry;
            }
        }

        // Sizeof = 12
        public class MDDFEntry : IChunk
        {
            public uint MMIDEntry;
            public uint UniqueId;
            public float[] Position;
            public float[] Rotation;
            public ushort Scale;
            public ushort Flags;        // MDDFFlags

            public void Read(ChunkHeader header, BinaryReader br)
            {
                MMIDEntry = br.ReadUInt32();
                UniqueId = br.ReadUInt32();
                Position = new float[3];
                for (int i = 0; i < 3; i++)
                    Position[i] = br.ReadSingle();
                Rotation = new float[3];
                for (int i = 0; i < 3; i++)
                    Rotation[i] = br.ReadSingle();
                Scale = br.ReadUInt16();
                Flags = br.ReadUInt16();
            }
        }

        public enum MDDFFlags : ushort
        {
            Biodome = 1, // this sets internal flags to | 0x800 (WDOODADDEF.var0xC).
            Shrubbery = 2 // the actual meaning of these is unknown to me. maybe biodome is for really big M2s.
        };
    }
}
