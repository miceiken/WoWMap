using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCLY
    {
        public uint TextureId;
        public uint Flags;
        public uint ofsMCAL;
        public short EffectId;
        private short padding;

        public enum MCLYFlags : uint
        {
            Rotate45 = 0x1,
            Rotate90 = 0x2,
            Rotate180 = 0x4,
            Fast = 0x8,
            Faster = 0x10,
            Fastest = 0x20,
            Animate = 0x40,
            Brighter = 0x80,
            AlphaMap = 0x100,
            CompressedAlphaMap = 0x200,
            SkyboxReflection = 0x400
        };

        public void Read(BinaryReader br)
        {
            TextureId = br.ReadUInt32();
            Flags = br.ReadUInt32();
            ofsMCAL = br.ReadUInt32();
            EffectId = br.ReadInt16();
            padding = br.ReadInt16();
        }
    }
}
