using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Archive;
using WoWMap.Chunks;
using WoWMap.Geometry;

namespace WoWMap.Layers
{
    public class WMOGroup
    {
        public WMOGroup(string filename)
        {
            Filename = filename;

            var mainChunk = new ChunkData(filename);
            MOGP = new MOGP(Chunk = mainChunk.GetChunkByName("MOGP"));

            var stream = Chunk.GetStream();
            stream.Seek(Chunk.Offset + MOGP.ChunkHeaderSize, SeekOrigin.Begin);
            SubData = new ChunkData(stream, Chunk.Size - MOGP.ChunkHeaderSize);

            Read();
        }

        public string Filename { get; private set; }
        public Chunk Chunk { get; private set; }
        public ChunkData SubData { get; private set; }

        public MOGP MOGP { get; private set; }
        public MOPY MOPY { get; private set; }
        public MOVI MOVI { get; private set; }
        public MOVT MOVT { get; private set; }
        public MONR MONR { get; private set; }
        public MODR MODR { get; private set; }
        public MLIQ MLIQ { get; private set; }

        public List<Vector3> LiquidVertices;
        public List<Triangle<uint>> LiquidIndices;

        public void Read()
        {
            foreach (var subChunk in SubData.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MOPY":
                        MOPY = new MOPY(subChunk);
                        break;
                    case "MOVI":
                        MOVI = new MOVI(subChunk);
                        break;
                    case "MOVT":
                        MOVT = new MOVT(subChunk);
                        break;
                    case "MONR":
                        MONR = new MONR(subChunk);
                        break;
                    case "MODR":
                        MODR = new MODR(subChunk);
                        break;
                    case "MLIQ":
                        MLIQ = new MLIQ(subChunk);

                        ReadLiquid();
                        break;
                }
            }
        }

        public void ReadLiquid()
        {
            LiquidVertices = new List<Vector3>((int)(MLIQ.Height * MLIQ.Width) * 4);
            LiquidIndices = new List<Triangle<uint>>((int)((MLIQ.Height * MLIQ.Width) * 2));

            var relPos = MLIQ.Position;
            for (int y = 0; y < MLIQ.Height; y++)
            {
                for (int x = 0; x < MLIQ.Width; x++)
                {
                    if (!MLIQ.ShouldRender(x, y))
                        continue;

                    var vo = (uint)LiquidVertices.Count;

                    LiquidVertices.Add(relPos - new Vector3(x * Constants.UnitSize, y * Constants.UnitSize, MLIQ.HeightMap[x, y]));
                    LiquidVertices.Add(relPos - new Vector3((x + 1) * Constants.UnitSize, y * Constants.UnitSize, MLIQ.HeightMap[x + 1, y]));
                    LiquidVertices.Add(relPos - new Vector3(x * Constants.UnitSize, (y + 1) * Constants.UnitSize, MLIQ.HeightMap[x, y + 1]));
                    LiquidVertices.Add(relPos - new Vector3((x + 1) * Constants.UnitSize, (y + 1) * Constants.UnitSize, MLIQ.HeightMap[x + 1, y + 1]));

                    LiquidIndices.Add(new Triangle<uint>(TriangleType.Water, vo, vo + 2, vo + 1));
                    LiquidIndices.Add(new Triangle<uint>(TriangleType.Water, vo + 2, vo + 3, vo + 1));
                }
            }
        }
    }
}
