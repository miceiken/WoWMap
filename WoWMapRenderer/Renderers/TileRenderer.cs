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
    class TileRenderer
    {
        public int VAO { get; private set; }
        public int VerticeVBO { get; private set; }
        public int IndicesVBO { get; private set; }

        private List<Vertex> _vertices = new List<Vertex>();
        private List<ushort> _indices = new List<ushort>();

        public int VerticeCount { get { return _vertices.Count; } }
        public ushort IndiceCount { get { return (ushort)_indices.Count; } }


        public TileRenderer()
        { }

        ~TileRenderer()
        { }

        public void Generate(ADT tile)
        {
            for (var i = 0; i < tile.MapChunks.Count; ++i)
            {
                var mapChunk = tile.MapChunks[i];
                if (mapChunk == null)
                    continue;

                GenerateTerrainIndices(mapChunk);
                GenerateTerrainVertices(mapChunk);
                // GenerateWMO(mapChunk); // Both Indices & Vertices
                GenerateM2(mapChunk);  // Both Indices & Vertices
            }
        }

        private void GenerateTerrainVertices(MapChunk mapChunk)
        {
            for (int i = 0, idx = 0; i < 17; ++i)
            {
                var maxJ = ((i % 2) != 0) ? 8 : 9;
                for (var j = 0; j < maxJ; j++)
                {

                    var vertex = new Vertex
                    {
                        Position = new Vector3
                        {
                            X = mapChunk.MCNK.Position.X - (i * Constants.UnitSize * 0.5f),
                            Y = mapChunk.MCNK.Position.Y - (j * Constants.UnitSize) - (((i % 2) != 0) ? (0.5f * Constants.UnitSize) : 0.0f),
                            Z = mapChunk.MCVT.Heights[idx] + mapChunk.MCNK.Position.Z
                        },
                        Type = 0
                    };

                    _vertices.Add(vertex);

                    ++idx;
                }
            }
        }

        private void GenerateTerrainIndices(MapChunk mapChunk)
        {
            var offset = (ushort)VerticeCount;
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

        private void GenerateM2(MapChunk mapChunk)
        {
            if (mapChunk.MCRD == null || mapChunk.ADT.MDDF == null)
                return;

            var drawn = new HashSet<uint>();
            for (var i = 0; i < mapChunk.MCRD.MDDFEntryIndex.Length; i++)
            {
                var doodad = mapChunk.ADT.MDDF.Entries[mapChunk.MCRD.MDDFEntryIndex[i]];
                if (drawn.Contains(doodad.UniqueId))
                    continue;
                drawn.Add(doodad.UniqueId);

                if (doodad.MMIDEntryIndex >= mapChunk.ADT.DoodadPaths.Count)
                    continue;

                var path = mapChunk.ADT.DoodadPaths[(int)doodad.MMIDEntryIndex];
                var model = new M2(path);

                if (!model.IsCollidable)
                    continue;

                // Doodads outside WMOs are treated like WMOs. Not a typo.
                var transform = Transformation.GetWMOTransform(doodad.Position, doodad.Rotation, doodad.Scale / 1024.0f);
                var vo = (ushort)VerticeCount;
                _vertices.AddRange(model.Vertices.Select(v => new Vertex() { Position = Vector3.Transform(v, transform), Type = 2 }));
                _indices.AddRange(model.Indices.SelectMany(t => new[] { (ushort)(vo + t.V0), (ushort)(vo + t.V1), (ushort)(vo + t.V2) }));
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

            GL.VertexAttribPointer(shader.GetAttribLocation("position"), 3, VertexAttribPointerType.Float, false,
                vertexSize, sizeof(int));
            GL.EnableVertexAttribArray(shader.GetAttribLocation("position"));

            GL.VertexAttribIPointer(shader.GetAttribLocation("type"), 1, VertexAttribIntegerType.Int,
                vertexSize, IntPtr.Zero);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("type"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(IndiceCount * sizeof(ushort)),
                _indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Not needed anymore
            _vertices.Clear();
            _indices.Clear();
        }

        public void Delete()
        { }

        public void Render(Shader shader, int[] terrainSamplers, int[] alphaMapSamplers)
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);

            GL.DrawElements(PrimitiveType.Triangles, IndiceCount, DrawElementsType.UnsignedShort, IntPtr.Zero);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
