using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;
using WoWMap.Geometry;
using OpenTK;

namespace WoWMap.Layers
{
    public class LiquidChunk
    {
        public LiquidChunk(ADT adt, Chunk chunk)
        {
            Chunk = chunk;
            ADT = adt;

            Read();
        }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }

        public MH2O MH2O { get; private set; }
        public MH2O.MH2OInformation[] Information { get; private set; }
        public MH2O.MH2OHeightmapData[] HeightMaps { get; private set; }

        public void Read()
        {
            if (Chunk == null) return;

            var stream = Chunk.GetStream();

            MH2O = new MH2O();
            MH2O.Read(Chunk.GetReader());
            HeightMaps = new MH2O.MH2OHeightmapData[256];
            Information = new MH2O.MH2OInformation[256];

            for (int i = 0; i < MH2O.Headers.Length; i++)
            {
                var header = MH2O.Headers[i];
                if (header == null || header.LayerCount == 0) continue;

                stream.Seek(Chunk.Offset + header.ofsInformation, SeekOrigin.Begin);
                var information = new MH2O.MH2OInformation();
                information.Read(Chunk.GetReader());
                Information[i] = information;

                // Ensure we have heightmap data
                if (information.ofsHeightmapData == 0) continue;

                // Not an ocean, lets grab the height map and render mask
                MH2O.MH2OHeightmapData heightMap;
                if (!IsOcean(information.LiquidObjectId, information.LiquidTypeId))
                {
                    // Read the height map
                    stream.Seek(Chunk.Offset + information.ofsHeightmapData, SeekOrigin.Begin);
                    heightMap = new MH2O.MH2OHeightmapData();
                    heightMap.Read(Chunk.GetReader());
                    heightMap.RenderMask = GetRenderMask(header, information);
                }
                else
                    heightMap = GetOceanHeightMap(information.MinHeightLevel);

                HeightMaps[i] = heightMap;
            }
        }

        private MH2O.MH2ORenderMask GetRenderMask(MH2O.MH2OHeader header, MH2O.MH2OInformation information)
        {
            var stream = Chunk.GetStream();

            // Read the render mask
            stream.Seek(Chunk.Offset + header.ofsRender, SeekOrigin.Begin);
            var renderMask = new MH2O.MH2ORenderMask();
            renderMask.Read(Chunk.GetReader());

            // Render mask 
            if ((renderMask.Mask.All(b => b == 0) || (information.Width == 8 && information.Height == 8)) && information.ofsMask2 != 0)
            {
                stream.Seek(Chunk.Offset + information.ofsMask2, SeekOrigin.Begin);
                var altMask = new byte[(int)Math.Ceiling(information.Width * information.Height / 8.0f)];
                stream.Read(altMask, 0, altMask.Length);

                for (int i = 0; i < altMask.Length; i++)
                    renderMask.Mask[i + information.YOffset] |= altMask[i];
            }

            return renderMask;
        }

        public static MH2O.MH2OHeightmapData GetOceanHeightMap(float heightLevel)
        {
            var data = new MH2O.MH2OHeightmapData
            {
                RenderMask = new MH2O.MH2ORenderMask
                {
                    Mask = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }
                }
            };
            data.Heightmap = new float[9, 9];
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                    data.Heightmap[x, y] = heightLevel;
            return data;
        }

        private static bool IsOcean(ushort liquidObjectId, ushort liquidType)
        {
            return liquidObjectId == 42 || liquidType == 2 || liquidType == 14;
        }
    }
}
