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
            public MH2OInformation Information;
            public uint LayerCount;
            public MH2ORenderMask Render;

            public void Read(BinaryReader br)
            {
                var ofsInformation = br.ReadUInt32();
                LayerCount = br.ReadUInt32();
                var ofsRender = br.ReadUInt32();

                var pos = br.BaseStream.Position;
                if (ofsInformation > 0)
                {
                    br.BaseStream.Position = ofsInformation;

                    var info = new MH2OInformation();
                    info.Read(br, this);
                    Information = info;
                }

                if (ofsRender > 0)
                {
                    br.BaseStream.Position = ofsRender;

                    var render = new MH2ORenderMask();
                    render.Read(br);
                    Render = render;
                }
                br.BaseStream.Position = pos;
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
            public MH2OHeightmapData Mask2;
            public MH2OHeightmapData HeightmapData;

            public void Read(BinaryReader br, MH2OHeader header)
            {
                LiquidTypeId = br.ReadUInt16();
                LiquidObjectId = br.ReadUInt16();
                MinHeightLevel = br.ReadSingle();
                MaxHeightLevel = br.ReadSingle();
                XOffset = br.ReadByte();
                YOffset = br.ReadByte();
                Width = br.ReadByte();
                Height = br.ReadByte();
                var ofsMask2 = br.ReadUInt32();
                var ofsHeightmapData = br.ReadUInt32();

                var pos = br.BaseStream.Position;

                if (ofsHeightmapData > 0)
                {
                    br.BaseStream.Position = ofsHeightmapData;

                    var heightMap = new MH2OHeightmapData();
                    heightMap.Read(br);
                    HeightmapData = heightMap;
                }

                if (ofsMask2 > 0)
                {
                    br.BaseStream.Position = ofsMask2;

                    Mask2 = MH2OHeightmapData.GetAlternativeData(header, this);
                }

                br.BaseStream.Position = pos;
            }
        }

        public class MH2OHeightmapData
        {
            public const float MaxStandableHeight = 1.5f;

            public float[,] Heightmap;
            public MH2ORenderMask Transparency;

            public void Read(BinaryReader br)
            {
                Heightmap = new float[9, 9];
                for (int y = 0; y < 9; y++)
                    for (int x = 0; x < 9; x++)
                        Heightmap[x, y] = br.ReadSingle();

                var transparency = new MH2ORenderMask();
                transparency.Read(br);
                Transparency = transparency;
            }

            public bool IsWater(int x, int y, float height)
            {
                if (Heightmap == null || Transparency == null)
                    return false;
                if (!Transparency.ShouldRender(x, y))
                    return false;
                var diff = Heightmap[x, y] - height;
                if (diff > MaxStandableHeight)
                    return true;
                return false;
            }

            public static MH2OHeightmapData GetOceanData(MH2OInformation info)
            {
                var data = new MH2OHeightmapData { Transparency = new MH2ORenderMask { Mask = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF } } };
                data.Heightmap = new float[9, 9];
                for (int y = 0; y < 9; y++)
                    for (int x = 0; x < 9; x++)
                        data.Heightmap[x, y] = info.MinHeightLevel;
                return data;
            }

            public static MH2OHeightmapData GetAlternativeData(MH2OHeader header, MH2OInformation info)
            {
                var data = new MH2OHeightmapData() { Transparency = new MH2ORenderMask { Mask = new byte[(int)Math.Ceiling(info.Width * info.Height / 8.0f)] } };
                for (int i = 0; i < data.Transparency.Mask.Length; i++)
                    data.Transparency.Mask[i + info.YOffset] |= header.Render.Mask[i];

                data.Heightmap = new float[9, 9];
                for (int y = info.YOffset; y < (info.YOffset + info.Height); y++)
                    for (int x = info.XOffset; x < (info.XOffset + info.Width); x++)
                        data.Heightmap[x, y] = info.HeightmapData.Heightmap[x, y];

                return data;
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
