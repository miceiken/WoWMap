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

        public void Read()
        {
            // Tile Table
            var chunk = Data.GetChunkByName("MAIN");
            if (chunk == null) return;

            IsValid = true;

            MAIN = new MAIN(chunk);

            TileTable = new bool[64, 64];
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                    TileTable[x, y] = MAIN.Entries[x, y].Flags.HasFlag(MAIN.MAINFlags.HasADT);

            // Global Model
            var fileChunk = Data.GetChunkByName("MWMO");
            var defChunk = Data.GetChunkByName("MODF");
            if (fileChunk == null || defChunk == null) return;

            IsGlobalModel = true;

            MODF = new MODF(defChunk);
            MWMO = new MWMO(fileChunk);
        }
    }
}
