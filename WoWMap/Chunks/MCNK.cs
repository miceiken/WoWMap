using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCNK : ChunkReader
    {
        public MCNK(Chunk c, uint h) : base(c, h) { }
        public MCNK(Chunk c) : base(c, c.Size) { }

        public MCNKFlags Flags;                                     // 0x000
        public uint IndexX;                                         // 0x004
        public uint IndexY;                                         // 0x008
        public uint nLayers;                                        // 0x00C
        public uint nDoodadRef;                                     // 0x010
        public uint ofsMCVT;                                        // 0x014
        public uint ofsMCNR;                                        // 0x018
        public uint ofsMCLY;                                        // 0x01C
        public uint ofsMCRF;                                        // 0x020
        public uint ofsMCAL;                                        // 0x024
        public uint nMCAL;                                          // 0x028
        public uint ofsMCSH;                                        // 0x02C
        public uint nMCSH;                                          // 0x030
        public uint AreaId;                                         // 0x034
        public uint nMapObjRefs;                                    // 0x038
        public ushort Holes;                                        // 0x03C
        public ushort HolesAlign;                                   // 0x03E
        public ushort[] ReallyLowQualityTextureingMap;              // 0x040 -- all hail Schlumpf, jk, who the hell even knows what a uint2 is
        public uint predTex;                                        // 0x050
        public uint nEffectDoodad;                                  // 0x054
        public uint ofsMCSE;                                        // 0x058
        public uint nSoundEmitters;                                 // 0x05C
        public uint ofsMCLQ;                                        // 0x060
        public uint nMCLQ;                                          // 0x064
        public Vector3 Position;                                    // 0x068
        public uint ofsMCCV;                                        // 0x074
        public uint ofsMCLV;                                        // 0x078
        private uint unused;                                        // 0x07C

        public static readonly uint ChunkHeaderSize = 128;

        [Flags]
        public enum MCNKFlags : uint
        {
            MCSH,
            Impass,
            LiquidRiver,
            LiquidOcean,
            LiquidMagma,
            MCCV,
            HighResolutionHoles = 0x10000
        };

        public override void Read()
        {
            var br = Chunk.GetReader();

            Flags = (MCNKFlags)br.ReadUInt32();
            IndexX = br.ReadUInt32();
            IndexY = br.ReadUInt32();
            nLayers = br.ReadUInt32();
            nDoodadRef = br.ReadUInt32();
            ofsMCVT = br.ReadUInt32();
            ofsMCNR = br.ReadUInt32();
            ofsMCLY = br.ReadUInt32();
            ofsMCRF = br.ReadUInt32();
            ofsMCAL = br.ReadUInt32();
            nMCAL = br.ReadUInt32();
            ofsMCSH = br.ReadUInt32();
            nMCSH = br.ReadUInt32();
            AreaId = br.ReadUInt32();
            nMapObjRefs = br.ReadUInt32();
            Holes = br.ReadUInt16();
            HolesAlign = br.ReadUInt16();
            ReallyLowQualityTextureingMap = new ushort[8];
            for (int i = 0; i < 8; i++)
                ReallyLowQualityTextureingMap[i] = br.ReadUInt16();
            predTex = br.ReadUInt32();
            nEffectDoodad = br.ReadUInt32();
            ofsMCSE = br.ReadUInt32();
            nSoundEmitters = br.ReadUInt32();
            ofsMCLQ = br.ReadUInt32();
            nMCLQ = br.ReadUInt32();
            Position = new Vector3(br);
            ofsMCCV = br.ReadUInt32();
            ofsMCLV = br.ReadUInt32();
            unused = br.ReadUInt32();
        }
    }
}
