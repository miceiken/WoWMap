using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MOVI : ChunkReader
    {
        public MOVI(Chunk c, uint h) : base(c, h) { }
        public MOVI(Chunk c) : base(c, c.Size) { }

        public Triangle<ushort>[] Indices;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Indices = new Triangle<ushort>[Chunk.Size / 6];
            for (int i = 0; i < Indices.Length; i++)
                Indices[i] = new Triangle<ushort>(TriangleType.Wmo, br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16());
        } 
    }
}
