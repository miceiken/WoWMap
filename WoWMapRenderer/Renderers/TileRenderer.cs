using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WoWMap;
using WoWMap.Chunks;
using WoWMap.Layers;

namespace WoWMapRenderer.Renderers
{
    class TileRenderer
    {
        public List<MapChunkRenderer> Renderers { get; private set; }

        public int VAO { get; private set; }
        public int VerticeVBO { get; private set; }
        public int IndicesVBO { get; private set; }

        private List<Vertex> _vertices = new List<Vertex>();
        private List<ushort> _indices = new List<ushort>();

        public int VerticeCount { get { return _vertices.Count; } }
        public int IndiceCount { get { return _indices.Count; } }

        private List<Texture>[] _textures = new List<Texture>[255];

        public TileRenderer()
        {
            Renderers = new List<MapChunkRenderer>();
        }

        ~TileRenderer()
        {
            /*foreach (var r in Renderers)
                r.Delete();
            Renderers.Clear();*/
        }

        public void Generate(ADT tile)
        {
            for (var i = 0; i < tile.MapChunks.Count; ++i)
            {
                var mapChunk = tile.MapChunks[i];
                if (mapChunk == null)
                    continue;

                var mapChunkRenderer = new MapChunkRenderer(mapChunk);
                mapChunkRenderer.AddTextureNames(tile.MTEX);

                var mapChunkIndiceStart = IndiceCount;

                GenerateIndices(mapChunk);
                GenerateVertices(mapChunk);

                var mapChunkIndiceCount = IndiceCount - mapChunkIndiceStart;
                mapChunkRenderer.SetIndices(mapChunkIndiceCount, mapChunkIndiceStart);

                mapChunkRenderer.ApplyAlphaMap(mapChunk.MCAL);
                // mapChunkRenderer.ApplyShadowMap(mapChunk.MCSH);

                Renderers.Add(mapChunkRenderer);
            }
        }

        private void GenerateVertices(MapChunk mapChunk)
        {
            for (int i = 0, idx = 0; i < 17; ++i)
            {
                var maxJ = ((i % 2) != 0) ? 8 : 9;
                for (var j = 0; j < maxJ; j++)
                {
                    var color = new Vector3(1.0f, 1.0f, 1.0f);
                    if (mapChunk.MCCV != null)
                    {
                        color.X = mapChunk.MCCV.Entries[idx].Red / 127.0f;
                        color.Y = mapChunk.MCCV.Entries[idx].Green / 127.0f;
                        color.Z = mapChunk.MCCV.Entries[idx].Blue / 127.0f;
                    }

                    var vertex = new Vertex
                    {
                        Color = color,
                        Position = new Vector3
                        {
                            X = mapChunk.MCNK.Position.X - (i * Constants.UnitSize * 0.5f),
                            Y = mapChunk.MCNK.Position.Y - (j * Constants.UnitSize) - (((i % 2) != 0) ? (0.5f * Constants.UnitSize) : 0.0f),
                            Z = mapChunk.MCVT.Heights[idx] + mapChunk.MCNK.Position.Z
                        },
                        TextureCoordinates = new Vector2
                        {
                            X = i * 0.5f / 8.0f,
                            Y = (j + (((i % 2) != 0) ? 0.5f : 0.0f)) / 8.0f
                        }
                    };

                    vertex.AlphaCoordinates = new Vector2
                    {
                        X = vertex.TextureCoordinates.X, 
                        Y = vertex.TextureCoordinates.Y
                    };

                    _vertices.Add(vertex);

                    ++idx;
                }
            }
        }

        private void GenerateIndices(MapChunk mapChunk)
        {
            var offset = VerticeCount;
            var unitidx = 0;
            for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
            {
                if (!mapChunk.HasHole(unitidx % 8, unitidx++ / 8))
                {
                    _indices.AddRange(new[] {
                        (ushort)(j + offset), (ushort)(j - 9 + offset), (ushort)(j + 8 + offset),
                        (ushort)(j + offset), (ushort)(j - 8 + offset), (ushort)(j - 9 + offset),
                        (ushort)(j + offset), (ushort)(j + 9 + offset), (ushort)(j - 8 + offset),
                        (ushort)(j + offset), (ushort)(j + 8 + offset), (ushort)(j + 9 + offset)
                    });
                }
                if ((j + 1) % (9 + 8) == 0) j += 9;
            }
        }

        public void Bind(Shader shader)
        {
            VerticeVBO = GL.GenBuffer();
            IndicesVBO = GL.GenBuffer();
            VAO = GL.GenVertexArray();

            GL.BindVertexArray(VAO);

            var vertexSize = Marshal.SizeOf(typeof(Vertex));
            var verticeSize = _vertices.Count * vertexSize;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VerticeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticeSize), _vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(shader.GetAttribLocation("vertex_shading"), 3, VertexAttribPointerType.Float, false,
                vertexSize, IntPtr.Zero);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("vertex_shading"));

            GL.VertexAttribPointer(shader.GetAttribLocation("position"), 3, VertexAttribPointerType.Float, false,
                vertexSize, sizeof(float) * 3);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("position"));

            GL.VertexAttribPointer(shader.GetAttribLocation("texture_coordinates"), 2, VertexAttribPointerType.Float, true,
                vertexSize, (IntPtr)(sizeof(float) * 6));
            GL.EnableVertexAttribArray(shader.GetAttribLocation("texture_coordinates"));

            GL.VertexAttribPointer(shader.GetAttribLocation("alpha_coordinates"), 2, VertexAttribPointerType.Float, true,
                vertexSize, (IntPtr)(sizeof(float) * 8));
            GL.EnableVertexAttribArray(shader.GetAttribLocation("alpha_coordinates"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_indices.Count * sizeof(ushort)),
                _indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Not needed anymore
            _vertices.Clear();
            _indices.Clear();
        }

        public void Delete()
        {
            Renderers.Clear();
        }

        public void Render(Shader shader)
        {
            // TODO: Don't bother calling glDrawElements on ranges when there is only one
            // texture used on the whole tile. Rather acquire the texture, load it,
            // and render all the terrain at once.
            
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);

            foreach (var renderer in Renderers)
                renderer.Render(shader);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
