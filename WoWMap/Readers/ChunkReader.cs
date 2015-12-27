using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Readers
{
    public abstract class ChunkReader
    {        
        public ChunkReader(Chunk chunk, uint headerSize, bool read = true)
        {
            Chunk = chunk;
            HeaderSize = headerSize;

            if (read)
                Read();
        }

        public ChunkReader(Chunk chunk, bool read = true)
            : this(chunk, chunk.Size, read)
        { }

        public Chunk Chunk { get; private set; }
        public uint HeaderSize { get; private set; }

        public abstract void Read();
    }
}
