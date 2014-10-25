using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;
using WoWMap.Geometry;

namespace WoWMap.Layers
{
    public class MapChunk
    {
        public MapChunk(ADT adt, Chunk chunk)
        {
            ADT = adt;
            Chunk = chunk;

            MCNK = new MCNK(chunk);
            //SubData = new ChunkData(chunk.GetStream());
            FindSubChunks();

            Holes = MCNK.Flags.HasFlag(MCNK.MCNKFlags.HighResolutionHoles) ?
                HighResHoles : TransformToHighRes(MCNK.Holes);

            GenerateVertices();
        }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }
        public ChunkData SubData { get; private set; }
        public MCNK MCNK { get; private set; }
        public MCVT MCVT { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public List<Triangle<uint>> Indices { get; private set; }

        public int Index
        {
            get { return (int)(MCNK.IndexX + (16 * MCNK.IndexY)); }
        }

        public void FindSubChunks()
        {
            var stream = Chunk.GetStream();
            var reader = new BinaryReader(stream);

            var offset = Chunk.Offset + MCNK.ChunkHeaderSize;
            while (offset < (Chunk.Offset + Chunk.Size))
            {
                stream.Seek(offset, SeekOrigin.Begin);

                var subChunkHeader = new ChunkHeader(reader);
                var subchunk = new Chunk(subChunkHeader, stream);
                switch (subChunkHeader.Name)
                {
                    case "MCVT":
                        MCVT = new MCVT(subchunk);
                        break;
                    case "MCRD": // TODO: implement http://pxr.dk/wowdev/wiki/index.php?title=Cataclysm#MCRD_.28optional.29
                        break;
                    case "MCRW": // TODO: implement
                        break;

                    case "MCLV":
                    case "MCCV":
                    case "MCNR":
                    case "MCLY":
                    case "MCRF":
                    case "MCAL":
                    case "MCLQ":
                    case "MCSE":
                        break;
                }

                offset += 8 + subChunkHeader.Size;
            }
        }

        #region Holes

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

        #endregion

        private void GenerateVertices()
        {
            if (MCVT == null) return;

            Vertices = new Vector3[145];

            var relPos = new Vector3(Constants.MaxXY - MCNK.Position.Y, Constants.MaxXY - MCNK.Position.X, MCNK.Position.Z);
            for (int i = 0, idx = 0; i < 17; i++)
            {
                for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                {
                    var v = new Vector3(relPos.X + j * Constants.UnitSize, relPos.Y + i * Constants.UnitSize * 0.5f, MCVT.Heights[idx] + relPos.Z);
                    if ((i % 2) != 0) v.X += 0.5f * Constants.UnitSize;
                    Vertices[idx++] = v;
                }
            }
        }

        public void GenerateIndices()
        {
            if (MCVT == null || Vertices == null || Vertices.Count() == 0) return;

            Indices = new List<Triangle<uint>>(64 * 4);

            //        var triangleType = TriangleType.Terrain;
            //        if (ADT.Liquid != null && ADT.Liquid.HeightMaps != null)
            //        {
            //            var data = ADT.Liquid.HeightMaps[Index];
            //            var maxHeight = Math.Max(Math.Max(Math.Max(Math.Max(Vertices[topLeft].Z, Vertices[topRight].Z), Vertices[bottomLeft].Z), Vertices[bottomRight].Z), Vertices[center].Z);
            //            if (data != null && data.IsWater(x, y, maxHeight))
            //                triangleType = TriangleType.Water;
            //        }

            // TODO: Check for liquid - or keep a seperate liquid mesh
            int unitidx = 0;
            for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
            {
                if (!HasHole(unitidx % 8, unitidx++ / 8))
                {
                    Indices.Add(new Triangle<uint>(TriangleType.Terrain, j, j - 9, j + 8));
                    Indices.Add(new Triangle<uint>(TriangleType.Terrain, j, j - 8, j - 9));
                    Indices.Add(new Triangle<uint>(TriangleType.Terrain, j, j + 9, j - 8));
                    Indices.Add(new Triangle<uint>(TriangleType.Terrain, j, j + 8, j + 9));
                }
                if ((j + 1) % (9 + 8) == 0) j += 9;
            }
        }
    }
}
