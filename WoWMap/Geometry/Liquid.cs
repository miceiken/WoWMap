using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;

namespace WoWMap.Geometry
{
    public class Liquid
    {
        public Liquid(ADT adt, Chunk chunk)
        {
            ADT = adt;
            Chunk = chunk;

            Read();
        }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }
        public MH2O MH2O { get; private set; }
        public MH2O.MH2OHeightmapData[] HeightMaps { get; private set; }
        public List<Vector3> Vertices { get; private set; }
        public List<Triangle<uint>> Triangles { get; private set; }

        public void Read()
        {
            if (Chunk == null) return;

            Vertices = new List<Vector3>();
            Triangles = new List<Triangle<uint>>();

            MH2O = new MH2O();
            MH2O.Read(Chunk.GetReader());

            int idx = 0;
            foreach (var header in MH2O.Headers)
            {
                MH2O.MH2OHeightmapData heightMap;
                if (header.LayerCount == 0) continue;
                if (header.Information.LiquidTypeId != 2)
                {
                    // I bet you $100 this is wrong
                    heightMap = header.Information.HeightmapData;
                    if ((header.Render.Mask.All(b => b == 0) || (header.Information.Width == 8 && header.Information.Height == 8)) && header.Information.Mask2 != null)
                        heightMap = header.Information.Mask2;
                }
                else // Ocean
                    heightMap = MH2O.MH2OHeightmapData.GetOceanData(header.Information);

                HeightMaps[idx] = heightMap;

                for (int y = header.Information.YOffset; y < (header.Information.YOffset + header.Information.Height); y++)
                {
                    for (int x = header.Information.XOffset; x < (header.Information.XOffset + header.Information.Width); x++)
                    {
                        if (!heightMap.Transparency.ShouldRender(x, y)) continue;
                        Console.WriteLine("Render [{0}, {1}]", x, y);

                        var mapChunk = ADT.MapChunks[idx];
                        var location = mapChunk.MCNK.Position;
                        location[1] = location[1] - (x * Global.UnitSize);
                        location[0] = location[0] - (y * Global.UnitSize);
                        location[2] = heightMap.Heightmap[x, y];

                        var vertOffset = (uint)Vertices.Count;
                        Vertices.Add(new Vector3(location[0], location[1], location[2]));
                        Vertices.Add(new Vector3(location[0] - Global.UnitSize, location[1], location[2]));
                        Vertices.Add(new Vector3(location[0], location[1] - Global.UnitSize, location[2]));
                        Vertices.Add(new Vector3(location[0] - Global.UnitSize, location[1] - Global.UnitSize, location[2]));

                        Triangles.Add(new Triangle<uint>(TriangleType.Water, vertOffset, vertOffset + 2, vertOffset + 1));
                        Triangles.Add(new Triangle<uint>(TriangleType.Water, vertOffset + 2, vertOffset + 3, vertOffset + 1));

                    }
                }

                idx++;
            }
        }
    }
}
