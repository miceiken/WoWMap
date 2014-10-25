using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MPHD : ChunkReader
    {
        public MPHD(Chunk c, uint h) : base(c, h) { }
        public MPHD(Chunk c) : base(c, c.Size) { }

        public MPHDFlags Flags;
        public uint Something;
        private uint[] Unused;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Flags = (MPHDFlags)br.ReadUInt32();
            Something = br.ReadUInt32();
            Unused = new uint[6];
            for (int i = 0; i < 6; i++)
                Unused[i] = br.ReadUInt32();
        }

        public enum MPHDFlags : uint
        {
            GlobalMapObject = 0x01,
            VertexShading = 0x02,
            TerrainShaders = 0x04,
            Disabled = 0x08,
            VertexLightning = 0x10,
            Ceiling = 0x20
        }
    }
}
