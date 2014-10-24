using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;

namespace WoWMap.Chunks
{
    public class MCNK
    {
        public MCNKFlags Flags;
        public uint IndexX;
        public uint IndexY;
        public uint nLayers;
        public uint nDoodadRef;
        public uint ofsMCVT;
        public uint ofsMCNR;
        public uint ofsMCLY;
        public uint ofsMCRF;
        public uint ofsMCAL;
        public uint sizeMCAL;
        public uint ofsMCSH;
        public uint sizeMCSH;
        public uint AreaId;
        public uint nMapObjRefs;
        public ushort Holes;
        public ushort HolesAlign;
        public uint[,] ReallyLowQualityTextureingMap;
        public uint predTex;
        public uint nEffectDoodad;
        public uint ofsSoundEmitters;
        public uint nSoundEmitters;
        public uint ofsLiquid;
        public uint sizeLiquid;
        public Vector3 Position;
        public uint ofsMCCV;
        public uint ofsMCLV;
        private uint unused;

        public static readonly int ChunkHeaderSize = 128;

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

        public void Read(BinaryReader br)
        {
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
            sizeMCAL = br.ReadUInt32();
            ofsMCSH = br.ReadUInt32();
            sizeMCSH = br.ReadUInt32();
            AreaId = br.ReadUInt32();
            nMapObjRefs = br.ReadUInt32();
            Holes = br.ReadUInt16();
            HolesAlign = br.ReadUInt16();
            ReallyLowQualityTextureingMap = new uint[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    ReallyLowQualityTextureingMap[i, j] = br.ReadUInt32();
            predTex = br.ReadUInt32();
            nEffectDoodad = br.ReadUInt32();
            ofsSoundEmitters = br.ReadUInt32();
            nSoundEmitters = br.ReadUInt32();
            ofsLiquid = br.ReadUInt32();
            sizeLiquid = br.ReadUInt32();
            Position = new Vector3(br);
            ofsMCCV = br.ReadUInt32();
            ofsMCLV = br.ReadUInt32();
            unused = br.ReadUInt32();
        }
    }
}
