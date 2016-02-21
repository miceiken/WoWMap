using System.Collections.Generic;
using System.IO;
using WoWMap.Chunks;
using WoWMap.Geometry;
using OpenTK;
using System.Linq;

namespace WoWMap.Layers
{
    public class WMOGroup
    {
        public WMOGroup(string filename, WMORoot root)
        {
            Filename = filename;
            Root = root;

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

        public WMORoot Root { get; private set; }

        public MOGP MOGP { get; private set; }
        public MOPY MOPY { get; private set; }
        public MOVI MOVI { get; private set; }
        public MOVT MOVT { get; private set; }
        public MONR MONR { get; private set; }
        public MODR MODR { get; private set; }
        public MLIQ MLIQ { get; private set; }

        public Mesh Terrain { get; private set; }
        public Mesh Liquid { get; private set; }

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
                        break;
                }
            }

            Generate();
        }

        public void Generate()
        {

            Terrain = GenerateTerrain();
            Liquid = GenerateLiquid();
        }

        public Mesh GenerateTerrain(MODF.MODFEntry wmoDefinition = null)
        {
            if (MOVT == null) return null;

            var indices = new List<uint>();

            for (var i = 0; i < MOVI.Indices.Length; i += 3)
            {
                if (((byte)MOPY.Entries[i / 3].Flags & 0x04) != 0 && MOPY.Entries[i / 3].MaterialId != 0xFF)
                    continue;
                indices.AddRange(new uint[] {
                    MOVI.Indices[i], MOVI.Indices[i + 1], MOVI.Indices[i + 2]
                });
            }

            if (wmoDefinition != null)
            {
                var transform = Transformation.GetWMOTransform(wmoDefinition.Position, wmoDefinition.Rotation);

                return new Mesh
                {
                    Type = MeshType.Terrain,
                    Indices = indices.ToArray(),
                    Vertices = MOVT.Vertices.Select(v => Vector3.Transform(v, transform)).ToArray(),
                    Normals = MONR.Normals.Select(v => Vector3.Transform(v, transform)).ToArray(),
                };
            }

            return new Mesh
            {
                Type = MeshType.Terrain,
                Indices = indices.ToArray(),
                Vertices = MOVT.Vertices,
                Normals = MONR.Normals,
            };
        }

        public Mesh GenerateLiquid(MODF.MODFEntry wmoDefinition = null)
        {
            if (MLIQ == null) return null;

            var vertices = new List<Vector3>((int)(MLIQ.Height * MLIQ.Width) * 4);
            var indices = new List<uint>((int)((MLIQ.Height * MLIQ.Width) * 3));

            var relPos = MLIQ.Position;
            for (var y = 0; y < MLIQ.Height; y++)
            {
                for (var x = 0; x < MLIQ.Width; x++)
                {
                    if (!MLIQ.ShouldRender(x, y))
                        continue;

                    var vo = (uint)vertices.Count;

                    vertices.AddRange(new[] {
                        relPos - new Vector3(x * Constants.UnitSize, y * Constants.UnitSize, MLIQ.HeightMap[x, y]),
                        relPos - new Vector3((x + 1) * Constants.UnitSize, y * Constants.UnitSize, MLIQ.HeightMap[x + 1, y]),
                        relPos - new Vector3(x * Constants.UnitSize, (y + 1) * Constants.UnitSize, MLIQ.HeightMap[x, y + 1]),
                        relPos - new Vector3((x + 1) * Constants.UnitSize, (y + 1) * Constants.UnitSize, MLIQ.HeightMap[x + 1, y + 1])
                    });

                    indices.AddRange(new uint[] {
                        vo, vo + 2, vo + 1,
                        vo + 2, vo + 3, vo + 1
                    });
                }
            }

            if (wmoDefinition != null)
            {
                var transform = Transformation.GetWMOTransform(wmoDefinition.Position, wmoDefinition.Rotation);

                return new Mesh
                {
                    Type = MeshType.Terrain,
                    Indices = indices.ToArray(),
                    Vertices = vertices.Select(v => Vector3.Transform(v, transform)).ToArray(),
                };
            }

            return new Mesh
            {
                Type = MeshType.Liquid,
                Indices = indices.ToArray(),
                Vertices = vertices.ToArray(),
            };
        }
    }
}
