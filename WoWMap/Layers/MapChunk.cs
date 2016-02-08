using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WoWMap.Chunks;
using WoWMap.Geometry;
using OpenTK;
using System.Diagnostics;

namespace WoWMap.Layers
{
    public class MapChunk
    {
        public MapChunk(ADT adt, Chunk chunk, bool isObj0 = false)
        {
            ADT = adt;
            Chunk = chunk;

            MCNK = new MCNK(chunk);
            Holes = MCNK.Flags.HasFlag(MCNK.MCNKFlags.HighResolutionHoles) ? HighResHoles : TransformToHighRes(MCNK.Holes);

            var stream = chunk.GetStream();
            stream.Seek(chunk.Offset + MCNK.ChunkHeaderSize, SeekOrigin.Begin);

            Read(new ChunkData(stream, chunk.Size - MCNK.ChunkHeaderSize));
        }

        public void Merge(Chunk chunk)
        {
            Read(new ChunkData(chunk.GetStream(), chunk.Size));
        }

        public WDT WDT { get { return ADT.WDT; } }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }

        public MCNK MCNK { get; private set; }
        public MCVT MCVT { get; private set; }
        public MCAL MCAL { get; private set; }
        public MCRD MCRD { get; private set; }
        public MCRW MCRW { get; private set; }
        public MCNR MCNR { get; private set; }
        public MCCV MCCV { get; private set; }
        public MCSH MCSH { get; private set; }
        public MCLY MCLY { get; private set; }

        public Vector3[] Vertices { get; private set; }
        public List<Triangle<uint>> Indices { get; private set; }

        public List<Vector3> DoodadVertices;
        public List<Triangle<uint>> DoodadIndices;
        public List<Vector3> DoodadNormals;

        public List<Vector3> WMOVertices;
        public List<Triangle<uint>> WMOIndices;
        public List<Vector3> WMONormals;

        public int Index
        {
            get { return (int)(MCNK.IndexX + (16 * MCNK.IndexY)); }
        }

        #region Holes

        // Credit goes to Bananenbrot for this
        // http://www.ownedcore.com/forums/world-of-warcraft/world-of-warcraft-bots-programs/wow-memory-editing/409718-navmesh-mpq-geometry-parsing-issues-3.html#post2787749
        private static byte[] TransformToHighRes(ushort holes)
        {
            var ret = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var holeIdxL = (i / 2) * 4 + (j / 2);
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

        private void Read(ChunkData subData)
        {
            foreach (var subChunk in subData.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MCNK":
                        break; // Ignore
                    case "MCVT":
                        MCVT = new MCVT(subChunk);
                        break;
                    case "MCRD":
                        MCRD = new MCRD(subChunk);
                        break;
                    case "MCRW":
                        MCRW = new MCRW(subChunk);
                        break;
                    case "MCNR":
                        MCNR = new MCNR(subChunk);
                        break;
                    case "MCCV":
                        MCCV = new MCCV(subChunk);
                        break;
                    //case "MCSH":
                    //    MCSH = new MCSH(subChunk);
                    //    break;
                    //case "MCLY":
                    //    MCLY = new MCLY(subChunk);
                    //    break;
                    //case "MCAL":
                    //    if (WDT == null)
                    //        Console.WriteLine($"Skipping MCAL Chunk in MCNK #{Index} because no WDT was provided!");
                    //    else
                    //        MCAL = new MCAL(this, WDT, subChunk);
                    //    break;
                    //default:
                    //    Console.WriteLine($"Skipped {subChunk.Name} MapChunk sub-chunk.");
                    //    break;
                }
            }
        }

        public void Generate()
        {
            if (MCVT != null)
            {
                GenerateVertices();
                GenerateIndices();
            }

            if (MCRW != null)
                GenerateWMOs(ref WMOVertices, ref WMONormals, ref WMOIndices);

            if (MCRD != null)
                GenerateDoodads(ref DoodadVertices, ref DoodadNormals, ref DoodadIndices);
        }

        #region MCVT - HeightMap

        private void GenerateVertices()
        {
            Vertices = new Vector3[145];

            for (int i = 0, idx = 0; i < 17; i++)
            {
                for (var j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                {
                    var v = new Vector3(MCNK.Position.X - (i * Constants.UnitSize * 0.5f), MCNK.Position.Y - (j * Constants.UnitSize), MCVT.Heights[idx] + MCNK.Position.Z);
                    if ((i % 2) != 0) v.Y -= 0.5f * Constants.UnitSize;
                    Vertices[idx++] = v;
                }
            }
        }

        private void GenerateIndices()
        {
            Indices = new List<Triangle<uint>>(64 * 4);

            var unitidx = 0;
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

        #endregion

        #region MCRW/MODF - WMOs

        public void GenerateWMOs(ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Triangle<uint>> indices)
        {
            if (ADT.MODF == null)
                return;

            var drawn = new HashSet<uint>();
            for (var i = 0; i < MCRW.MODFEntryIndex.Length; i++)
            {
                var wmo = ADT.MODF.Entries[MCRW.MODFEntryIndex[i]];
                if (drawn.Contains(wmo.UniqueId))
                    continue;
                drawn.Add(wmo.UniqueId);

                if (wmo.MWIDEntryIndex >= ADT.ModelPaths.Count)
                    continue;

                var path = ADT.ModelPaths[(int)wmo.MWIDEntryIndex];
                var model = new WMORoot(path);

                if (vertices == null)
                    vertices = new List<Vector3>(1000);
                if (indices == null)
                    indices = new List<Triangle<uint>>(1000);
                if (normals == null)
                    normals = new List<Vector3>(1000);

                InsertWMOGeometry(wmo, model, ref vertices, ref indices, ref normals);
            }
        }

        public static void InsertWMOGeometry(MODF.MODFEntry wmo, WMORoot model, ref List<Vector3> vertices, ref List<Triangle<uint>> indices, ref List<Vector3> normals)
        {
            var transform = Transformation.GetWMOTransform(wmo.Position, wmo.Rotation);
            foreach (var group in model.Groups)
            {
                var vo = (uint)vertices.Count;
                vertices.AddRange(group.MOVT.Vertices.Select(v => Vector3.Transform(v, transform)));
                normals.AddRange(group.MONR.Normals.Select(v => Vector3.Transform(v, transform)));

                // No. Makes this thing unreadable.
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < group.MOVI.Indices.Length; i++)
                {
                    if (((byte)group.MOPY.Entries[i].Flags & 0x04) != 0 && group.MOPY.Entries[i].MaterialId != 0xFF)
                        continue;

                    var idx = group.MOVI.Indices[i];
                    indices.Add(new Triangle<uint>(TriangleType.Wmo, vo + idx.V0, vo + idx.V1, vo + idx.V2));
                }
            }

            if (wmo.DoodadSet < model.MODS.Entries.Length)
            {
                var set = model.MODS.Entries[wmo.DoodadSet];
                var instances = new List<MODD.MODDEntry>((int)set.nDoodads);
                for (var i = set.FirstInstanceIndex; i < (set.nDoodads + set.FirstInstanceIndex); i++)
                {
                    if (i >= model.MODD.Entries.Length)
                        break;
                    instances.Add(model.MODD.Entries[(int)i]);
                }

                foreach (var instance in instances)
                {
                    string path;
                    if (!model.MODN.Filenames.TryGetValue(instance.ofsMODN, out path))
                        continue;

                    var doodad = new M2(path);
                    if (!doodad.IsCollidable)
                        continue;

                    var doodadTransform = Transformation.GetDoodadTransform(instance, wmo);
                    var vo = (uint)vertices.Count;

                    vertices.AddRange(doodad.Vertices.Select(vertex => Vector3.Transform(vertex, doodadTransform)));
                    normals.AddRange(doodad.Normals.Select(normal => Vector3.Transform(normal, doodadTransform)));
                    indices.AddRange(doodad.Indices.Select(t => new Triangle<uint>(TriangleType.Doodad, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
                }
            }

            foreach (var group in model.Groups)
            {
                if ((group.LiquidVertices == null || group.LiquidVertices.Count == 0) || (group.LiquidIndices == null || group.LiquidIndices.Count == 0))
                    continue;

                var vo = (uint)vertices.Count;
                vertices.AddRange(@group.LiquidVertices.Select(v => Vector3.Transform(v, transform)));
                indices.AddRange(@group.LiquidIndices.Select(t => new Triangle<uint>(t.Type, t.V1 + vo, t.V0 + vo, t.V2 + vo)));
            }
        }

        #endregion

        #region MCRD/MDDF - Doodads

        public void GenerateDoodads(ref List<Vector3> vertices, ref List<Vector3> normals, ref List<Triangle<uint>> indices)
        {
            if (ADT.MDDF == null)
                return;

            var drawn = new HashSet<uint>();
            for (var i = 0; i < MCRD.MDDFEntryIndex.Length; i++)
            {
                var doodad = ADT.MDDF.Entries[MCRD.MDDFEntryIndex[i]];
                if (drawn.Contains(doodad.UniqueId))
                    continue;
                drawn.Add(doodad.UniqueId);

                if (doodad.MMIDEntryIndex >= ADT.DoodadPaths.Count)
                    continue;

                var path = ADT.DoodadPaths[(int)doodad.MMIDEntryIndex];
                var model = new M2(path);

                if (!model.IsCollidable)
                    continue;

                if (vertices == null)
                    vertices = new List<Vector3>((MCRD.MDDFEntryIndex.Length / 4) * model.Vertices.Length);
                if (indices == null)
                    indices = new List<Triangle<uint>>((MCRD.MDDFEntryIndex.Length / 4) * model.Indices.Length);
                if (normals == null)
                    normals = new List<Vector3>(MCRD.MDDFEntryIndex.Length / 4 * model.Normals.Length);

                // Doodads outside WMOs are treated like WMOs. Not a typo.
                var transform = Transformation.GetWMOTransform(doodad.Position, doodad.Rotation, doodad.Scale / 1024.0f);
                var vo = (uint)vertices.Count;
                vertices.AddRange(model.Vertices.Select(v => Vector3.Transform(v, transform)));
                normals.AddRange(model.Normals.Select(v => Vector3.Transform(v, transform)));
                indices.AddRange(model.Indices.Select(t => new Triangle<uint>(TriangleType.Doodad, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            }
        }

        #endregion
    }
}
