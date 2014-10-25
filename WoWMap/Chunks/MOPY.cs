using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MOPY : ChunkReader
    {
        public MOPY(Chunk c, uint h) : base(c, h) { }
        public MOPY(Chunk c) : base(c, c.Size) { }

        public MOPYFlags Flags;
        public byte MaterialId;

        [Flags]
        public enum MOPYFlags : byte
        {
            NoCamCollide,
            Detail,
            Collision,
            Hint,
            Render,
            CollideHit
        };

        public override void Read()
        {
            var br = Chunk.GetReader();

            Flags = (MOPYFlags)br.ReadByte();
            MaterialId = br.ReadByte();
        }
    }
}
