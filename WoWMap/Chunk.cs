using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;

namespace WoWMap
{
    public class Chunk
    {
        public string Name { get; private set; }
        public uint Size { get; private set; }
        public uint Offset { get; private set; }
        private Stream Stream { get; set; }

        public Chunk(string name, uint size, uint offset, Stream stream)
        {
            Name = name;
            Size = size;
            Offset = offset;
            Stream = stream;
        }

        public Stream GetStream()
        {
            Stream.Seek(Offset, SeekOrigin.Begin);
            return Stream;
        }

        private BinaryReader _reader;
        public BinaryReader GetReader()
        {
            return _reader ?? (_reader = new BinaryReader(GetStream()));
        }

    }
}
