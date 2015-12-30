using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;

namespace WoWMap.Layers
{
    public class ObjectsADT
    {
        private string Filename { get; set; }
        private WDT WDT { get; set; }

        private ChunkData Data { get; set; }

        public ObjectsADT(string fileName, WDT wdtFile)
        {
            Filename = fileName + "_obj0.adt";
            WDT = wdtFile;

            Data = new ChunkData(Filename);
        }

        public void Read(ADT combinedFile)
        {
            var mcnkIdx = 0;
            foreach (var subChunk in Data.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MMDX":
                        combinedFile.MMDX = new MMDX(subChunk);
                        break;
                    case "MMID":
                        combinedFile.MMID = new MMID(subChunk);
                        break;
                    case "MWMO":
                        combinedFile.MWMO = new MWMO(subChunk);
                        break;
                    case "MWID":
                        combinedFile.MWID = new MWID(subChunk);
                        break;
                    case "MDDF":
                        combinedFile.MDDF = new MDDF(subChunk);
                        break;
                    case "MODF":
                        combinedFile.MODF = new MODF(subChunk);
                        break;
                    case "MCNK":
                        combinedFile.UpdateMapChunk(subChunk, mcnkIdx++);
                        break;
                    default:
                        Console.WriteLine($"Unhandled {subChunk.Name} chunk in Objects ADT.");
                        break;
                }
            }
        }
    }
}
