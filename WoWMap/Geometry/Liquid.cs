using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;
using System.Diagnostics;

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

            var stream = Chunk.GetStream();

            MH2O = new MH2O();
            MH2O.Read(Chunk.GetReader());
            HeightMaps = new Chunks.MH2O.MH2OHeightmapData[256];

            for (int i = 0; i < MH2O.Headers.Length; i++)
            {
                var header = MH2O.Headers[i];
                if (header == null || header.LayerCount == 0) continue;

                stream.Seek(Chunk.Offset + header.ofsInformation, SeekOrigin.Begin);
                var information = new MH2O.MH2OInformation();
                information.Read(Chunk.GetReader());


                Debug.WriteLine("--- MH2O #{0}", i);
                MH2O.MH2OHeightmapData heightMap;
                if (information.LiquidTypeId != 2)
                {
                    stream.Seek(Chunk.Offset + header.ofsRender, SeekOrigin.Begin);
                    var renderMask = new MH2O.MH2ORenderMask();
                    renderMask.Read(Chunk.GetReader());
                    Debug.WriteLine("RenderMask: {0}", string.Join(" ", renderMask.Mask));

                    if ((renderMask.Mask.All(b => b == 0) || (information.Width == 8 && information.Height == 8)) && information.ofsMask2 != 0)
                    {
                        stream.Seek(Chunk.Offset + information.ofsMask2, SeekOrigin.Begin);
                        var altMask = new byte[(int)Math.Ceiling(information.Width * information.Height / 8.0f)];
                        stream.Read(altMask, 0, altMask.Length);

                        for (int mi = 0; mi < altMask.Length; mi++)
                            renderMask.Mask[mi + information.YOffset] |= altMask[mi];
                    }
                    Debug.WriteLine("RenderMask: {0}", string.Join(" ", renderMask.Mask));

                    stream.Seek(Chunk.Offset + information.ofsHeightmapData, SeekOrigin.Begin);
                    heightMap = new MH2O.MH2OHeightmapData();
                    heightMap.Read(Chunk.GetReader());
                }
                else // Ocean
                    heightMap = GetOceanHeightMap(information.MinHeightLevel);

                Debug.WriteLine("RenderMask: {0}", string.Join(" ", heightMap.Transparency.Mask));
                Debug.WriteLine("Heights: {0}", string.Join(" ", heightMap.Heightmap));
                Debug.WriteLine("---");

                HeightMaps[i] = heightMap;

                for (int y = information.YOffset; y < (information.YOffset + information.Height); y++)
                {
                    for (int x = information.XOffset; x < (information.XOffset + information.Width); x++)
                    {
                        if (!heightMap.Transparency.ShouldRender(x, y)) continue;

                        var mapChunk = ADT.MapChunks[i];
                        var location = mapChunk.MCNK.Position;
                        location[1] = location[1] - (x * Constants.UnitSize);
                        location[0] = location[0] - (y * Constants.UnitSize);
                        location[2] = heightMap.Heightmap[x, y];

                        var vertOffset = (uint)Vertices.Count;
                        Vertices.Add(new Vector3(location[0], location[1], location[2]));
                        Vertices.Add(new Vector3(location[0] - Constants.UnitSize, location[1], location[2]));
                        Vertices.Add(new Vector3(location[0], location[1] - Constants.UnitSize, location[2]));
                        Vertices.Add(new Vector3(location[0] - Constants.UnitSize, location[1] - Constants.UnitSize, location[2]));

                        Triangles.Add(new Triangle<uint>(TriangleType.Water, vertOffset, vertOffset + 2, vertOffset + 1));
                        Triangles.Add(new Triangle<uint>(TriangleType.Water, vertOffset + 2, vertOffset + 3, vertOffset + 1));

                    }
                }
            }
        }

        public static MH2O.MH2OHeightmapData GetOceanHeightMap(float heightLevel)
        {
            var data = new MH2O.MH2OHeightmapData { Transparency = new MH2O.MH2ORenderMask { Mask = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF } } };
            data.Heightmap = new float[9, 9];
            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                    data.Heightmap[x, y] = heightLevel;
            return data;
        }
    }
}
