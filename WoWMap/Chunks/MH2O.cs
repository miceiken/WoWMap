using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MH2O
    {
        public MH2OHeader[] Headers;

        public void Read(BinaryReader br)
        {
            Headers = new MH2OHeader[256];
            for (int i = 0; i < Headers.Length; i++)
            {
                var entry = new MH2OHeader();
                entry.Read(br);
                Headers[i] = entry;
            }
        }

        public class MH2OHeader
        {
            public uint ofsInformation;
            public uint LayerCount;
            public ulong ofsRender; // or is it uint32???

            public void Read(BinaryReader br)
            {
                ofsInformation = br.ReadUInt32();
                LayerCount = br.ReadUInt32();
                ofsRender = br.ReadUInt64();
            }
        }

        public class MH2OInformation
        {
            public ushort LiquidTypeId;
            public ushort LiquidObjectId;
            public float MinHeightLevel;
            public float MaxHeightLevel;
            public byte XOffset;
            public byte YOffset;
            public byte Width;
            public byte Height;
            public uint ofsMask2;
            public uint ofsHeightmapData;

            public void Read(BinaryReader br)
            {
                LiquidTypeId = br.ReadUInt16();
                LiquidObjectId = br.ReadUInt16();
                MinHeightLevel = br.ReadSingle();
                MaxHeightLevel = br.ReadSingle();
                XOffset = br.ReadByte();
                YOffset = br.ReadByte();
                Width = br.ReadByte();
                Height = br.ReadByte();
                ofsMask2 = br.ReadUInt32();
                ofsHeightmapData = br.ReadUInt32();
            }
        }

        public class MH2OHeightmapData
        {
            public float[] Heightmap;
            public char[] Transparency;

            public void Read(BinaryReader br)
            { // TODO: Find out how to read this

            }
        }
    }
}
