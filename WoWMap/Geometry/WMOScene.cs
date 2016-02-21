using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WoWMap.Geometry
{
    public class WMOScene
    {
        public IEnumerable<Mesh> Terrain { get; set; } = Enumerable.Empty<Mesh>();
        public IEnumerable<Mesh> Doodads { get; set; } = Enumerable.Empty<Mesh>();
        public IEnumerable<Mesh> Liquids { get; set; } = Enumerable.Empty<Mesh>();
    }

    public static class WMOSceneHelpers
    {
        public static Mesh Flatten(this WMOScene scene)
        {
            // ew
            return scene.Terrain
                .Concat(scene.Doodads)
                .Concat(scene.Liquids)
                .OfType<Mesh>()
                .Flatten(MeshType.WorldModelObject);
        }

        public static WMOScene Transform(this WMOScene scene, Matrix4 mat)
        {
            return new WMOScene
            {
                Terrain = scene.Terrain.OfType<Mesh>().Transform(mat),
                Doodads = scene.Doodads.OfType<Mesh>().Transform(mat),
                Liquids = scene.Liquids.OfType<Mesh>().Transform(mat),
            };
        }
    }
}
