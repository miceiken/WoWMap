using System;
using System.Collections.Generic;
using WoWMap.Chunks;
using OpenTK;

namespace WoWMap.Layers
{
    public enum ADTType { Normal, Objects, Textures };
    public class ADT
    {
        public string Filename { get; private set; }

        public ADTType Type { get; private set; }

        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public Vector3 TilePosition { get { return new Vector3((32 - X) * Constants.TileSize, (32 - Y) * Constants.TileSize, 0); } }

        private ADT(string filename, ADTType type)
        {
            switch (type)
            {
                case ADTType.Normal: filename += ".adt"; break;
                case ADTType.Objects: filename += "_obj0.adt"; break;
                case ADTType.Textures: filename += "_tex0.adt"; break;
            }

            Filename = filename;
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

        public ADT Objects { get; private set; }         // _obj0.adt - Contains information about world objects
        public ADT Textures { get; private set; }      // _tex0.adt - Contains information about world textures

        public MapChunk[] MapChunks { get; private set; }
        public LiquidChunk Liquid { get; private set; }

        public MHDR MHDR { get; private set; }      // Header
        public MMDX MMDX { get; private set; }      // Filenames for doodads
        public MMID MMID { get; private set; }      // Offsets for doodad filenames
        public MWMO MWMO { get; private set; }      // Filenames for WMOs
        public MWID MWID { get; private set; }      // Offsets for WMO filenames
        public MDDF MDDF { get; private set; }      // Placement information for doodads
        public MODF MODF { get; private set; }      // Placement information for WMOs
        public MTEX MTEX { get; private set; }      // Filenames for textures

        public List<string> DoodadPaths { get; private set; }
        public List<string> ModelPaths { get; private set; }

        public void Read()
        {
            if (Type == ADTType.Normal)
            {
                Objects = new ADT(World, X, Y, ADTType.Objects);
                Objects.Read();

                Textures = new ADT(World, X, Y, ADTType.Textures);
                Textures.Read();
            }

            Console.WriteLine("* Reading {0} Type ADT.", Type);

            MapChunks = new MapChunk[16 * 16];
            var mcIdx = 0;

            foreach (var subChunk in Data.Chunks)
            {
                if (subChunk.Name != "MCNK")
                    Console.WriteLine("Found {0} chunk", subChunk.Name);

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
                    case "MTEX":
                        MTEX = new MTEX(subChunk);
                        break;
                    case "MH2O":
                        Liquid = new LiquidChunk(this, subChunk);
                        break;
                    case "MCNK":
                        MapChunks[mcIdx++] = new MapChunk(this, subChunk);
                        break;
                }
            }

            ReadModels();
            ReadDoodads();
        }

        public void Generate()
        {
            foreach (var mc in MapChunks)
                mc.Generate();

            if (Type != ADTType.Normal)
                return;
            Objects.Generate();
            Textures.Generate();
        }

        private void ReadModels()
        {
            if (Type != ADTType.Objects || (MWID == null || MWMO == null))
                return;

            ModelPaths = new List<string>(MWMO.Filenames.Count);
            foreach (var t in MWID.Offsets)
                ModelPaths.Add(MWMO.Filenames[t]);
        }

        private void ReadDoodads()
        {
            if (Type != ADTType.Objects || (MMID == null || MMDX == null))
                return;

            DoodadPaths = new List<string>(MMDX.Filenames.Count);
            foreach (var t in MMID.Offsets)
                DoodadPaths.Add(MMDX.Filenames[t]);
        }
    }
}
