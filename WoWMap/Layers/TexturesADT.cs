using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;

namespace WoWMap.Layers
{
    public class TexturesADT
    {
        public string Filename { get; private set; }
        public WDT WDT { get; private set; }

        private ChunkData Data { get; set; }

        public TexturesADT(string fileName, WDT wdtFile)
        {
            Filename = fileName + "_tex0.adt";
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
                    case "MTEX":
                        combinedFile.MTEX = new MTEX(subChunk);
                        break;
                    case "MCNK":
                        combinedFile.UpdateMapChunk(subChunk, mcnkIdx++);
                        break;
                    default:
                        Console.WriteLine($"Unhandled {subChunk.Name} chunk in Textures ADT.");
                        break;
                }
            }
        }
    }
}
