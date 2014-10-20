using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MODF : IChunkReader
    {
        public MODFEntry[] Entries;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            for (int i = 0; i < header.Size / 22; i++)
            {
                var entry = new MODFEntry();
                entry.Read(header, br);
                Entries[i] = entry;
            }
        }

        // Sizeof = 22
        public class MODFEntry : IChunkReader
        {
            public uint MWIDEntry;
            public uint UniqueId;
            public float[] Position;
            public float[] Rotation;
            public float[] LowerBounds;
            public float[] UpperBounds;
            public ushort Flags; // MODFFlags
            public ushort DoodadSet;
            public ushort NameSet;
            private ushort padding;

            public void Read(ChunkHeader header, BinaryReader br)
            {
                MWIDEntry = br.ReadUInt32();
                UniqueId = br.ReadUInt32();
                for (int i = 0; i < 3; i++)
                    Position[i] = br.ReadSingle();
                for (int i = 0; i < 3; i++)
                    Rotation[i] = br.ReadSingle();
                for (int i = 0; i < 3; i++)
                    LowerBounds[i] = br.ReadSingle();
                for (int i = 0; i < 3; i++)
                    UpperBounds[i] = br.ReadSingle();
                Flags = br.ReadUInt16();
                DoodadSet = br.ReadUInt16();
                NameSet = br.ReadUInt16();
                padding = br.ReadUInt16();
            }
        }

        public enum MODFFlags : ushort
        {
            Destroyable = 1
        };
    }
}
