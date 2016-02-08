using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MTEX : ChunkReader
    {
        public MTEX(Chunk c, uint h) : base(c, h) { }
        public MTEX(Chunk c) : base(c, c.Size) { }

        public List<string> Filenames { get; private set; }

        public override void Read()
        {
            var br = Chunk.GetReader();

            Filenames = new List<string>();

            var data = br.ReadBytes((int)Chunk.Size);
            var sb = new StringBuilder();
            var offset = 0u;
            for (uint i = 0; i < data.Length; i++)
            {
                if (data[i] == '\0') // Terminate string
                {
                    if (sb.Length > 1)
                        Filenames.Add(sb.ToString());
                    offset = i + 1;
                    sb = new StringBuilder();
                }
                else
                    sb.Append((char)data[i]);
            }
        }
    }
}
