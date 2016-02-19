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
        private List<uint> _indices = new List<uint>();

        public int VerticeCount { get { return _vertices.Count; } }
        public int IndiceCount { get; private set; }


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
                GenerateWMO(mapChunk);
                GenerateM2(mapChunk);
                GenerateLiquid(mapChunk);
            }

            IndiceCount = _indices.Count;
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
            var offset = (uint)VerticeCount;
            var unitidx = 0;

            for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
            {
                if (!mapChunk.HasHole(unitidx % 8, unitidx++ / 8))
                {
                    _indices.AddRange(new[] {
                        (j + offset), (j - 9 + offset), (j + 8 + offset),
                        (j + offset), (j - 8 + offset), (j - 9 + offset),
                        (j + offset), (j + 9 + offset), (j - 8 + offset),
                        (j + offset), (j + 8 + offset), (j + 9 + offset)
                    });
                }
                if ((j + 1) % (9 + 8) == 0) j += 9;
            }
        }

        private void GenerateWMO(MapChunk mapChunk)
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
                var vo = (uint)VerticeCount;
                _vertices.AddRange(vertices.Select(v => new Vertex() { Position = v, Type = 1 }));
                _indices.AddRange(indices.SelectMany(t => new[] { (vo + t.V0), (vo + t.V1), (vo + t.V2) }));
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
                var vo = (uint)VerticeCount;
                _vertices.AddRange(model.Vertices.Select(v => new Vertex() { Position = Vector3.Transform(v, transform), Type = 2 }));
                _indices.AddRange(model.Indices.SelectMany(t => new[] { (vo + t.V0), (vo + t.V1), (vo + t.V2) }));
            }
        }

        private void GenerateLiquid(MapChunk mapChunk)
        {
            if (mapChunk.ADT.Liquid.HeightMaps[mapChunk.Index] == null)
                return;

            var vertices = new List<Vector3>();
            var indices = new List<Triangle<uint>>();
            mapChunk.GenerateLiquid(ref vertices, ref indices);

            var vo = (uint)VerticeCount;
            _vertices.AddRange(vertices.Select(v => new Vertex() { Position = v, Type = 3 }));
            _indices.AddRange(indices.SelectMany(t => new[] { (vo + t.V0), (vo + t.V1), (vo + t.V2) }));
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
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(IndiceCount * sizeof(uint)),
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

            GL.DrawElements(PrimitiveType.Triangles, IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
