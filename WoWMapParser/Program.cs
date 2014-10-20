using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap;
using WoWMap.Chunks;
using System.Diagnostics;

namespace WoWMapParser
{
    class Program
    {
        static void Main(string[] args)
        {
            const string path = @"PVPZone01_30_29.adt";
            var adt = new ADT(path);
            var sw = Stopwatch.StartNew();
            adt.Read();
            sw.Stop();

            Console.WriteLine("Loaded {0} chunks from '{1}' in {2}ms", adt.Chunks.Count, System.IO.Path.GetFileName(path), sw.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
