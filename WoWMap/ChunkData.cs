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
        public ChunkData(Stream stream)
        {
            Stream = stream;
            Chunks = new List<Chunk>();

            var br = new BinaryReader(stream);
            var baseOffset = (uint)stream.Position;
            var calcOffset = 0u;
            while ((calcOffset + baseOffset) < stream.Length && (calcOffset < stream.Length))
            {
                var header = new ChunkHeader(br);
                calcOffset += 8; // Add 8 bytes as we read header name + size
                Chunks.Add(new Chunk(header.Name, header.Size, calcOffset + baseOffset, stream));
                calcOffset += header.Size; // Move past the chunk

                // We seek from our current position to save some time
                if ((calcOffset + baseOffset) < stream.Length && calcOffset < stream.Length)
                    stream.Seek(header.Size, SeekOrigin.Current);
            }
        }

        public ChunkData(string filename)
            : this(CASC.OpenFile(filename))
        { }

        public Chunk GetChunkByName(string name)
        {
            return Chunks.FirstOrDefault(c => c.Name == name);
        }

        public Stream Stream { get; private set; }
        public List<Chunk> Chunks { get; private set; }
    }
}
