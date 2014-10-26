using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Readers;
using WoWMap.Geometry;
using SharpDX;

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
        public float[,] HeightMap;
        public byte[,] RenderFlags;

        public bool ShouldRender(int x, int y)
        {
            return RenderFlags[x, y] != 0x0F;
        }

        public override void Read()
        {
            var br = Chunk.GetReader();

            XVertices = br.ReadUInt32();
            YVertices = br.ReadUInt32();
            Width = br.ReadUInt32();
            Height = br.ReadUInt32();
            Position = br.ReadVector3();
            MaterialId = br.ReadUInt16();
            HeightMap = new float[XVertices, YVertices];
            for (int y = 0; y < YVertices; y++)
            {
                for (int x = 0; x < XVertices; x++)
                {
                    br.ReadUInt32();
                    HeightMap[x, y] = br.ReadSingle();
                }
            }
            RenderFlags = new byte[Width, Height];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    RenderFlags[x, y] = br.ReadByte();
        }
    }
}
