using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;
using WoWMap.Geometry;

namespace WoWMap.Chunks
{
    public class MODD : ChunkReader
    {
        public MODD(Chunk c) : base(c, c.Size) { }
        public MODD(Chunk c, uint h) : base(c, h) { }

        public uint ofsMODN;
        public Vector3 Position;
        public float[] Rotation;        // Quaternion
        public float Scale;
        public byte[] Color;

        public override void Read()
        {
            var br = Chunk.GetReader();

            ofsMODN = br.ReadUInt32();
            Position = br.ReadVector3();
            Rotation = new float[4];
            for (int i = 0; i < 4; i++)
                Rotation[i] = br.ReadSingle();
            Scale = br.ReadSingle();
            Color = new byte[4];
            for (int i = 0; i < 4; i++)
                Color[i] = br.ReadByte();
        }
    }
}
