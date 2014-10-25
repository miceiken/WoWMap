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
            Chunk = new ChunkData(filename).GetChunkByName("MOGP");
            
            Read();
        }

        public string Filename { get; private set; }
        public Chunk Chunk { get; private set; }

        public MOGP MOGP { get; private set; }
        public MOPY MOPY { get; private set; }
        public MOVI MOVI { get; private set; }
        public MOVT MOVT { get; private set; }
        public MONR MONR { get; private set; }
        public MODR MODR { get; private set; }
        public MLIQ MLIQ { get; private set; }

        public void Read()
        {
            var stream = Chunk.GetStream();
            var reader = new BinaryReader(stream);

            MOGP = new MOGP(Chunk);

            var offset = Chunk.Offset + MOGP.ChunkHeaderSize;
            while (offset < (Chunk.Offset + Chunk.Size))
            {
                stream.Seek(offset, SeekOrigin.Begin);

                var subChunkHeader = new ChunkHeader(reader);
                var subchunk = new Chunk(subChunkHeader, stream);
                switch (subChunkHeader.Name)
                {
                    case "MOPY":
                        MOPY = new MOPY(subchunk);
                        break;
                    case "MOVI":
                        MOVI = new MOVI(subchunk);
                        break;
                    case "MOVT":
                        MOVT = new MOVT(subchunk);
                        break;
                    case "MONR":
                        MONR = new MONR(subchunk);
                        break;
                    case "MODR":
                        MODR = new MODR(subchunk);
                        break;
                    case "MLIQ":
                        MLIQ = new MLIQ(subchunk);
                        break;
                }

                offset += 8 + subChunkHeader.Size;
            }
        }
    }
}
