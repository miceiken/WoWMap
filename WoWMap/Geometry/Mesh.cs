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
        public uint[] Indices;
        public Vector3[] Vertices;        
        public Vector3[] Normals;
    }

    public static class MeshHelpers
    {
        public static IEnumerable<Mesh> Transform(this IEnumerable<Mesh> meshes, Matrix4 mat)
        {
            return meshes.Select(m => m.Transform(mat));
        }

        public static Mesh Transform(this Mesh mesh, Matrix4 mat)
        {
            return new Mesh
            {
                Type = mesh.Type,
                Indices = mesh.Indices,
                Vertices = mesh.Vertices.Select(v => Vector3.Transform(v, mat)).ToArray(),
                Normals = mesh.Normals.Select(v => Vector3.Transform(v, mat)).ToArray(),
            };
        }
    }
}
