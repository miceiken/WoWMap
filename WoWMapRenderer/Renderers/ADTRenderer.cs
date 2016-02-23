using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap;
using WoWMap.Geometry;
using WoWMap.Layers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer.Renderers
{
    public class ADTRenderer : IRenderer
    {
        public ADTRenderer(RenderView controller, ADT adt)
        {
            Controller = controller;
            ADT = adt;
            MeshRenderers = new GenericCollectionRenderer<MeshRenderer>(Controller);
        }

        public RenderView Controller { get; private set; }
        public ADT ADT { get; private set; }
        public GenericCollectionRenderer<MeshRenderer> MeshRenderers { get; private set; }

        public void Generate()
        {
            for (var i = 0; i < ADT.MapChunks.Count; ++i)
            {
                var mapChunk = ADT.MapChunks[i];
                if (mapChunk == null)
                    continue;

                if (!mapChunk.Scene.Terrain.IsNullOrEmpty())
                    MeshRenderers.Renderers.Add(new MeshRenderer(Controller, mapChunk.Scene.Terrain.Flatten(MeshType.Terrain)));
                if (!mapChunk.Scene.Doodads.IsNullOrEmpty())
                    MeshRenderers.Renderers.Add(new MeshRenderer(Controller, mapChunk.Scene.Doodads.Flatten(MeshType.Doodad)));
                if (!mapChunk.Scene.Liquids.IsNullOrEmpty())
                    MeshRenderers.Renderers.Add(new MeshRenderer(Controller, mapChunk.Scene.Liquids.Flatten(MeshType.Liquid)));
                if (!mapChunk.Scene.WorldModelObjects.IsNullOrEmpty())
                {
                    var flat = mapChunk.Scene.WorldModelObjects.Select(s => s.Flatten());
                    if (!flat.IsNullOrEmpty())
                        MeshRenderers.Renderers.Add(new MeshRenderer(Controller, flat.Flatten(MeshType.WorldModelObject)));
                }
            }
            MeshRenderers.Renderers.RemoveAll(m => m.IndiceCount == 0);
        }

        public void Update() => MeshRenderers.Update();
        public void Bind(Shader shader) => MeshRenderers.Bind(shader);
        public void Delete() => MeshRenderers.Delete();
        public void Render(Shader shader)
        {
            MeshRenderers.Render(shader);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
