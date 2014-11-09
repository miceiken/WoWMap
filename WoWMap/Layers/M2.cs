using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Archive;
using WoWMap.Chunks;
using WoWMap.Geometry;
using SharpDX;

namespace WoWMap.Layers
{
    public class M2
    {
        public M2(string filename)
        {
            Filename = filename;

            var stream = CASC.OpenFile(Path.ChangeExtension(filename, ".M2"));
            Chunk = new Chunk("MD20", 0, (uint)stream.Position, stream);
            MD20 = new MD20(Chunk);

            Read();
        }

        public string Filename { get; private set; }
        public Chunk Chunk { get; private set; }

        public MD20 MD20 { get; private set; }

        public bool IsCollidable { get; private set; }

        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Triangle<ushort>[] Indices { get; private set; }

        public void Read()
        {
            if (MD20.OffsetBoundingNormals == 0
                || MD20.OffsetBoundingVertices == 0
                || MD20.OffsetBoundingTriangles == 0
                || MD20.BoundingRadius == 0.0f)
                return;

            IsCollidable = true;

            ReadVertices();
            ReadIndices();
            ReadNormals();
        }

        private void ReadVertices()
        {
            var stream = Chunk.GetStream();
            stream.Seek(MD20.OffsetBoundingVertices, SeekOrigin.Begin);
            var br = Chunk.GetReader();

            Vertices = new Vector3[MD20.CountBoundingVertices];
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), -br.ReadSingle());
        }

        private void ReadIndices()
        {
            var stream = Chunk.GetStream();
            stream.Seek(MD20.OffsetBoundingTriangles, SeekOrigin.Begin);
            var br = Chunk.GetReader();

            Indices = new Triangle<ushort>[MD20.CountBoundingTriangles / 3];
            for (int i = 0; i < Indices.Length; i++)
                Indices[i] = new Triangle<ushort>(TriangleType.Doodad, br.ReadUInt16(), br.ReadUInt16(), br.ReadUInt16());
        }

        private void ReadNormals()
        {
            var stream = Chunk.GetStream();
            stream.Seek(MD20.OffsetBoundingNormals, SeekOrigin.Begin);
            var br = Chunk.GetReader();

            Normals = new Vector3[MD20.CountBoundingNormals];
            for (int i = 0; i < Normals.Length; i++)
                Normals[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), -br.ReadSingle());
        }
    }
}
