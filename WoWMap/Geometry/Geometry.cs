using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;
using WoWMap.Chunks;
using Vector3 = SharpDX.Vector3;
using SharpDX;
using SharpNav;
using SharpNav.Geometry;

namespace WoWMap.Geometry
{
    public class Geometry
    {
        public List<Vector3> Vertices { get; private set; }
        public List<Triangle<uint>> Indices { get; private set; }

        public Geometry()
        {
            Vertices = new List<Vector3>(10000);
            Indices = new List<Triangle<uint>>(10000);
        }

        public void AddGeometry(IEnumerable<Vector3> vertices, IEnumerable<Triangle<uint>> indices)
        {
            var vo = (uint)this.Vertices.Count;
            foreach (var v in vertices)
                this.Vertices.Add(v);
            foreach (var i in indices)
                this.Indices.Add(new Triangle<uint>(i.Type, i.V0 + vo, i.V1 + vo, i.V2 + vo));
        }

        public void AddADT(ADT source)
        {
            foreach (var s in new ADT[] { source, source.ADTObjects })
            {
                foreach (var mc in s.MapChunks)
                {
                    if (mc.Vertices != null && mc.Vertices.Count() > 0 && mc.Indices != null && mc.Indices.Count > 0)
                        AddGeometry(mc.Vertices, mc.Indices);
                    if (mc.WMOVertices != null && mc.WMOVertices.Count > 0 && mc.WMOIndices != null && mc.WMOIndices.Count > 0)
                        AddGeometry(mc.WMOVertices, mc.WMOIndices);
                    if (mc.DoodadVertices != null && mc.DoodadVertices.Count > 0 && mc.DoodadIndices != null && mc.DoodadIndices.Count > 0)
                        AddGeometry(mc.DoodadVertices, mc.DoodadIndices);
                }
                if (s.Liquid != null && s.Liquid.Vertices != null && s.Liquid.Indices != null)
                    AddGeometry(source.Liquid.Vertices, source.Liquid.Indices);
            }
        }

        public void AddWDT(WDT source)
        { }

        public void AddDungeon(WMORoot model, MODF.MODFEntry def)
        {
            var verts = new List<Vector3>();
            var inds = new List<Triangle<uint>>();
            MapChunk.InsertWMOGeometry(def, model, verts, inds);
            AddGeometry(verts, inds);
        }

        public void GetRawData(out float[] vertices, out int[] indices, out byte[] areas)
        {
            vertices = new float[Vertices.Count * 3];
            for (int i = 0; i < Vertices.Count; i++)
            {
                var vert = Vertices[i];
                vertices[(i * 3) + 0] = vert.X;
                vertices[(i * 3) + 1] = vert.Y;
                vertices[(i * 3) + 2] = vert.Z;
            }
            indices = new int[Indices.Count * 3];
            for (int i = 0; i < Indices.Count; i++)
            {
                var tri = Indices[i];
                indices[(i * 3) + 0] = (int)tri.V0;
                indices[(i * 3) + 1] = (int)tri.V1;
                indices[(i * 3) + 2] = (int)tri.V2;
            }
            areas = new byte[Indices.Count];
            for (int i = 0; i < Indices.Count; i++)
            {
                switch (Indices[i].Type)
                {
                    /*case TriangleType.Water:
                        AreaId.
                        areas[i] = (byte)PolyArea.Water;
                        break;

                    default:
                        areas[i] = (byte)PolyArea.Terrain;
                        break;*/
                    default:
                        areas[i] = (byte)AreaId.Walkable;
                        break;
                }
            }
        }

        public void PrepareNavmesh()
        {
            float[] vertices;
            int[] indices;
            byte[] areas;
            GetRawData(out vertices, out indices, out areas);
            var settings = WoWSettings;
            var tris = TriangleEnumerable.FromIndexedFloat(vertices, indices, 0, 0, 0, indices.Length / 3);            
            
            var area = AreaIdGenerator.From(tris, AreaId.Walkable)
                //.MarkAboveHeight(areaSettings.MaxLevelHeight, AreaId.Null)
                //.MarkBelowHeight(areaSettings.MinLevelHeight, AreaId.Null)
                //.MarkBelowSlope(areaSettings.MaxTriSlope, AreaId.Null)
                .ToArray();

            var hf = new Heightfield(tris.GetBoundingBox(), settings);
            hf.RasterizeTrianglesWithAreas(tris.ToArray(), area);
            hf.FilterLedgeSpans(settings.VoxelAgentHeight, settings.VoxelMaxClimb);
            hf.FilterWalkableLowHeightSpans(settings.VoxelAgentHeight);

            var chf = new CompactHeightfield(hf, settings);
            chf.Erode(settings.VoxelAgentWidth);
            chf.BuildDistanceField();
            chf.BuildRegions(settings.VoxelAgentWidth + 8, settings.MinRegionSize, settings.MergedRegionSize);

            var cset = new ContourSet(chf, settings);
            var pmesh = new PolyMesh(cset, settings);
            var dmesh = new PolyMeshDetail(pmesh, chf, settings);

            var buildData = new NavMeshBuilder(pmesh, dmesh, new SharpNav.Pathfinding.OffMeshConnection[0], settings);
            var tiledNavMesh = new TiledNavMesh(buildData);
            
            var navMeshQuery = new NavMeshQuery(tiledNavMesh, 65536);
        }

        public static NavMeshGenerationSettings WoWSettings
        {
            get
            {
                var cfg = NavMeshGenerationSettings.Default;

                cfg.CellSize = Constants.TileSize / 1800;
                cfg.CellHeight = 0.3f;
                cfg.MinRegionSize = (int)Math.Pow(6, 2);
                cfg.MergedRegionSize = (int)Math.Pow(12, 2);
                cfg.MaxClimb = 50f;
                cfg.SampleDistance = 3;
                cfg.MaxSampleError = 1;
                cfg.AgentHeight = 2.1f;
                cfg.AgentWidth = 0.6f;
                cfg.VertsPerPoly = 6;
                cfg.MaxEdgeLength = (int)(cfg.AgentWidth * 8);
                cfg.MaxEdgeError = 1;

                return cfg;
            }
        }
    }
}
