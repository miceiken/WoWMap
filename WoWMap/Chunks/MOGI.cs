using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;
using WoWMap.Geometry;

namespace WoWMap.Chunks
{
    public class MOGI : ChunkReader
    {
        public MOGI(Chunk c) : base(c, c.Size) { }
        public MOGI(Chunk c, uint h) : base(c, h) { }

        public MOGIFlags Flags;
        public Vector3 BoundingBox1;
        public Vector3 BoundingBox2;
        public uint ofsMOGN;

        [Flags]
        public enum MOGIFlags : uint
        {
            Outdoor = 0x8,
            unk0 = 0x40,
            unk1 = 0x80,
            Indoor = 0x2000,
            unk2 = 0x8000,      // Frequently used
            unk3 = 0x10000,     // Used in Stormwind?
            Skybox = 0x40000,
        };

        public override void Read()
        {
            var br = Chunk.GetReader();

            Flags = (MOGIFlags)br.ReadUInt32();
            BoundingBox1 = br.ReadVector3();
            BoundingBox2 = br.ReadVector3();
        }
    }
}
