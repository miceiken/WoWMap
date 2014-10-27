using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Chunks;
using WoWMap.Geometry;
using SharpDX;

namespace WoWMap.Layers
{
    public enum ADTType { Normal, Objects, Textures };
    public class ADT
    {
        public ADTType Type { get; private set; }

        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        private ADT(string filename, ADTType type)
        {
            switch (type)
            {
                case ADTType.Normal: filename += ".adt"; break;
                case ADTType.Objects: filename += "_obj0.adt"; break;
                case ADTType.Textures: filename += "_tex0.adt"; break;
            }

            Data = new ChunkData(filename);
            Type = type;
        }

        public ADT(string world, int x, int y, ADTType type = ADTType.Normal)
            : this(string.Format(@"World\Maps\{0}\{0}_{1}_{2}", world, x, y), type)
        {
            World = world;
            X = x;
            Y = y;
        }

        public ChunkData Data { get; private set; }

        public ADT ADTObjects { get; private set; }         // _obj0.adt - Contains information about world objects
        //public ADT ADTTextures { get; private set; }      // _tex0.adt - Contains information about world textures

        public MapChunk[] MapChunks { get; private set; }
        public LiquidChunk Liquid { get; private set; }

        public MHDR MHDR { get; private set; }      // Header
        public MMDX MMDX { get; private set; }      // Filenames for doodads
        public MMID MMID { get; private set; }      // Offsets for doodad filenames
        public MWMO MWMO { get; private set; }      // Filenames for WMOs
        public MWID MWID { get; private set; }      // Offsets for WMO filenames
        public MDDF MDDF { get; private set; }      // Placement information for doodads
        public MODF MODF { get; private set; }      // Placement information for WMOs

        public List<string> DoodadPaths { get; private set; }
        public List<string> ModelPaths { get; private set; }

        public void Read()
        {
            if (Type == ADTType.Normal)
            {
                ADTObjects = new ADT(World, X, Y, ADTType.Objects);
                ADTObjects.Read();

                //ADTTextures = new ADT(World, X, Y, ADTType.Textures);
                //ADTTextures.Read();
            }

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
                        ReadDoodads();
                        break;
                    case "MWMO":
                        MWMO = new MWMO(subChunk);
                        break;
                    case "MWID":
                        MWID = new MWID(subChunk);
                        ReadModels();
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
        }

        // TODO: This will probably go wrong one day
        private void ReadModels()
        {
            if (Type != ADTType.Objects || (MWID == null || MWMO == null))
                return;

            ModelPaths = new List<string>();
            for (int i = 0; i < MWID.Offsets.Length; i++)
                ModelPaths.Add(MWMO.Filenames[MWID.Offsets[i]]);
        }

        private void ReadDoodads()
        {
            if (Type != ADTType.Objects || (MMID == null || MMDX == null))
                return;

            DoodadPaths = new List<string>();
            for (int i = 0; i < MMID.Offsets.Length; i++)
                DoodadPaths.Add(MMDX.Filenames[MMID.Offsets[i]]);
        }

        public void SaveObj(string filename = null)
        {
            if (Type != ADTType.Normal)
                return;

            if (filename == null)
                filename = string.Format("{0}_{1}_{2}.obj", World, X, Y);
            var vertices = new List<Vector3>((256 * 145) + (256 * 2 * 1000));
            var triangles = new List<Triangle<uint>>((256 * 256) + (256 * 1000 * 4));

            //var sources = new ADT[] { this, ADTObjects, /* ADTTextures */ };
            //foreach (var source in sources)
            //{
            //    foreach (var mapChunk in source.MapChunks)
            //    {
            //        var subVertices = new IEnumerable<Vector3>[] { mapChunk.Vertices, mapChunk.WMOVertices, mapChunk.DoodadVertices };
            //        var subIndices = new IEnumerable<Triangle<uint>>[] { mapChunk.Indices, mapChunk.WMOIndices, mapChunk.DoodadIndices };
            //        for (int i = 0; i < 3; i++)
            //        {
            //            if (subVertices[i] == null || subIndices[i] == null)
            //                continue;

            //            var vo = (uint)vertices.Count;
            //            vertices.AddRange(subVertices[i]);
            //            triangles.AddRange(subIndices[i].Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            //        }
            //    }
            //}

            foreach (var mapChunk in MapChunks)
            {
                var vo = (uint)vertices.Count;
                vertices.AddRange(mapChunk.Vertices);
                triangles.AddRange(mapChunk.Indices.Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            }

            ////Rendering just tile WMOs
            //foreach (var wmoChunks in ADTObjects.MapChunks)
            //{
            //    if (wmoChunks.WMOVertices == null || wmoChunks.WMOIndices == null)
            //        continue;
            //    var vo = (uint)vertices.Count;
            //    vertices.AddRange(wmoChunks.WMOVertices);
            //    triangles.AddRange(wmoChunks.WMOIndices.Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            //}

            // //Rendering just tile Doodads
            //foreach (var doodadChunks in ADTObjects.MapChunks)
            //{
            //    if (doodadChunks.DoodadVertices == null || doodadChunks.DoodadIndices == null)
            //        continue;
            //    var vo = (uint)vertices.Count;
            //    vertices.AddRange(doodadChunks.DoodadVertices);
            //    triangles.AddRange(doodadChunks.DoodadIndices.Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            //}

            // Rendering just tile water

            if (Liquid != null && (Liquid.Vertices != null && Liquid.Indices != null))
            {
                var vo = (uint)vertices.Count;
                vertices.AddRange(Liquid.Vertices);
                triangles.AddRange(Liquid.Indices.Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
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
