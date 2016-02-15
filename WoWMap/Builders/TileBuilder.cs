using System;
using System.Collections.Generic;
using System.Linq;
using WoWMap.Layers;
using WoWMap.Chunks;
using OpenTK;
using System.Globalization;
using System.IO;
using SharpNav;
using SharpNav.Geometry;


namespace WoWMap.Builders
{
    public class TileBuilder
    {
        public TileBuilder(string world, int x, int y)
        {
            s_Tile = new ADT(world, x, y);
        }

        private ADT s_Tile;

        public NavMeshGenerationSettings NavmeshSettings { get; set; } = WoWSettings;

        #region WoWSettings

        public static NavMeshGenerationSettings WoWSettings
        {
            get
            {
                var cfg = NavMeshGenerationSettings.Default;

                cfg.CellSize = Constants.TileSize / 1800;
                cfg.CellHeight = 0.3f;
                cfg.MinRegionSize = (int)Math.Pow(6, 2);
                cfg.MergedRegionSize = (int)Math.Pow(12, 2);
                cfg.MaxClimb = 1f;
                cfg.AgentHeight = 2.1f;
                cfg.AgentRadius = 0.6f;
                cfg.MaxEdgeLength = (int)(cfg.AgentRadius / cfg.CellSize) * 8;
                cfg.MaxEdgeError = 1.3f;
                cfg.VertsPerPoly = 6;
                cfg.SampleDistance = 3;
                cfg.MaxSampleError = 1; // 1.25f?

                return cfg;
            }
        }

        #endregion

        public string World { get { return s_Tile.World; } }
        public int X { get { return s_Tile.X; } }
        public int Y { get { return s_Tile.Y; } }

        private Geometry.Geometry s_Geometry;
        public BBox3 Bounds { get; private set; }

        public void LoadTileData()
        {
            s_Geometry = new Geometry.Geometry();
            // First read target tile to verify we have geometry and get bounding box
            // Read tile
            s_Tile.Read();
            // Generate relevant geometry
            s_Tile.Generate();
            // Append to our geometry
            s_Geometry.AddADT(s_Tile);

            // Generate the bounding box for the tile
            // (Because we need to cut the surrounding tiles later)
            Bounds = Geometry.Geometry.GetBoundingBox(X, Y, s_Geometry.Vertices);

            // We need to load the surrounding tiles because there sometimes is overlap in geometry to and from different tiles
            for (var y = Y - 1; y <= Y + 1; y++)
            {
                for (var x = X - 1; y <= X + 1; y++)
                {
                    // Skip target tile as we already added it
                    if (X == x && Y == y) continue;

                    // Add a surrounding tile
                    var tile = new ADT(World, x, y);
                    tile.Read();
                    tile.Generate();
                    s_Geometry.AddADT(tile);
                }
            }
        }

        public NavMeshBuilder Build()
        {
            // Generate tile data
            LoadTileData();

            // Extract raw data from geometry
            float[] vertices;
            int[] indices;

            s_Geometry.GetRawData(out vertices, out indices/*, out areas*/);

            var hf = new Heightfield(Bounds, NavmeshSettings);
            hf.RasterizeTriangles(vertices, Area.Default);
            hf.FilterLedgeSpans(NavmeshSettings.VoxelAgentHeight, NavmeshSettings.VoxelMaxClimb);
            hf.FilterLowHangingWalkableObstacles(NavmeshSettings.VoxelMaxClimb);
            hf.FilterWalkableLowHeightSpans(NavmeshSettings.VoxelAgentHeight);

            var chf = new CompactHeightfield(hf, NavmeshSettings);
            chf.Erode(NavmeshSettings.VoxelAgentRadius);
            chf.BuildDistanceField();
            chf.BuildRegions((int)(NavmeshSettings.AgentRadius / NavmeshSettings.CellSize) + 8, NavmeshSettings.MinRegionSize, NavmeshSettings.MergedRegionSize);

            var cset = chf.BuildContourSet(NavmeshSettings);
            var pmesh = new PolyMesh(cset, NavmeshSettings);
            var dmesh = new PolyMeshDetail(pmesh, chf, NavmeshSettings);

            var buildData = new NavMeshBuilder(pmesh, dmesh, new SharpNav.Pathfinding.OffMeshConnection[0], NavmeshSettings);

            Console.WriteLine("Rasterized " + vertices.Length / 9 + " triangles.");
            Console.WriteLine("Generated " + cset.Count + " regions.");
            Console.WriteLine("PolyMesh contains " + pmesh.VertCount + " vertices in " + pmesh.PolyCount + " polys.");
            Console.WriteLine("PolyMeshDetail contains " + dmesh.VertCount + " vertices and " + dmesh.TrisCount + " tris in " + dmesh.MeshCount + " meshes.");

            return buildData;
        }
    }
}
