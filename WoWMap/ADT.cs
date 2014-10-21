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
    public class ADT
    {
        public ADT(string filename)
        {
            Filename = filename;
        }

        public string Filename
        {
            get;
            private set;
        }

        public ChunkData Data { get; private set; }
        public MapChunk[] MapChunks { get; private set; }
        public MHDR Header { get; private set; }

        public void Read()
        {
            using (var file = File.Open(Filename, FileMode.Open))
            {
                Data = new ChunkData(file);

                Header = new MHDR();
                Header.Read(Data.GetChunkByName("MHDR").GetReader());

                MapChunks = new MapChunk[16 * 16];
                int idx = 0;
                foreach (var mapChunk in Data.Chunks.Where(c => c.Name == "MCNK"))
                    MapChunks[idx++] = new MapChunk(this, mapChunk);

                foreach (var mapChunk in MapChunks)
                    mapChunk.GenerateTriangles();
            }
        }
    }
}
