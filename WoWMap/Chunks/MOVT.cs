using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OpenTK;
using WoWMap.Geometry;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MOVT : ChunkReader
    {
        public MOVT(Chunk c, uint h) : base(c, h) { }
        public MOVT(Chunk c) : base(c, c.Size) { }

        public Vector3[] Vertices;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Vertices = new Vector3[Chunk.Size / 12];
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
    }
}
