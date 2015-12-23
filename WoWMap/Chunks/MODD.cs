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
    public class MODD : ChunkReader
    {
        public MODD(Chunk c) : base(c, c.Size) { }
        public MODD(Chunk c, uint h) : base(c, h) { }

        public MODDEntry[] Entries;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MODDEntry[Chunk.Size / 40];
            for (var i = 0; i < Entries.Length; i++)
                Entries[i] = MODDEntry.Read(br);
        }

        public class MODDEntry
        {
            public uint ofsMODN;
            public Vector3 Position;
            public Quaternion Rotation;        // Quaternion
            public float Scale;
            public byte[] Color;

            public static MODDEntry Read(BinaryReader br)
            {
                return new MODDEntry
                {
                    ofsMODN = br.ReadUInt32(),
                    Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    Rotation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle()),
                    Scale = br.ReadSingle(),
                    Color = new[] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() }
                };
            }
        }
    }
}
