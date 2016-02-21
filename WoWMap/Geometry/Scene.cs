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

        public IEnumerable<Mesh> Terrain { get; set; } = Enumerable.Empty<Mesh>();
        public IEnumerable<WMOScene> WorldModelObjects { get; set; } = Enumerable.Empty<WMOScene>();
        public IEnumerable<Mesh> Doodads { get; set; } = Enumerable.Empty<Mesh>();
        public IEnumerable<Mesh> Liquids { get; set; } = Enumerable.Empty<Mesh>();
    }

    public static class SceneHelpers
    {
        public static Scene Transform(this Scene scene, Matrix4 mat)
        {
            return new Scene
            {
                ADT = scene.ADT,

                Terrain = scene.Terrain.OfType<Mesh>().Select(m => m.Transform(mat)),
                WorldModelObjects = scene.WorldModelObjects.Select(s => s.Transform(mat)),
                Doodads = scene.Doodads.OfType<Mesh>().Select(m => m.Transform(mat)),
                Liquids = scene.Liquids.OfType<Mesh>().Select(m => m.Transform(mat)),
            };
        }
    }
}
