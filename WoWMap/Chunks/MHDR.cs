using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MHDR
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
        private uint[] padding;

        public enum MHDRFlags : uint
        {
            MFBO = 1, // Contains a MFBO chunk
            Northrend = 2 // Is set for some Northrend ones
        };

        public MCIN MCIN { get; private set; }
        public MTEX MTEX { get; private set; }
        public MMDX MMDX { get; private set; }
        public MMID MMID { get; private set; }
        public MWMO MWMO { get; private set; }
        public MWID MWID { get; private set; }
        public MDDF MDDF { get; private set; }
        public MODF MODF { get; private set; }
        public MFBO MFBO { get; private set; }
        public MH2O MH2O { get; private set; }

        public void Read(BinaryReader br)
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

            // We don't have to read this if we keep jumping back and forth.
            //padding = new uint[4];
            //for (int i = 0; i < 4; i++)
            //    padding[i] = br.ReadUInt32();

            //var position = br.BaseStream.Position;
            //Process(br);
            //br.BaseStream.Position = position;
        }

        // How do we not make this ugly?
        public void Process(BinaryReader br)
        {
            var header = new ChunkHeader();

            if (ofsMCIN > 0)
            {
                br.BaseStream.Position = ofsMCIN;
                MCIN = new MCIN();
                header.Read(br);
                MCIN.Read(br);
            }

            if (ofsMTEX > 0)
            {
                br.BaseStream.Position = ofsMTEX;
                MTEX = new MTEX();
                header.Read(br);
                MTEX.Read(br, header.Size);
            }

            if (ofsMMDX > 0)
            {
                br.BaseStream.Position = ofsMMDX;
                MMDX = new MMDX();
                header.Read(br);
                MMDX.Read(br, header.Size);
            }

            if (ofsMMID > 0)
            {
                br.BaseStream.Position = ofsMMID;
                MMID = new MMID();
                header.Read(br);
                MMID.Read(br, header.Size);
            }

            if (ofsMWMO > 0)
            {
                br.BaseStream.Position = ofsMWMO;
                MWMO = new MWMO();
                header.Read(br);
                MWMO.Read(br, header.Size);
            }

            if (ofsMWID > 0)
            {
                br.BaseStream.Position = ofsMWID;
                MWID = new MWID();
                header.Read(br);
                MWID.Read(br, header.Size);
            }

            if (ofsMDDF > 0)
            {
                br.BaseStream.Position = ofsMDDF;
                MDDF = new MDDF();
                header.Read(br);
                MDDF.Read(br, header.Size);
            }

            if (ofsMODF > 0)
            {
                br.BaseStream.Position = ofsMODF;
                MODF = new MODF();
                header.Read(br);
                MODF.Read(br, header.Size);
            }

            if (ofsMFBO > 0)
            {
                br.BaseStream.Position = ofsMFBO;
                MFBO = new MFBO();
                header.Read(br);
                MFBO.Read(br);
            }

            if (ofsMH2O > 0)
            {
                br.BaseStream.Position = ofsMH2O;
                MH2O = new MH2O();
                header.Read(br);
                MH2O.Read(br);
            }
        }
    }
}
