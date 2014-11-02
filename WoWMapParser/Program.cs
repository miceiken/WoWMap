//#define REDIRECT_OUTPUT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            //ReadDBC();
            ReadADT();
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

        static void ReadDBC()
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
            const string continent = "Azeroth";
            var allGeom = new Geometry();
            
            for (int y = 27; y < 29; y++)
            {
                for (int x = 28; x < 30; x++)
                {
                    //var geom = new Geometry();
                    Console.Write("Parsing {0} [{1}, {2}]", continent, x, y);
                    var sw = Stopwatch.StartNew();
                    var adt = new ADT(continent, x, y);
                    adt.Read();
                    sw.Stop();
                    Console.WriteLine(" (done! {0}ms)", sw.ElapsedMilliseconds);
                    //geom.AddADT(adt);
                    //geom.GenerateNavmesh(Geometry.GetBoundingBox(x, y, geom.Vertices));
                    allGeom.AddADT(adt);
                }
            }

            allGeom.SaveWavefrontObject("Azeroth.obj");
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
