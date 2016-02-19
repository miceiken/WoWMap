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
    public enum VertexType : int
    {
        Terrain = 0,
        WMO,
        M2,
        Liquid
    };

    public class TileRenderer
    {
        public class TileRenderData
        {
            public int VAO { get; set; }
            public int VerticeVBO { get; set; }
            public int IndicesVBO { get; set; }

            public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
            public List<uint> Indices { get; private set; } = new List<uint>();

            public int VerticeCount { get { return Vertices.Count; } }
            public int IndiceCount { get; set; }
        }

        private Dictionary<VertexType, TileRenderData> _renderData = new Dictionary<VertexType, TileRenderData>();

        public TerrainRenderer BaseRenderer { get; private set; }

        public TileRenderer(TerrainRenderer baseRenderer)
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

                GenerateTerrainIndices(mapChunk, _renderData[VertexType.Terrain] = new TileRenderData());
                GenerateTerrainVertices(mapChunk, _renderData[VertexType.Terrain]);
                GenerateWMO(mapChunk, _renderData[VertexType.WMO] = new TileRenderData());
                GenerateM2(mapChunk, _renderData[VertexType.M2] = new TileRenderData());
                GenerateLiquid(mapChunk, _renderData[VertexType.Liquid] = new TileRenderData());
            }
        }

        private void GenerateTerrainVertices(MapChunk mapChunk, TileRenderData renderData)
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

                    renderData.Vertices.Add(vertex);

                    ++idx;
                }
            }
        }

        private void GenerateTerrainIndices(MapChunk mapChunk, TileRenderData renderData)
        {
            var offset = (uint)renderData.VerticeCount;
            var unitidx = 0;

            for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
            {
                if (!mapChunk.HasHole(unitidx % 8, unitidx++ / 8))
                {
                    renderData.Indices.AddRange(new[] {
                        (j + offset), (j - 9 + offset), (j + 8 + offset),
                        (j + offset), (j - 8 + offset), (j - 9 + offset),
                        (j + offset), (j + 9 + offset), (j - 8 + offset),
                        (j + offset), (j + 8 + offset), (j + 9 + offset)
                    });
                }
                if ((j + 1) % (9 + 8) == 0) j += 9;
            }
        }

        private void GenerateWMO(MapChunk mapChunk, TileRenderData renderData)
        {
            if (mapChunk.MCRW == null || mapChunk.ADT.MODF == null)
                return;

            var drawn = new HashSet<uint>();
            for (var i = 0; i < mapChunk.MCRW.MODFEntryIndex.Length; i++)
            {
                var wmo = mapChunk.ADT.MODF.Entries[mapChunk.MCRW.MODFEntryIndex[i]];
                if (drawn.Contains(wmo.UniqueId))
                    continue;
                drawn.Add(wmo.UniqueId);

                if (wmo.MWIDEntryIndex >= mapChunk.ADT.ModelPaths.Count)
                    continue;

                var path = mapChunk.ADT.ModelPaths[(int)wmo.MWIDEntryIndex];
                var model = new WMORoot(path);

                var vertices = new List<Vector3>(1000);
                var indices = new List<Triangle<uint>>(1000);
                var normals = new List<Vector3>(1000);

                MapChunk.InsertWMOGeometry(wmo, model, ref vertices, ref indices, ref normals);
                var vo = (uint)renderData.VerticeCount;
                renderData.Vertices.AddRange(vertices.Select(v => new Vertex() { Position = v, Type = 1 }));
                renderData.Indices.AddRange(indices.SelectMany(t => new[] { (vo + t.V0), (vo + t.V1), (vo + t.V2) }));
            }
        }

        private void GenerateM2(MapChunk mapChunk, TileRenderData renderData)
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
                var vo = (uint)renderData.VerticeCount;
                renderData.Vertices.AddRange(model.Vertices.Select(v => new Vertex() { Position = Vector3.Transform(v, transform), Type = 2 }));
                renderData.Indices.AddRange(model.Indices.SelectMany(t => new[] { (vo + t.V0), (vo + t.V1), (vo + t.V2) }));
            }
        }

        private void GenerateLiquid(MapChunk mapChunk, TileRenderData renderData)
        {
            if (mapChunk.ADT?.Liquid?.HeightMaps[mapChunk.Index] == null)
                return;

            var vertices = new List<Vector3>();
            var indices = new List<Triangle<uint>>();
            mapChunk.GenerateLiquid(ref vertices, ref indices);

            var vo = (uint)renderData.VerticeCount;
            renderData.Vertices.AddRange(vertices.Select(v => new Vertex() { Position = v, Type = 3 }));
            renderData.Indices.AddRange(indices.SelectMany(t => new[] { (vo + t.V0), (vo + t.V1), (vo + t.V2) }));
        }

        public void Bind(Shader shader)
        {
            foreach (var kvp in _renderData)
            {
                var renderData = kvp.Value;

                renderData.VerticeVBO = GL.GenBuffer();
                renderData.IndicesVBO = GL.GenBuffer();
                renderData.VAO = GL.GenVertexArray();

                renderData.IndiceCount = renderData.Indices.Count;

                GL.BindVertexArray(renderData.VAO);

                var vertexSize = Marshal.SizeOf(typeof(Vertex));
                var verticeSize = renderData.Vertices.Count * vertexSize;

                GL.BindBuffer(BufferTarget.ArrayBuffer, renderData.VerticeVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticeSize), renderData.Vertices.ToArray(), BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(shader.GetAttribLocation("position"), 3, VertexAttribPointerType.Float, false,
                    vertexSize, sizeof(int));
                GL.EnableVertexAttribArray(shader.GetAttribLocation("position"));

                GL.VertexAttribIPointer(shader.GetAttribLocation("type"), 1, VertexAttribIntegerType.Int,
                    vertexSize, IntPtr.Zero);
                GL.EnableVertexAttribArray(shader.GetAttribLocation("type"));

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderData.IndicesVBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(renderData.IndiceCount * sizeof(uint)),
                    renderData.Indices.ToArray(), BufferUsageHint.StaticDraw);

                // Not needed anymore
                renderData.Vertices.Clear();
                renderData.Indices.Clear();
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        { }

        public void Render(Shader shader, int[] terrainSamplers, int[] alphaMapSamplers)
        {
            foreach (var kvp in _renderData)
            {
                if (!BaseRenderer._drawType[kvp.Key]) continue;
                var renderData = kvp.Value;

                GL.BindVertexArray(renderData.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderData.IndicesVBO);

                GL.DrawElements(PrimitiveType.Triangles, renderData.IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
