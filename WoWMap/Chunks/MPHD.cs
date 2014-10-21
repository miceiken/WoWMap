using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MPHD
    {
        public MPHDFlags Flags;
        public uint Something;
        private uint[] Unused;

        public void Read(BinaryReader br)
        {
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
