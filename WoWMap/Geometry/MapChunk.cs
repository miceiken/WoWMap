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
            MCNK = new MCNK();
            MCNK.Read(chunk.GetReader());

            Holes = MCNK.Flags.HasFlag(MCNK.MCNKFlags.HighResolutionHoles) ?
                HighResHoles : TransformToHighRes(MCNK.Holes);

            var stream = chunk.GetStream();
            stream.Seek(MCNK.ofsMCVT, SeekOrigin.Current);
            MCVT = new MCVT();
            MCVT.Read(new BinaryReader(stream));

            GenerateVertices();
        }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }
        public MCNK MCNK { get; private set; }
        public MCVT MCVT { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public List<Triangle<byte>> Triangles { get; private set; }

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

        private void GenerateVertices()
        {
            Vertices = new Vector3[145];
            int idx = 0;
            for (int j = 0; j < 17; j++)
            {
                int values = (j % 2) != 0 ? 8 : 9;
                for (int i = 0; i < values; i++)
                {

                    var vertex = new Vector3()
                    {
                        X = MCNK.Position[0] - (j * Global.UnitSize * 0.5f),
                        Y = MCNK.Position[1] - (i * Global.UnitSize),
                        Z = MCNK.Position[2] + MCVT.Height[idx]
                    };

                    if (values == 0) vertex.Y -= Global.UnitSize * 0.5f;

                    Vertices[idx++] = vertex;
                }
            }
        }

        public void GenerateTriangles()
        {
            Triangles = new List<Triangle<byte>>(64 * 4);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (HasHole(x / 2, y / 2)) continue;

                    var topLeft = (byte)((17 * y) + x);
                    var topRight = (byte)((17 * y) + x + 1);
                    var bottomLeft = (byte)((17 * (y + 1)) + x);
                    var bottomRight = (byte)((17 * (y + 1)) + x + 1);
                    var center = (byte)((17 * y) + 9 + x);

                    var triangleType = TriangleType.Terrain;
                    if (ADT.Liquid != null && ADT.Liquid.HeightMaps != null)
                    {
                        var data = ADT.Liquid.HeightMaps[Index];
                        var maxHeight = Math.Max(Math.Max(Math.Max(Math.Max(Vertices[topLeft].Z, Vertices[topRight].Z), Vertices[bottomLeft].Z), Vertices[bottomRight].Z), Vertices[center].Z);
                        if (data != null && data.IsWater(x, y, maxHeight))
                            triangleType = TriangleType.Water;
                    }

                    Console.WriteLine("MapChunk #{0} [{1}, {2}]: Triangle is {3}", Index, x, y, triangleType);

                    Triangles.Add(new Triangle<byte>(triangleType, topRight, topLeft, center));
                    Triangles.Add(new Triangle<byte>(triangleType, topLeft, bottomLeft, center));
                    Triangles.Add(new Triangle<byte>(triangleType, bottomLeft, bottomRight, center));
                    Triangles.Add(new Triangle<byte>(triangleType, bottomRight, topRight, center));
                }
            }
        }
    }
}
