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
            for (var i = 0; i < 6; i++)
                Unused[i] = br.ReadUInt32();
        }

        public bool HasFlag(MPHDFlags flag)
        {
            return (Flags & flag) != 0;
        }

        [Flags]
        public enum MPHDFlags : uint
        {
            GlobalMapObject = 0x01,
            VertexShading = 0x02, // vertexBufferFormat = PNC. (adds color: ADT.MCNK.MCCV)
            TerrainShaders = 0x04, // shader = 2. Decides whether to use _env terrain shaders or not: funky and if MCAL has 4096 instead of 2048(?)
            Disabled = 0x08,
            VertexLightning = 0x10, // vertexBufferFormat = PNC2. (adds second color: ADT.MCNK.MCLV)
            Ceiling = 0x20,
            TotallyUnk = 0x40,
            SomethingShader = 0x80 // affects mcal too
        }
    }
}
