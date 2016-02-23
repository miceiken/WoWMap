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

        public void Merge(ADT adt, Chunk chunk)
        {
            ADT = adt;
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

        public Scene Scene { get; private set; }

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
            Scene = new Scene()
            {
                Terrain = GenerateTerrain(),
                WorldModelObjects = GenerateWMOs(),
                Doodads = GenerateDoodads(),
                Liquids = GenerateLiquid(),
            };
        }

        #region MCVT - HeightMap

        private IEnumerable<Mesh> GenerateTerrain()
        {
            if (MCVT == null)
                yield break;

            var vertices = new Vector3[145];
            var normals = new Vector3[145];

            for (int i = 0, idx = 0; i < 17; i++)
            {
                for (var j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                {
                    var v = new Vector3(MCNK.Position.X - (i * Constants.UnitSize * 0.5f), MCNK.Position.Y - (j * Constants.UnitSize), MCVT.Heights[idx] + MCNK.Position.Z);
                    if ((i % 2) != 0) v.Y -= 0.5f * Constants.UnitSize;
                    normals[idx] = MCNR.Entries[idx].Normal;
                    vertices[idx++] = v;
                }
            }

            var indices = new List<uint>(64 * 4 * 3);

            var unitidx = 0;
            for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
            {
                if (!HasHole(unitidx % 8, unitidx++ / 8))
                {
                    indices.AddRange(new uint[] {
                        j, j - 9, j + 8,
                        j, j - 8, j - 9,
                        j, j + 9, j - 8,
                        j, j + 8, j + 9
                    });
                }
                if ((j + 1) % (9 + 8) == 0) j += 9;
            }

            yield return new Mesh
            {
                Type = MeshType.Terrain,
                Indices = indices.ToArray(),
                Vertices = vertices,
                Normals = normals,
            };
        }


        #endregion

        #region MCRW/MODF - WMOs

        public IEnumerable<WMOScene> GenerateWMOs()
        {
            if (MCRW == null || ADT.MODF == null)
                yield break;

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

                yield return GenerateWMOScene(wmo, model);
            }
        }

        public static WMOScene GenerateWMOScene(MODF.MODFEntry wmoDefinition, WMORoot model)
        {
            return new WMOScene
            {
                Terrain = model.Groups.Select(g => g.GenerateTerrain(wmoDefinition)).OfType<Mesh>() ?? Enumerable.Empty<Mesh>(),
                Doodads = model.GenerateDoodads(wmoDefinition.DoodadSet, wmoDefinition).OfType<Mesh>() ?? Enumerable.Empty<Mesh>(),
                Liquids = model.Groups.Select(g => g.GenerateLiquid(wmoDefinition)).OfType<Mesh>() ?? Enumerable.Empty<Mesh>(),
            };
        }

        #endregion

        #region MCRD/MDDF - Doodads

        public IEnumerable<Mesh> GenerateDoodads()
        {
            if (MCRD == null || ADT.MDDF == null)
                yield break;

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

                // Doodads outside WMOs are treated like WMOs. Not a typo.
                yield return model.Mesh.Transform(Transformation.GetWMOTransform(doodad.Position, doodad.Rotation, doodad.Scale / 1024.0f));
            }
        }

        #endregion

        #region MH2O - Liquid

        public IEnumerable<Mesh> GenerateLiquid()
        {
            if (ADT.Liquid?.HeightMaps[Index] == null)
                yield break;

            var information = ADT.Liquid.Information[Index];
            var heightMap = ADT.Liquid.HeightMaps[Index];

            var vertices = new List<Vector3>();
            var indices = new List<uint>();

            var basePos = MCNK.Position;
            for (int y = information.YOffset; y < (information.YOffset + information.Height); y++)
            {
                for (int x = information.XOffset; x < (information.XOffset + information.Width); x++)
                {
                    if (!heightMap.RenderMask.ShouldRender(x, y))
                        continue;

                    var v = new Vector3(basePos.X - (y * Constants.UnitSize), basePos.Y - (x * Constants.UnitSize), heightMap.Heightmap[x, y]);

                    vertices.AddRange(new[] { v,
                        new Vector3(v.X - Constants.UnitSize, v.Y, v.Z),
                        new Vector3(v.X, v.Y - Constants.UnitSize, v.Z),
                        new Vector3(v.X - Constants.UnitSize, v.Y - Constants.UnitSize, v.Z)
                    });

                    var vo = (uint)vertices.Count;

                    indices.AddRange(new uint[] {
                        vo, vo + 2, vo + 1,
                        vo + 2, vo + 3, vo + 1
                    });
                }
            }

            yield return new Mesh
            {
                Type = MeshType.Liquid,
                Indices = indices.ToArray(),
                Vertices = vertices.ToArray()
            };
        }

        #endregion
    }
}
