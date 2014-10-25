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
        public ChunkReader(Chunk chunk, uint headerSize)
        {
            Chunk = chunk;
            HeaderSize = headerSize;

            Read();
        }

        public ChunkReader(Chunk chunk)
            : this(chunk, chunk.Size)
        { }

        public Chunk Chunk { get; private set; }
        public uint HeaderSize { get; private set; }

        public abstract void Read();
    }
}
