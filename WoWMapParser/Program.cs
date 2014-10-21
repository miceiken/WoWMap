using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap;
using WoWMap.Archive;
using System.Diagnostics;

namespace WoWMapParser
{
    class Program
    {
        static void Main(string[] args)
        {
            CASC.Initialize(@"D:\Games\World of Warcraft\");
            // ReadADT();
            ReadWDT();

            Console.ReadKey();
        }

        static void ReadADT()
        {
            const string path = @"PVPZone01_30_29.adt";
            var adt = new ADT(path);
            var sw = Stopwatch.StartNew();
            adt.Read();
            sw.Stop();

            //Console.WriteLine("Loaded {0} chunks from '{1}' in {2}ms", adt.Chunks.Count, System.IO.Path.GetFileName(path), sw.ElapsedMilliseconds);
        }

        static void ReadWDT()
        {
            const string path = @"PVPZone01.wdt";
            var wdt = new WDT(path);
            var sw = Stopwatch.StartNew();
            wdt.Read();
            sw.Stop();
        }
    }
}
