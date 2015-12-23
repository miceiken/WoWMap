using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Archive;

namespace WoWMap
{
    public class ChunkData
    {
        public ChunkData(Stream stream, uint chunkSize = 0)
        {
            FromStream(stream, chunkSize);
        }

        public ChunkData(string filename)
        {
            FromStream(CASC.OpenFile(filename));
        }

        private void FromStream(Stream stream, uint chunkSize = 0)
        {
            Stream = stream;
            Chunks = new List<Chunk>();

            var maxRead = (uint)stream.Position + chunkSize;
            if (chunkSize == 0)
                maxRead = (uint)stream.Length;
            maxRead = Math.Min(maxRead, (uint)stream.Length);

            var br = new BinaryReader(stream);

            var baseOffset = (uint)stream.Position;
            var calcOffset = 0u;
            while ((calcOffset + baseOffset) < maxRead && (calcOffset < maxRead))
            {
                var header = new ChunkHeader(br);
                calcOffset += 8; // Add 8 bytes as we read header name + size
                Chunks.Add(new Chunk(header.Name, header.Size, calcOffset + baseOffset, stream));
                calcOffset += header.Size; // Move past the chunk

                // We seek from our current position to save some time
                if ((calcOffset + baseOffset) < maxRead && calcOffset < maxRead)
                    stream.Seek(header.Size, SeekOrigin.Current);
            }
        }

        public Stream Stream { get; private set; }
        public List<Chunk> Chunks { get; private set; }

        public Chunk GetChunkByName(string name)
        {
            return Chunks.FirstOrDefault(c => c.Name == name);
        }        
    }
}
