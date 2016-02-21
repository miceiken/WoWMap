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

                _meshRenderers.AddRange(new[]
                {
                    new MeshRenderer(mapChunk.Scene.Terrain.Flatten()),
                    new MeshRenderer(mapChunk.Scene.Doodads.Flatten()),
                    new MeshRenderer(mapChunk.Scene.Liquids.Flatten()),
                    new MeshRenderer(mapChunk.Scene.WorldModelObjects.Flatten())
                });
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
