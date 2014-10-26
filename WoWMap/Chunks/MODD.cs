using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;
using WoWMap.Geometry;
using SharpDX;

namespace WoWMap.Chunks
{
    public class MODD : ChunkReader
    {
        public MODD(Chunk c) : base(c, c.Size) { }
        public MODD(Chunk c, uint h) : base(c, h) { }

        public MODDEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MODDEntry[Chunk.Size / 40];
            for (int i = 0; i < Entries.Length; i++)
                Entries[i] = MODDEntry.Read(br);
        }

        public class MODDEntry
        {
            public uint ofsMODN;
            public Vector3 Position;
            public float[] Rotation;        // Quaternion
            public float Scale;
            public byte[] Color;

            public static MODDEntry Read(BinaryReader br)
            {
                var e = new MODDEntry();
                e.ofsMODN = br.ReadUInt32();
                e.Position = br.ReadVector3();
                e.Rotation = new float[4];
                for (int i = 0; i < 4; i++)
                    e.Rotation[i] = br.ReadSingle();
                e.Scale = br.ReadSingle();
                e.Color = new byte[4];
                for (int i = 0; i < 4; i++)
                    e.Color[i] = br.ReadByte();
                return e;
            }
        }
    }
}
