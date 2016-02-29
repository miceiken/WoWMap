using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMap.Geometry
{
    public enum MeshType : int
    {
        Terrain,
        WorldModelObject,
        Doodad,
        Liquid
    };

    public class Mesh
    {
        public MeshType Type;
        public uint[] Indices = Array.Empty<uint>();
        public Vector3[] Vertices = Array.Empty<Vector3>();
        public Vector3[] Normals = Array.Empty<Vector3>();

        public void GenerateIndices()
        {
            var indices = new List<uint>(Vertices.Length);
            for (uint vo = 0; vo < Vertices.Length; vo += 3)
                indices.AddRange(new uint[] {
                        vo, vo + 2, vo + 1,
                        vo + 2, vo + 3, vo + 1
                    });
            Indices = indices.ToArray();
        }

        //public void GenerateNormals()
        //{
        //    var normals = new List<Vector3>();
        //    for (var i = 0; i < Indices.Length; i++)
        //        normals.Add(Vector3.Cross((Vertices[i + 1] - Vertices[i]), (Vertices[i + 2] - Vertices[i])).Normalized());
        //}
    }

    public static class MeshHelpers
    {
        public static Mesh Flatten(this IEnumerable<Mesh> meshes, MeshType type)
        {
            var indices = new List<uint>();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            foreach (var mesh in meshes)
            {
                if (mesh == null) continue;
                var vo = (uint)vertices.Count;
                if (mesh.Vertices != null) vertices.AddRange(mesh.Vertices);
                if (mesh.Normals != null) normals.AddRange(mesh.Normals);
                if (mesh.Indices != null) indices.AddRange(mesh.Indices.Select(i => vo + i));
            }

            return new Mesh
            {
                Type = type,
                Indices = indices.ToArray(),
                Vertices = vertices.ToArray(),
                Normals = normals.ToArray(),
            };
        }

        public static IEnumerable<Mesh> Transform(this IEnumerable<Mesh> meshes, Matrix4 mat)
        {
            return meshes.OfType<Mesh>().Select(m => m.Transform(mat));
        }

        public static Mesh Transform(this Mesh mesh, Matrix4 mat)
        {
            var normalTransform = Matrix4.Transpose(Matrix4.Invert(mat));
            return new Mesh
            {
                Type = mesh.Type,
                Indices = mesh.Indices,
                Vertices = mesh.Vertices.OfType<Vector3>().Select(v => Vector3.Transform(v, mat)).ToArray(),
                Normals = mesh.Normals.OfType<Vector3>().Select(v => Vector3.Transform(v, normalTransform)).ToArray(),
            };
        }
    }
}
