//#define REDIRECT_OUTPUT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTK;
using SharpNav;
using SharpNav.Pathfinding;
using WoWMap.Archive;
using WoWMap.Layers;
using WoWMap.Geometry;
using WoWMap.Builders;

namespace WoWMapParser
{
    class Program
    {
        private static Func<Vector3, Vector3> WoWToSharpNav = v => new Vector3(v.X, v.Z, v.Y);
        private static Func<Vector3, Vector3> SharpNavToWoW = v => new Vector3(v.X, v.Z, v.Y);

        static void Main(string[] args)
        {
            Initialize();

            //ReadMapsDBC();
            //ReadADT();
            //ReadADTs();
            CreateNavmesh();
            //TestNavmesh();
            //ReadWDT();
            //ReadWMO();

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        static void Initialize()
        {
#if REDIRECT_OUTPUT
            Console.SetOut(new StreamWriter("log.txt", false) { AutoFlush = true });
#endif

            Console.WriteLine("Initializing CASC - this may take a while...");

            var sw = Stopwatch.StartNew();
            CASC.Initialize(@"D:\Games\World of Warcraft");
            sw.Stop();

            Console.WriteLine("CASC initialized in {0}ms", sw.ElapsedMilliseconds);
        }

        static void ReadMapsDBC()
        {
            const string path = @"DBFilesClient\Map.dbc";
            var sw = Stopwatch.StartNew();
            var dbc = new DBC<MapRecord>(path);
            sw.Stop();

            Console.WriteLine("Loaded {0} records from '{1}' in {2}ms", dbc.Rows.Count(), System.IO.Path.GetFileName(path), sw.ElapsedMilliseconds);

            foreach (var row in dbc.Rows)
            {
                if (row == null) continue;
                Console.WriteLine("{0} - '{1}' - '{2}'", row.ID, row.MapNameLang, row.Directory);
            }
        }

        static void ReadADT()
        {
            // var adt = new ADT("Kalimdor", 32, 36);
            //var adt = new ADT("PvPZone01", 32, 30);
            var adt = new ADT("Azeroth", 28, 28);
            // var adt = new ADT("Azeroth", 31, 40);
            adt.Read();
            adt.Generate();
            var geom = new Geometry();
            geom.AddADT(adt);
            geom.SaveWavefrontObject(Path.GetFileNameWithoutExtension(adt.Filename) + ".obj");
        }

        static void ReadADTs()
        {
            const string continent = "Azeroth";
            var allGeom = new Geometry();

            var sw = new Stopwatch();
            for (int y = 27; y < 29; y++)
            {
                for (int x = 28; x < 30; x++)
                {
                    Console.Write("Parsing {0} [{1}, {2}]", continent, x, y);
                    sw.Start();
                    var adt = new ADT(continent, x, y);
                    adt.Read();
                    adt.Generate();
                    sw.Reset();
                    Console.WriteLine(" (done! {0}ms)", sw.ElapsedMilliseconds);
                    allGeom.AddADT(adt);
                }
            }

            allGeom.SaveWavefrontObject(continent + ".obj");
        }

        static void CreateNavmesh()
        {
            var sw = Stopwatch.StartNew();
            var build = new TileBuilder("Azeroth", 28, 28).Build();
            Console.WriteLine("Generated navmesh in {0}", sw.Elapsed);

            TestNavmesh(build);
        }

        static void TestNavmesh(NavMeshBuilder build)
        {
            // Azeroth 28 28 / Deathknell (wow-style coordinates)
            // Outside church: 1843.734 1604.214 94.55994
            // Inside church: 1844.074 1642.581 97.62832
            // Outside spawn: 1672.226 1662.989 139.2343
            // Inside spawn: 1665.264 1678.277 120.5302
            // Outside cave: 2051.3 1807.121 102.5225
            // Inside cave: 2082.813 1950.718 98.04765
            // Outside house: 1861.465 1582.03 92.79533
            // Upstairs house: 1859.929 1560.804 99.07755

            var tmesh = new TiledNavMesh(build);

            var query = new NavMeshQuery(tmesh, 65536);
            var extents = new Vector3(5f, 5f, 5f);

            var posStart = WoWToSharpNav(new Vector3(1665.2f, 1678.2f, 120.5f)); // Inside spawn
            var posEnd = WoWToSharpNav(new Vector3(1672.2f, 1662.9f, 139.2f)); // Outside spawn

            NavPoint endPt;
            query.FindNearestPoly(ref posEnd, ref extents, out endPt);

            NavPoint startPt;
            query.FindNearestPoly(ref posStart, ref extents, out startPt);

            var path = new List<int>();
            if (!query.FindPath(ref startPt, ref endPt, path))
                Console.WriteLine("No path!");

            Vector3 actualStart = new Vector3();
            query.ClosestPointOnPoly(startPt.Polygon, startPt.Position, ref actualStart);

            Vector3 actualEnd = new Vector3();
            query.ClosestPointOnPoly(endPt.Polygon, endPt.Position, ref actualEnd);

            var smoothPath = new List<Vector3>();
            Vector3[] straightPath = new Vector3[2048];
            int[] pathFlags = new int[2048];
            int[] pathRefs = new int[2048];
            int pathCount = -1;
            query.FindStraightPath(actualStart, actualEnd, path.ToArray(), path.Count, straightPath, pathFlags, pathRefs, ref pathCount, 2048, PathBuildFlags.AllCrossingVertices);

            foreach (var v in straightPath)
                Console.WriteLine(v);
        }

        static void ReadWDT()
        {
            string path = string.Format(@"World\Maps\{0}\{0}.wdt", "WailingCaverns");
            var sw = Stopwatch.StartNew();
            var wdt = new WDT(path);
            Console.WriteLine("Loaded {0} chunks from '{1}' in {2}ms", wdt.Data.Chunks.Count, System.IO.Path.GetFileName(path), sw.ElapsedMilliseconds);
            wdt.GenerateGlobalModel();
            sw.Stop();
            var geom = new Geometry();
            geom.AddWDTGlobalmodel(wdt);
            geom.SaveWavefrontObject("WailingCaverns.obj");
        }

        //static void ReadWMO()
        //{
        //    const string path = @"World\wmo\Northrend\HowlingFjord\DaggercapCave.wmo";
        //    var wmo = new WMORoot(path);
        //    using (var sw = new StreamWriter(Path.GetFileNameWithoutExtension(path) + ".obj", false))
        //    {
        //        //sw.WriteLine("o " + filename);
        //        var nf = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        //        int vo = 0;
        //        foreach (var g in wmo.Groups)
        //        {
        //            foreach (var v in g.MOVT.Vertices)
        //                sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Z.ToString(nf) + " " + v.Y.ToString(nf));
        //            foreach (var t in g.MOVI.Indices)
        //                sw.WriteLine("f " + (t.V0 + vo + 1) + " " + (t.V1 + vo + 1) + " " + (t.V2 + vo + 1));
        //            vo += g.MOVT.Vertices.Count();

        //            if (g.LiquidVertices == null || g.LiquidIndices == null)
        //                continue;

        //            foreach (var v in g.LiquidVertices)
        //                sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Z.ToString(nf) + " " + v.Y.ToString(nf));
        //            foreach (var t in g.LiquidIndices)
        //                sw.WriteLine("f " + (t.V0 + vo + 1) + " " + (t.V1 + vo + 1) + " " + (t.V2 + vo + 1));
        //            vo += g.LiquidVertices.Count;
        //        }
        //    }
        //}
    }
}
