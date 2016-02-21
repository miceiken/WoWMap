using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Archive;
using WoWMap.Chunks;
using WoWMap.Geometry;
using OpenTK;

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
        public Mesh Mesh { get; private set; }

        public void Read()
        {
            if (MD20.OffsetBoundingNormals == 0
                || MD20.OffsetBoundingVertices == 0
                || MD20.OffsetBoundingTriangles == 0
                || MD20.BoundingRadius == 0.0f)
                return;

            IsCollidable = true;

            Mesh = new Mesh();

            var stream = Chunk.GetStream();
            stream.Seek(MD20.OffsetBoundingVertices, SeekOrigin.Begin);
            var br = Chunk.GetReader();

            var vertices = new Vector3[MD20.CountBoundingVertices];
            for (var i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            stream.Seek(MD20.OffsetBoundingTriangles, SeekOrigin.Begin);
            br = Chunk.GetReader();

            var indices = new ushort[MD20.CountBoundingTriangles];
            for (var i = 0; i < indices.Length; i++)
                indices[i] = br.ReadUInt16();

            stream.Seek(MD20.OffsetBoundingNormals, SeekOrigin.Begin);
            br = Chunk.GetReader();

            var normals = new Vector3[MD20.CountBoundingNormals];
            for (var i = 0; i < normals.Length; i++)
                normals[i] = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            Mesh = new Mesh
            {
                Type = MeshType.Doodad,
                Indices = indices.Select(i => (uint)i).ToArray(),
                Vertices = vertices.ToArray(),
                Normals = normals.ToArray()
            };
        }
    }
}
