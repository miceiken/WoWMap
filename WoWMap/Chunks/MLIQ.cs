using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;
using WoWMap.Geometry;

namespace WoWMap.Chunks
{
    public class MLIQ : ChunkReader
    {
        public MLIQ(Chunk c, uint h) : base(c, h) { }
        public MLIQ(Chunk c) : base(c, c.Size) { }

        // http://pxr.dk/wowdev/wiki/index.php?title=WMO/v17#MLIQ_chunk

        public uint XVertices;
        public uint YVertices;
        public uint Width;
        public uint Height;
        public Vector3 Position;
        public ushort MaterialId;
        public float[][] HeightMap;
        public byte[] Types;

        public override void Read()
        {
            var br = Chunk.GetReader();

            XVertices = br.ReadUInt32();
            YVertices = br.ReadUInt32();
            Width = br.ReadUInt32();
            Height = br.ReadUInt32();
            Position = br.ReadVector3();
            MaterialId = br.ReadUInt16();
            // TODO: read the rest?
        }
    }
}
