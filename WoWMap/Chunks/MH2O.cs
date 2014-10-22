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
            public uint ofsRender;

            public void Read(BinaryReader br)
            {
                ofsInformation = br.ReadUInt32();
                LayerCount = br.ReadUInt32();
                ofsRender = br.ReadUInt32();
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
            public const float MaxStandableHeight = 1.5f;

            public float[,] Heightmap;
            public MH2ORenderMask RenderMask;

            public void Read(BinaryReader br)
            {
                Heightmap = new float[9, 9];
                for (int y = 0; y < 9; y++)
                    for (int x = 0; x < 9; x++)
                        Heightmap[x, y] = br.ReadSingle();
            }

            public bool IsWater(int x, int y, float height)
            {
                if (Heightmap == null || RenderMask == null)
                    return false;
                if (!RenderMask.ShouldRender(x, y))
                    return false;
                var diff = Heightmap[x, y] - height;
                if (diff > MaxStandableHeight)
                    return true;
                return false;
            }
        }

        public class MH2ORenderMask
        {
            public byte[] Mask;

            public bool ShouldRender(int x, int y)
            {
                return (Mask[y] >> x & 1) != 0;
            }

            public void Read(BinaryReader br)
            {
                Mask = br.ReadBytes(8);
            }
        }
    }
}
