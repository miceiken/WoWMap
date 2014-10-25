using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MWMO : ChunkReader
    {
        public MWMO(Chunk c, uint h) : base(c, h) { }
        public MWMO(Chunk c) : base(c, c.Size) { }

        private string[] _filenames;

        public Dictionary<uint, string> Filenames;

        public override void Read()
        {
            var br = Chunk.GetReader();

            var chunk = br.ReadBytes((int)Chunk.Size);
            _filenames = Helpers.SplitStrings(chunk).ToArray();

            Filenames = new Dictionary<uint, string>();
            for (uint i = 0, off = 0; i < _filenames.Length; i++)
            {
                Filenames.Add(off, _filenames[i]);
                off += (uint)_filenames[i].Length + 1;
            }
        }
    }
}
