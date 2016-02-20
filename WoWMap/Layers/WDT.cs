using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;
using WoWMap.Geometry;
using WoWMap.Archive;
using OpenTK;

namespace WoWMap.Layers
{
    public class WDT
    {
        public WDT(string filename)
        {
            Data = new ChunkData(filename);
            Read();
        }

        public ChunkData Data { get; private set; }

        public bool IsValid { get; private set; }
        public bool[,] TileTable { get; private set; }
        public MAIN MAIN { get; private set; }

        public MPHD MPHD { get; private set; }

        public bool IsGlobalModel { get; private set; }
        public MWMO MWMO { get; private set; }
        public MODF MODF { get; private set; }

        public string ModelFile { get; private set; }
        public WMORoot GlobalModel { get; private set; }

        public List<Vector3> ModelVertices { get; private set; }
        public List<Triangle<uint>> ModelIndices { get; private set; }
        public List<Vector3> ModelNormals { get; private set; }

        public bool HasTile(int x, int y)
        {
            if (x < 0 || x > 64 || y < 0 || y > 64) return false;
            return TileTable[x, y];
        }

        public int TileCount
        {
            get
            {
                var c = 0;
                for (var y = 0; y < 64; y++)
                    for (var x = 0; x < 64; x++)
                        if (TileTable[x, y])
                            ++c;
                return c;
            }
        }

        public void Read()
        {
            foreach (var subChunk in Data.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MAIN":
                        MAIN = new MAIN(subChunk);

                        IsValid = true;

                        TileTable = new bool[64, 64];
                        for (var y = 0; y < 64; y++)
                            for (var x = 0; x < 64; x++)
                                TileTable[x, y] = MAIN.Entries[x, y].Flags.HasFlag(MAIN.MAINFlags.HasADT);
                        break;

                    case "MWMO":
                        MWMO = new MWMO(subChunk);
                        break;

                    case "MODF":
                        MODF = new MODF(subChunk);
                        break;

                    case "MPHD":
                        MPHD = new MPHD(subChunk);
                        break;
                }
            }

            IsGlobalModel = (MODF != null && MWMO != null);
        }

        public void GenerateGlobalModel()
        {
            // TODO: exceptions

            // According to wiki this MWMO chunk only contains one zero-terminated string
            ModelFile = MWMO.Filenames.FirstOrDefault().Value;
            if (string.IsNullOrEmpty(ModelFile)) return;
            GlobalModel = new WMORoot(ModelFile);
            if (GlobalModel == null) return;
            var placementEntry = MODF.Entries.FirstOrDefault();
            if (placementEntry == null) return;

            var verts = new List<Vector3>();
            var norms = new List<Vector3>();
            var inds = new List<Triangle<uint>>();

            MapChunk.InsertWMOGeometry(placementEntry, GlobalModel, ref verts, ref inds, ref norms);

            // Global model WMO's come out wrong, rotate
            ModelVertices = verts.Select(v => Vector3.Transform(v, Matrix4.CreateRotationX((float)(-Math.PI / 2.0f)))).ToList();
            ModelIndices = inds;
            ModelNormals = norms;
        }
    }
}
