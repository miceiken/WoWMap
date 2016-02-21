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

        public void GenerateNormals() => Normals = Vertices.Select(v => v.Normalized()).ToArray();
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
            return new Mesh
            {
                Type = mesh.Type,
                Indices = mesh.Indices,
                Vertices = mesh.Vertices.OfType<Vector3>().Select(v => Vector3.Transform(v, mat)).ToArray(),
                Normals = mesh.Normals.OfType<Vector3>().Select(v => Vector3.Transform(v, mat)).ToArray(),
            };
        }
    }
}
