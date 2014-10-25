using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Archive;
using WoWMap.Chunks;
using WoWMap.Geometry;
using System.IO;

namespace WoWMap.Layers
{
    public class WMOGroup
    {
        public WMOGroup(string filename)
        {
            Filename = filename;

            var mainChunk = new ChunkData(filename);
            MOGP = new MOGP(Chunk = mainChunk.GetChunkByName("MOGP"));

            var stream = Chunk.GetStream();
            stream.Seek(Chunk.Offset + MOGP.ChunkHeaderSize, SeekOrigin.Begin);
            SubData = new ChunkData(stream, Chunk.Size - MOGP.ChunkHeaderSize);

            Read();
        }

        public string Filename { get; private set; }
        public Chunk Chunk { get; private set; }
        public ChunkData SubData { get; private set; }

        public MOGP MOGP { get; private set; }
        public MOPY MOPY { get; private set; }
        public MOVI MOVI { get; private set; }
        public MOVT MOVT { get; private set; }
        public MONR MONR { get; private set; }
        public MODR MODR { get; private set; }
        public MLIQ MLIQ { get; private set; }

        public void Read()
        {
            foreach (var subChunk in SubData.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MOPY":
                        MOPY = new MOPY(subChunk);
                        break;
                    case "MOVI":
                        MOVI = new MOVI(subChunk);
                        break;
                    case "MOVT":
                        MOVT = new MOVT(subChunk);
                        break;
                    case "MONR":
                        MONR = new MONR(subChunk);
                        break;
                    case "MODR":
                        MODR = new MODR(subChunk);
                        break;
                    case "MLIQ":
                        MLIQ = new MLIQ(subChunk);
                        break;
                }
            }

            // Do shit
        }
    }
}
