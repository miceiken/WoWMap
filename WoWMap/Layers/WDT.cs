using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;
using WoWMap.Geometry;
using WoWMap.Archive;

namespace WoWMap.Layers
{
    public class WDT
    {
        public WDT(string filename)
        {
            Data = new ChunkData(filename);
            Read();
        }

        public ChunkData Data { get; private set; }

        public bool IsValid { get; private set; }
        public bool[,] TileTable { get; private set; }
        public MAIN MAIN { get; private set; }

        public bool IsGlobalModel { get; private set; }
        public MWMO MWMO { get; private set; }
        public MODF MODF { get; private set; }
        public string ModelFile { get; private set; }

        public bool HasTile(int x, int y)
        {
            return TileTable[x, y];
        }

        public int TileCount
        {
            get
            {
                var c = 0;
                for (var y = 0; y < 64; y++)
                    for (var x = 0; x < 64; x++)
                        if (TileTable[x, y])
                            ++c;
                return c;
            }
        }

        public void Read()
        {
            foreach (var subChunk in Data.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MAIN":
                        MAIN = new MAIN(subChunk);

                        IsValid = true;

                        TileTable = new bool[64, 64];
                        for (var y = 0; y < 64; y++)
                            for (var x = 0; x < 64; x++)
                                TileTable[x, y] = MAIN.Entries[x, y].Flags.HasFlag(MAIN.MAINFlags.HasADT);                        
                        break;

                    case "MWMO":
                        MWMO = new MWMO(subChunk);
                        break;

                    case "MODF":
                        MODF = new MODF(subChunk);
                        break;
                }
            }

            IsGlobalModel = (MODF != null && MWMO != null);
        }
    }
}
