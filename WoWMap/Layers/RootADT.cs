using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;

namespace WoWMap.Layers
{
    public sealed class RootADT
    {
        public string Filename { get; private set; }
        public WDT WDT { get; private set; }

        public ChunkData Data { get; private set; }

        public RootADT(string filename, WDT wdt)
        {
            WDT = wdt;
            Filename = filename;

            Data = new ChunkData(filename + ".adt");
        }

        public void Read(ADT combinedFile)
        {
            foreach (var subChunk in Data.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MVER":
                        combinedFile.MVER = new MVER(subChunk);
                        break;
                    case "MAMP":
                        combinedFile.MAMP = new MAMP(subChunk);
                        break;
                    // case "MFBO":
                    //     combinedFile.MFBO = new MFBO(subChunk);
                    //     break;
                    case "MHDR":
                        combinedFile.MHDR = new MHDR(subChunk);
                        break;
                    case "MH2O":
                        combinedFile.Liquid = new LiquidChunk(combinedFile, subChunk);
                        break;
                    case "MCNK":
                        combinedFile.AddMapChunk(subChunk);
                        break;
                    default:
                        Console.WriteLine($"Unhandled {subChunk.Name} chunk in model ADT.");
                        break;
                }
            }
        }
    }
}
