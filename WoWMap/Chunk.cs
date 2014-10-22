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

        public int FindSubChunkOffset(string name)
        {
            var bytes = Encoding.ASCII.GetBytes(name).Reverse().ToArray();
            if (bytes.Length != 4) return -1;

            var stream = GetStream();            
            int matched = 0;
            while (stream.Position < stream.Length)
            {
                var b = stream.ReadByte();
                if (b == bytes[matched])
                    matched++;
                else
                    matched = 0;
                if (matched == 4)
                    return (int)(stream.Position - 4);
            }
            return -1;
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
