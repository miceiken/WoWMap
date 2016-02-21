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
        public IEnumerable<Mesh> Terrain { get; set; }
        public IEnumerable<Mesh> Doodads { get; set; }
        public IEnumerable<Mesh> Liquids { get; set; }
    }

    public static class WMOSceneHelpers
    {
        public static Mesh Flatten(this WMOScene scene)
        {
            // ew
            return scene.Terrain
                .Concat(scene.Doodads)
                .Concat(scene.Liquids)
                .Flatten(MeshType.WorldModelObject);
        }

        public static Mesh Flatten(this IEnumerable<WMOScene> scenes)
        {
            // ew
            return scenes
                .Select(s => s.Flatten())
                .Flatten();
        }

        public static WMOScene Transform(this WMOScene scene, Matrix4 mat)
        {
            return new WMOScene
            {
                Terrain = scene.Terrain.Transform(mat),
                Doodads = scene.Doodads.Transform(mat),
                Liquids = scene.Liquids.Transform(mat),
            };
        }
    }
}
