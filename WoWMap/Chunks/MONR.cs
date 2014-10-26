using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using WoWMap.Readers;
using SharpDX;

namespace WoWMap.Chunks
{
    public class MONR : ChunkReader
    {
        public MONR(Chunk c, uint h) : base(c, h) { }
        public MONR(Chunk c) : base(c, c.Size) { }

        public Vector3[] Normals;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Normals = new Vector3[Chunk.Size / 12];
            for (int i = 0; i < Normals.Length; i++)
                Normals[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), -br.ReadSingle()); // br.ReadVector3();
        }
    }
}
