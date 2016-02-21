using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;

namespace WoWMap.Geometry
{
    public class Scene
    {
        public ADT ADT { get; set; }

        public IEnumerable<Mesh> Terrain { get; set; }
        public IEnumerable<WMOScene> WorldModelObjects { get; set; }
        public IEnumerable<Mesh> Doodads { get; set; }
        public IEnumerable<Mesh> Liquids { get; set; }
    }

    public static class SceneHelpers
    {
        public static Mesh Flatten(this IEnumerable<Mesh> meshes, MeshType? type = null)
        {
            var indices = new List<uint>();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            foreach (var mesh in meshes)
            {
                var vo = (uint)vertices.Count;
                vertices.AddRange(mesh.Vertices);
                normals.AddRange(mesh.Normals);
                indices.AddRange(mesh.Indices.Select(i => vo + i));
            }

            return new Mesh
            {
                Type = type.HasValue ? type.Value : meshes.FirstOrDefault().Type,
                Indices = indices.ToArray(),
                Vertices = vertices.ToArray(),
                Normals = normals.ToArray(),
            };
        }

        public static Scene Transform(this Scene scene, Matrix4 mat)
        {
            return new Scene
            {
                ADT = scene.ADT,

                Terrain = scene.Terrain.Select(m => m.Transform(mat)),
                WorldModelObjects = scene.WorldModelObjects.Select(s => s.Transform(mat)),
                Doodads = scene.Doodads.Select(m => m.Transform(mat)),
                Liquids = scene.Liquids.Select(m => m.Transform(mat)),
            };
        }
    }
}
