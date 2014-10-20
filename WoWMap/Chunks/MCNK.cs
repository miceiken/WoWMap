using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCNK : IChunkReader
    {
        public uint Flags;
        public uint IndexX;
        public uint IndexY;
        public uint nLayers;
        public uint nDoodadRef;
        public uint ofsHeight;
        public uint ofsNormal;
        public uint ofsLayer;
        public uint ofsAlpha;
        public uint sizeAlpha;
        public uint ofsShadow;
        public uint sizeShadow;
        public uint AreaId;
        public uint nMapObjRefs;
        public uint Holes;
        public uint[] ReallyLowQualityTextureingMap;
        public uint predTex;
        public uint noEffectDoodad;
        public uint ofsSoundEmitters;
        public uint numSoundEmitters;
        public uint ofsLiquid;
        public uint sizeLiquid;
        public float[] Position;
        public uint ofsMCCV;
        public uint ofsMCLV;
        private uint unused;

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

        public void Read(ChunkHeader header, BinaryReader br)
        {
            Flags = br.ReadUInt32();
            IndexX = br.ReadUInt32();
            IndexY = br.ReadUInt32();
            nLayers = br.ReadUInt32();
            nDoodadRef = br.ReadUInt32();
            ofsHeight = br.ReadUInt32();
            ofsNormal = br.ReadUInt32();
            ofsLayer = br.ReadUInt32();
            ofsAlpha = br.ReadUInt32();
            sizeAlpha = br.ReadUInt32();
            ofsShadow = br.ReadUInt32();
            sizeShadow = br.ReadUInt32();
            AreaId = br.ReadUInt32();
            nMapObjRefs = br.ReadUInt32();
            Holes = br.ReadUInt32();
            for (int i = 0; i < 8; i++)
                ReallyLowQualityTextureingMap[i] = br.ReadUInt32();
            predTex = br.ReadUInt32();
            noEffectDoodad = br.ReadUInt32();
            ofsSoundEmitters = br.ReadUInt32();
            numSoundEmitters = br.ReadUInt32();
            ofsLiquid = br.ReadUInt32();
            sizeLiquid = br.ReadUInt32();
            for (int i = 0; i < 3; i++)
                Position[i] = br.ReadSingle();
            ofsMCCV = br.ReadUInt32();
            ofsMCLV = br.ReadUInt32();
            unused = br.ReadUInt32();
        }
    }
}
