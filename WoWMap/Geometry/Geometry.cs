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

namespace WoWMap.Geometry
{
    public enum PolyArea : byte
    {
        Terrain = 1,
        Water = 2,
        Road = 3,
        Danger = 4,
    };

    [Flags]
    public enum PolyFlag : byte
    {
        Walk = 1,
        Swim = 2,
        FlightMaster = 4,
    };

    public class Geometry
    {
        public List<Vector3> Vertices { get; private set; }
        public List<uint> Indices { get; private set; }

        public Geometry()
        {
            Vertices = new List<Vector3>(10000);
            Indices = new List<uint>(10000);
        }

        public void SaveWavefrontObject(string filename)
        {
            using (var sw = new StreamWriter(filename, false))
            {
                sw.WriteLine("o " + filename);
                var nf = CultureInfo.InvariantCulture.NumberFormat;
                foreach (var v in Vertices)
                    sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Y.ToString(nf) + " " + v.Z.ToString(nf));
                for (int i = 0; i < Indices.Count; i += 3)
                    sw.WriteLine("f " + (Indices[i] + 1) + " " + (Indices[i + 1] + 1) + " " + (Indices[i + 2] + 1));
            }
        }

        public void AddGeometry(IEnumerable<Vector3> vertices, IEnumerable<uint> indices)
        {
            var vo = (uint)Vertices.Count;
            Vertices.AddRange(vertices);
            Indices.AddRange(indices);
        }

        public void AddMesh(Mesh mesh)
        {
            AddGeometry(mesh.Vertices, mesh.Indices);
        }

        public void AddWMOScene(WMOScene wmosecene)
        {
            foreach (var mesh in wmosecene.Terrain)
                AddGeometry(mesh.Vertices, mesh.Indices);
            foreach (var mesh in wmosecene.Doodads)
                AddGeometry(mesh.Vertices, mesh.Indices);
            foreach (var mesh in wmosecene.Liquids)
                AddGeometry(mesh.Vertices, mesh.Indices);
        }

        public void AddScene(Scene scene)
        {
            foreach (var mesh in scene.Terrain)
                AddMesh(mesh);
            foreach (var wmoscene in scene.WorldModelObjects)
                AddWMOScene(wmoscene);
            foreach (var mesh in scene.Doodads)
                AddMesh(mesh);
            foreach (var mesh in scene.Liquids)
                AddMesh(mesh);
        }

        public void AddADT(ADT s)
        {
            foreach (var mc in s.MapChunks)
            {
                var scene = mc.Scene;
                foreach (var mesh in scene.Terrain)
                    AddGeometry(mesh.Vertices, mesh.Indices);
                foreach (var wmosecene in scene.WorldModelObjects)
                {
                    foreach (var mesh in wmosecene.Terrain)
                        AddGeometry(mesh.Vertices, mesh.Indices);
                    foreach (var mesh in wmosecene.Doodads)
                        AddGeometry(mesh.Vertices, mesh.Indices);
                    foreach (var mesh in wmosecene.Liquids)
                        AddGeometry(mesh.Vertices, mesh.Indices);
                }
                foreach (var mesh in scene.Doodads)
                    AddGeometry(mesh.Vertices, mesh.Indices);
                foreach (var mesh in scene.Liquids)
                    AddGeometry(mesh.Vertices, mesh.Indices);
            }
        }

        public void AddWDTGlobalmodel(WDT s)
        {
            if (s.ModelScene != null)
                AddWMOScene(s.ModelScene);
        }

        /*
         * This part is for navmesh generation
         * THIS IS ALSO WHY WE TAMPER WITH COORDINATE SYSTEMS
         * IF IT WORKS DON'T BREAK
         */
        public void GetRawData(out float[] vertices, out int[] indices)
        {
            vertices = new float[Vertices.Count * 3];
            for (int i = 0; i < Vertices.Count; i++)
            {
                var vert = Vertices[i];
                vertices[(i * 3) + 0] = vert.X;
                vertices[(i * 3) + 1] = vert.Z;
                vertices[(i * 3) + 2] = vert.Y;
            }
            indices = Indices.Select(i => (int)i).ToArray();
            //areas = new AreaId[Indices.Count];
            //for (int i = 0; i < Indices.Count; i++)
            //{
            //    switch (Indices[i].Type)
            //    {
            //        case TriangleType.Water:
            //            areas[i] = (AreaId)PolyArea.Water;
            //            break;

            //        default:
            //            areas[i] = AreaId.Walkable; //(AreaId)PolyArea.Terrain;
            //            break;
            //    }
            //}
        }

        public static BBox3 GetBoundingBox(int x, int y, IEnumerable<Vector3> vertices)
        {
            var bBoxMin = new Vector3((32 - (x + 1)) * Constants.TileSize, vertices.Select(v => v.Z).Min(), (32 - (y + 1)) * Constants.TileSize);
            //bBoxMin.X -= ((int)(WoWSettings.AgentWidth / WoWSettings.CellSize) + 8) * WoWSettings.CellSize;
            //bBoxMin.Z -= ((int)(WoWSettings.AgentWidth / WoWSettings.CellSize) + 8) * WoWSettings.CellSize;

            var bBoxMax = new Vector3((32 - x) * Constants.TileSize, vertices.Select(v => v.Z).Max(), (32 - y) * Constants.TileSize);
            //bBoxMax.X += ((int)(WoWSettings.AgentWidth / WoWSettings.CellSize) + 8) * WoWSettings.CellSize;
            //bBoxMax.Z += ((int)(WoWSettings.AgentWidth / WoWSettings.CellSize) + 8) * WoWSettings.CellSize;

            return new BBox3(bBoxMin, bBoxMax);
        }
    }
}
