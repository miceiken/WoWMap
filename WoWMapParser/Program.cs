#define REDIRECT_OUTPUT

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
            //Console.ReadKey();
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
            var adt = new ADT("Kalimdor", 32, 36);
            var sw = Stopwatch.StartNew();
            adt.Read();
            sw.Stop();

            adt.SaveObj();

            Console.WriteLine("Loaded {0} chunks from '{1}' in {2}ms", adt.Data.Chunks.Count, "err", sw.ElapsedMilliseconds);
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
            const string path = @"World\WMO\Northrend\Battleground\ND_BG_Keep01.wmo";
            var wmo = new WMORoot(path);
        }
    }
}
