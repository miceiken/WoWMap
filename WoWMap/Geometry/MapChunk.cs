using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;
using System.IO;

namespace WoWMap.Geometry
{
    public class MapChunk
    {
        public MapChunk(ADT adt, Chunk chunk)
        {
            ADT = adt;
            Chunk = chunk;
            var stream = chunk.GetStream();
            MCNK = new MCNK();
            MCNK.Read(chunk.GetReader());

            Holes = MCNK.Flags.HasFlag(MCNK.MCNKFlags.HighResolutionHoles) ?
                HighResHoles : TransformToHighRes(MCNK.Holes);

            //stream.Seek(chunk.FindSubChunkOffset("MCVT") + 8, SeekOrigin.Begin);
            //MCVT = new MCVT();
            //MCVT.Read(chunk.GetReader());

            GenerateVertices();
        }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }
        public MCNK MCNK { get; private set; }
        public MCVT MCVT { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public List<Triangle<uint>> Triangles { get; private set; }

        // Credit goes to Bananenbrot for this
        // http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/409718-navmesh-mpq-geometry-parsing-issues-3.html#post2787749
        private static byte[] TransformToHighRes(ushort holes)
        {
            var ret = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int holeIdxL = (i / 2) * 4 + (j / 2);
                    if (((holes >> holeIdxL) & 1) == 1)
                        ret[i] |= (byte)(1 << j);
                }
            }
            return ret;
        }

        private byte[] HighResHoles
        {
            get { return BitConverter.GetBytes(MCNK.ofsMCVT + ((ulong)MCNK.ofsMCNR << 32)); }
        }

        public byte[] Holes { get; private set; }

        public bool HasHole(int col, int row)
        {
            return ((Holes[row] >> col) & 1) == 1;
        }

        public int Index
        {
            get { return (int)(MCNK.IndexX + (16 * MCNK.IndexY)); }
        }

        public Vector3[] RecastVertices;
        private void GenerateVertices()
        {
            Vertices = new Vector3[145];

            var stream = Chunk.GetStream();
            stream.Seek(Chunk.FindSubChunkOffset("MCVT") + 8, SeekOrigin.Begin); // +8 to skip name+size
            var br = Chunk.GetReader();

            // This one is correct
            int idx = 0;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var vertex = new Vector3()
                    {
                        X = MCNK.Position.X - (i * Constants.UnitSize),
                        Y = MCNK.Position.Y - (j * Constants.UnitSize),
                        Z = MCNK.Position.Z + br.ReadSingle()
                    };
                    Vertices[idx++] = vertex;
                }

                if (i < 8)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        var vertex = new Vector3()
                        {
                            X = MCNK.Position.X - (i * Constants.UnitSize) - (Constants.UnitSize / 2.0f),
                            Y = MCNK.Position.Y - (j * Constants.UnitSize) - (Constants.UnitSize / 2.0f),
                            Z = MCNK.Position.Z + br.ReadSingle()
                        };
                        Vertices[idx++] = vertex;
                    }
                }
            }

            // This one is for recast

            //var cRow = Index / 16;
            //var cCol = Index % 16;
            //MCNK.Position = new Vector3(Constants.ChunkSize * cCol, MCNK.Position.Z, Constants.ChunkSize * cRow);

            //int idx = 0;
            //for (int i = 0; i < 9; i++)
            //{
            //    for (int j = 0; j < 9; j++)
            //    {
            //        var vertex = new Vector3()
            //        {
            //            X = MCNK.Position.X + (j * Constants.UnitSize),
            //            Y = MCNK.Position.Y + br.ReadSingle(),
            //            Z = MCNK.Position.Z + (i * Constants.UnitSize),
            //        };
            //        Vertices[idx++] = vertex;
            //    }

            //    if (i < 8)
            //    {
            //        for (int j = 0; j < 8; j++)
            //        {
            //            var vertex = new Vector3()
            //            {
            //                X = MCNK.Position.X + (j * Constants.UnitSize) + (Constants.UnitSize / 2.0f),
            //                Y = MCNK.Position.Y + br.ReadSingle(),
            //                Z = MCNK.Position.Z + (i * Constants.UnitSize) + (Constants.UnitSize / 2.0f),
            //            };
            //            Vertices[idx++] = vertex;
            //        }
            //    }
            //}
        }

        public void GenerateTriangles()
        {
            Triangles = new List<Triangle<uint>>(64 * 4);
            //for (int y = 0; y < 8; y++)
            //{
            //    for (int x = 0; x < 8; x++)
            //    {
            //        if (HasHole(x, y)) continue;

            //        var topLeft = (byte)((17 * y) + x);
            //        var topRight = (byte)((17 * y) + x + 1);
            //        var bottomLeft = (byte)((17 * (y + 1)) + x);
            //        var bottomRight = (byte)((17 * (y + 1)) + x + 1);
            //        var center = (byte)((17 * y) + 9 + x);

            //        var triangleType = TriangleType.Terrain;
            //        if (ADT.Liquid != null && ADT.Liquid.HeightMaps != null)
            //        {
            //            var data = ADT.Liquid.HeightMaps[Index];
            //            var maxHeight = Math.Max(Math.Max(Math.Max(Math.Max(Vertices[topLeft].Z, Vertices[topRight].Z), Vertices[bottomLeft].Z), Vertices[bottomRight].Z), Vertices[center].Z);
            //            if (data != null && data.IsWater(x, y, maxHeight))
            //                triangleType = TriangleType.Water;
            //        }

            //        Triangles.Add(new Triangle<byte>(triangleType, topRight, topLeft, center));
            //        Triangles.Add(new Triangle<byte>(triangleType, topLeft, bottomLeft, center));
            //        Triangles.Add(new Triangle<byte>(triangleType, bottomLeft, bottomRight, center));
            //        Triangles.Add(new Triangle<byte>(triangleType, bottomRight, topRight, center));
            //    }
            //}

            for (uint j = 9; j < 8 * 8 + 9 * 8; ++j)
            {
                Triangles.Add(new Triangle<uint>(TriangleType.Terrain, j, j - 9, j + 8));
                Triangles.Add(new Triangle<uint>(TriangleType.Terrain, j, j - 8, j - 9));
                Triangles.Add(new Triangle<uint>(TriangleType.Terrain, j, j + 9, j - 8));
                Triangles.Add(new Triangle<uint>(TriangleType.Terrain, j, j + 8, j + 9));
                if ((j + 1) % (9 + 8) == 0)
                    j += 9;
            }
        }
    }
}
