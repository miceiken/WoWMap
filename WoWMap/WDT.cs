using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;
using WoWMap.Geometry;

namespace WoWMap
{
    public class WDT
    {
        public WDT(string filename)
        {
            Filename = filename;
        }

        public string Filename
        {
            get;
            private set;
        }

        public ChunkData Data { get; private set; }
        public bool[,] TileTable { get; private set; }
        public MWMO MWMO { get; private set; }
        public MODF MODF { get; private set; }
        public MAIN MAIN { get; private set; }
        public string ModelFile { get; private set; }

        public bool HasTile(int x, int y)
        {
            return TileTable[x, y];
        }

        public void Read()
        {
            using (var file = File.Open(Filename, FileMode.Open))
            {
                Data = new ChunkData(file);

                // Tile Table
                var chunk = Data.GetChunkByName("MAIN");
                if (chunk == null) return;

                MAIN = new MAIN();
                MAIN.Read(chunk.GetReader());

                TileTable = new bool[64, 64];
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                        TileTable[x, y] = MAIN.Entries[x, y].Flags.HasFlag(MAIN.MAINFlags.HasADT);

                // Global Model
                var fileChunk = Data.GetChunkByName("MWMO");
                var defChunk = Data.GetChunkByName("MODF");
                if (fileChunk == null || defChunk == null) return;

                MODF = new MODF();
                MODF.Read(defChunk.GetReader(), defChunk.Size);

                MWMO = new MWMO();
                MWMO.Read(fileChunk.GetReader(), fileChunk.Size);
            }
        }
    }
}
