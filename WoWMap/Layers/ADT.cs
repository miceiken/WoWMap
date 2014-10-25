using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;
using WoWMap.Geometry;

namespace WoWMap.Layers
{
    public class ADT
    {
        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public ADT(string filename)
        {
            Data = new ChunkData(filename);
        }

        public ADT(string world, int x, int y)
            : this(string.Format(@"World\Maps\{0}\{0}_{1}_{2}.adt", world, x, y))
        {
            World = world;
            X = x;
            Y = y;
        }

        public ChunkData Data { get; private set; }

        public MapChunk[] MapChunks { get; private set; }
        public LiquidChunk Liquid { get; private set; }

        public MHDR MHDR { get; private set; }
        public MMDX MMDX { get; private set; }
        public MMID MMID { get; private set; }
        public MWMO MWMO { get; private set; }
        public MWID MWID { get; private set; }
        public MDDF MDDF { get; private set; }
        public MODF MODF { get; private set; }

        public List<string> DoodadPaths { get; private set; }
        public List<string> ModelPaths { get; private set; }

        public void Read()
        {
            MapChunks = new MapChunk[16 * 16];
            int mcIdx = 0;

            foreach (var subChunk in Data.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MHDR":
                        MHDR = new MHDR(subChunk);
                        break;
                    case "MMDX":
                        MMDX = new MMDX(subChunk);
                        break;
                    case "MMID":
                        MMID = new MMID(subChunk);
                        break;
                    case "MWMO":
                        MWMO = new MWMO(subChunk);
                        break;
                    case "MWID":
                        MWID = new MWID(subChunk);
                        break;
                    case "MDDF":
                        MDDF = new MDDF(subChunk);
                        break;
                    case "MODF":
                        MODF = new MODF(subChunk);
                        break;
                    case "MH2O":
                        Liquid = new LiquidChunk(this, subChunk);
                        break;
                    case "MCNK":
                        MapChunks[mcIdx++] = new MapChunk(this, subChunk);
                        break;
                }
            }

            foreach (var mapChunk in MapChunks)
                mapChunk.GenerateIndices();

            ReadModels();
            ReadDoodads();
        }

        private void ReadModels()
        {
            if ((MWID == null || MWID.Offsets.Count() == 0) || (MWMO == null || MWMO.Filenames.Count() == 0))
                return;

            ModelPaths = new List<string>();
            for (int i = 0; i < MWID.Offsets.Length; i++)
                ModelPaths.Add(MWMO.Filenames[MWID.Offsets[i] / 4]);
        }

        private void ReadDoodads()
        {
            if ((MMID == null || MMID.Offsets.Count() == 0) || (MMDX == null || MMDX.Filenames.Count() == 0))
                return;

            DoodadPaths = new List<string>();
            for (int i = 0; i < MMID.Offsets.Length; i++)
                DoodadPaths.Add(MMDX.Filenames[MMID.Offsets[i] / 4]);
        }

        public void SaveObj(string filename = null)
        {
            if (filename == null)
                filename = string.Format("{0}_{1}_{2}.obj", World, X, Y);
            var vertices = new List<Vector3>();
            var triangles = new List<Triangle<uint>>();

            foreach (var mapChunk in MapChunks)
            {
                var vo = (uint)vertices.Count;
                vertices.AddRange(mapChunk.Vertices);
                triangles.AddRange(mapChunk.Indices.Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            }

            using (var sw = new StreamWriter(filename, false))
            {
                sw.WriteLine("o " + filename);
                var nf = CultureInfo.InvariantCulture.NumberFormat;
                foreach (var v in vertices)
                    sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Z.ToString(nf) + " " + v.Y.ToString(nf));
                foreach (var t in triangles)
                    sw.WriteLine("f " + (t.V0 + 1) + " " + (t.V1 + 1) + " " + (t.V2 + 1));
            }
        }
    }
}
