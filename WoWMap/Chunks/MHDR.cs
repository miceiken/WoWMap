using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MHDR : IChunkReader
    {
        public MHDRFlags Flags;
        public uint ofsMCIN;
        public uint ofsMTEX;
        public uint ofsMMDX;
        public uint ofsMMID;
        public uint ofsMWMO;
        public uint ofsMWID;
        public uint ofsMDDF;
        public uint ofsMODF;
        public uint ofsMFBO;
        public uint ofsMH2O;
        public uint ofsMTXF;
        public uint[] padding;

        public enum MHDRFlags : uint
        {
            MFBO = 1, // Contains a MFBO chunk
            Northrend = 2 // Is set for some Northrend ones
        };

        public void Read(ChunkHeader header, BinaryReader br)
        {
            Flags = (MHDRFlags)br.ReadUInt32();
            ofsMCIN = br.ReadUInt32();
            ofsMTEX = br.ReadUInt32();
            ofsMMDX = br.ReadUInt32();
            ofsMMID = br.ReadUInt32();
            ofsMWMO = br.ReadUInt32();
            ofsMWID = br.ReadUInt32();
            ofsMDDF = br.ReadUInt32();
            ofsMODF = br.ReadUInt32();
            ofsMFBO = br.ReadUInt32();
            ofsMH2O = br.ReadUInt32();
            ofsMTEX = br.ReadUInt32();
            for (int i = 0; i < 4; i++)
                padding[i] = br.ReadUInt32();
        }
    }
}
