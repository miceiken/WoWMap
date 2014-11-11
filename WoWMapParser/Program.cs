//#define REDIRECT_OUTPUT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using WoWMap;
using WoWMap.Archive;
using WoWMap.Layers;
using WoWMap.Geometry;

namespace WoWMapParser
{
    class Program
    {
        static void Main(string[] args)
        {
            Initialize();

            //ReadMapsDBC();
            ReadADT();
            //ReadADTs();
            //CreateNavmesh();
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
            CASC.Initialize(@"D:\Games\World of Warcraft\");
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
            var adt = new ADT("Azeroth", 28, 28);
            //var adt = new ADT("Kalimdor", 32, 36);
            adt.Read();

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
                    sw.Reset();
                    Console.WriteLine(" (done! {0}ms)", sw.ElapsedMilliseconds);
                    allGeom.AddADT(adt);
                }
            }

            allGeom.SaveWavefrontObject(continent + ".obj");
        }

        static void CreateNavmesh()
        {
            var geom = new Geometry();
            var sw = Stopwatch.StartNew();
            var adt = new ADT("Azeroth", 28, 28);
            adt.Read();
            sw.Stop();
            Console.WriteLine("Read ADT in {0}", sw.Elapsed);
            geom.AddADT(adt);
            sw.Restart();
            var bbox = Geometry.GetBoundingBox(28, 28, geom.Vertices);
            var build = geom.GenerateNavmesh(bbox);
            sw.Stop();
            Console.WriteLine("Generated navmesh in {0}", sw.Elapsed);

            TestNavmesh(new SharpNav.TiledNavMesh(build));
        }

        static void TestNavmesh(SharpNav.TiledNavMesh tmesh)
        {
            // Azeroth 28 28 / Deathknell (wow-style coordinates)
            // Outside church: 1843,734 1604,214 94,55994
            // Inside church: 1844,074 1642,581 97,62832
            // Outside spawn: 1672,226 1662,989 139,2343
            // Inside spawn: 1665,264 1678,277 120,5302
            // Outside cave: 2051,3 1807,121 102,5225
            // Inside cave: 2082,813 1950,718 98,04765
            // Outside house: 1861,465 1582,03 92,79533
            // Upstairs house: 1859,929 1560,804 99,07755

            var query = new SharpNav.NavMeshQuery(tmesh, 65535);

            var extents = new SharpNav.Vector3(2.5f, 2.5f, 2.5f);

            // WoW(X, Y, Z) -> SharpNav(Y, Z, X) -- or so I think :-----D
            var posStart = new SharpNav.Vector3(1662.9f, 139.2f, 1672.2f); // Outside spawn
            var posEnd = new SharpNav.Vector3(1678.3f, 120.5f, 1665.3f); // Inside spawn

            SharpNav.Vector3 aStartPos;
            int snRef;
            if (!query.FindNearestPoly(ref posStart, ref extents, out snRef, out aStartPos))
                Console.WriteLine("No start poly");

            //SharpNav.Vector3 rPos;
            //int rRef;
            //if (!query.FindRandomPoint(out rRef, out rPos))
            //    Console.WriteLine("No end poly");

            SharpNav.Vector3 aEndPos;
            int enRef;
            if (!query.FindNearestPoly(ref posEnd, ref extents, out enRef, out aEndPos))
                Console.WriteLine("No end poly");

            var path = new List<int>();
            if (!query.FindPath(snRef, enRef, ref aStartPos, ref aEndPos, path))
                Console.WriteLine("No path");

            //if (!query.FindStraightPath(posStart, posEnd, path.ToArray(), path.Count))
            //    return;
        }

        static void ReadWDT()
        {
            const string path = @"PVPZone01.wdt";
            var wdt = new WDT(path);
            var sw = Stopwatch.StartNew();
            wdt.Read();
            sw.Stop();

            Console.WriteLine("Loaded {0} chunks from '{1}' in {2}ms", wdt.Data.Chunks.Count, System.IO.Path.GetFileName(path), sw.ElapsedMilliseconds);
        }

        static void ReadWMO()
        {
            const string path = @"World\wmo\Northrend\HowlingFjord\DaggercapCave.wmo";
            var wmo = new WMORoot(path);
            using (var sw = new StreamWriter(Path.GetFileNameWithoutExtension(path) + ".obj", false))
            {
                //sw.WriteLine("o " + filename);
                var nf = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
                int vo = 0;
                foreach (var g in wmo.Groups)
                {
                    foreach (var v in g.MOVT.Vertices)
                        sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Z.ToString(nf) + " " + v.Y.ToString(nf));
                    foreach (var t in g.MOVI.Indices)
                        sw.WriteLine("f " + (t.V0 + vo + 1) + " " + (t.V1 + vo + 1) + " " + (t.V2 + vo + 1));
                    vo += g.MOVT.Vertices.Count();

                    if (g.LiquidVertices == null || g.LiquidIndices == null)
                        continue;

                    foreach (var v in g.LiquidVertices)
                        sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Z.ToString(nf) + " " + v.Y.ToString(nf));
                    foreach (var t in g.LiquidIndices)
                        sw.WriteLine("f " + (t.V0 + vo + 1) + " " + (t.V1 + vo + 1) + " " + (t.V2 + vo + 1));
                    vo += g.LiquidVertices.Count;
                }
            }
        }
    }
}
