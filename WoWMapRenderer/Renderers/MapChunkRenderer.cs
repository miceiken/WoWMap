using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WoWMap;
using WoWMap.Chunks;
using WoWMap.Geometry;
using WoWMap.Layers;

namespace WoWMapRenderer.Renderers
{
    public class MapChunkRenderer : IRenderer
    {
        private List<MeshRenderer> _meshRenderers = new List<MeshRenderer>();

        public TerrainRenderer BaseRenderer { get; private set; }

        public MapChunkRenderer(TerrainRenderer baseRenderer)
        {
            BaseRenderer = baseRenderer;
        }

        public void Generate(ADT tile)
        {
            for (var i = 0; i < tile.MapChunks.Count; ++i)
            {
                var mapChunk = tile.MapChunks[i];
                if (mapChunk == null)
                    continue;

                if (!mapChunk.Scene.Terrain.IsNullOrEmpty())
                    _meshRenderers.Add(new MeshRenderer(mapChunk.Scene.Terrain.Flatten(MeshType.Terrain)));
                if (!mapChunk.Scene.Doodads.IsNullOrEmpty())
                    _meshRenderers.Add(new MeshRenderer(mapChunk.Scene.Doodads.Flatten(MeshType.Doodad)));
                if (!mapChunk.Scene.Liquids.IsNullOrEmpty())
                    _meshRenderers.Add(new MeshRenderer(mapChunk.Scene.Liquids.Flatten(MeshType.Liquid)));
                if (!mapChunk.Scene.WorldModelObjects.IsNullOrEmpty())
                {
                    var flat = mapChunk.Scene.WorldModelObjects.Select(s => s.Flatten());
                    if (!flat.IsNullOrEmpty())
                        _meshRenderers.Add(new MeshRenderer(flat.Flatten(MeshType.WorldModelObject)));
                }
            }
        }

        public void Bind(Shader shader)
        {
            foreach (var meshRenderer in _meshRenderers)
                meshRenderer.Bind(shader);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        {
            foreach (var meshRenderer in _meshRenderers)
                meshRenderer.Delete();
        }

        public void Render(Shader shader)
        {
            foreach (var meshRenderer in _meshRenderers)
                meshRenderer.Render(shader);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
