using System;
using System.Collections.Generic;
using WoWMap.Chunks;
using OpenTK;

namespace WoWMap.Layers
{
    public class ADT
    {
        public RootADT Root { get; private set; }
        public TexturesADT Textures { get; private set; }
        public ObjectsADT Objects { get; private set; }

        public Vector2 TilePosition { get { return new Vector2((32 - X) * Constants.TileSize, (32 - Y) * Constants.TileSize); } }

        public string Filename { get; private set; }

        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        // Meh. Not ideal. Whatever.
        public MVER MVER { get; set; }
        public MAMP MAMP { get; set; }
        public MHDR MHDR { get; set; }
        public MMDX MMDX { get; set; }
        public MMID MMID { get; set; }
        public MWID MWID { get; set; }
        public MWMO MWMO { get; set; }
        public MDDF MDDF { get; set; }
        public MODF MODF { get; set; }
        public MTEX MTEX { get; set; }

        public LiquidChunk Liquid { get; set; }
        public List<MapChunk> MapChunks { get; set; }

        public List<string> DoodadPaths { get; private set; }
        public List<string> ModelPaths { get; private set; }

        public WDT WDT { get; private set; }

        public bool Finalized { get; private set; }

        public ADT(string fileName, WDT wdtFile)
        {
            Filename = fileName;
            Finalized = false;
            WDT = wdtFile;

            Root = new RootADT(fileName, wdtFile);
            Textures = new TexturesADT(fileName, wdtFile);
            Objects = new ObjectsADT(fileName, wdtFile);

            MapChunks = new List<MapChunk>(256);
        }

        public ADT(string mapName, int x, int y, WDT wdtFile = null)
            : this(string.Format(@"World\Maps\{0}\{0}_{1}_{2}", mapName, x, y), wdtFile)
        {
            World = mapName;
            X = x;
            Y = y;
        }

        public void AddMapChunk(Chunk subChunk)
        {
            MapChunks.Add(new MapChunk(this, subChunk));
        }

        public void UpdateMapChunk(Chunk subChunk, int mapChunkIndex)
        {
            MapChunks[mapChunkIndex].Merge(subChunk);
        }

        public void Read()
        {
            if (Finalized)
                return;

            Root.Read(this);
            Textures.Read(this);
            Objects.Read(this);

            ReadModels();
            ReadDoodads();

            Finalized = true;
        }

        private void ReadModels()
        {
            if (MWID == null || MWMO == null)
                return;

            ModelPaths = new List<string>(MWMO.Filenames.Count);
            foreach (var t in MWID.Offsets)
                ModelPaths.Add(MWMO.Filenames[t]);
        }

        private void ReadDoodads()
        {
            if (MMID == null || MMDX == null)
                return;

            DoodadPaths = new List<string>(MMDX.Filenames.Count);
            foreach (var t in MMID.Offsets)
                DoodadPaths.Add(MMDX.Filenames[t]);
        }

        public void Generate()
        {
            foreach (var mc in MapChunks)
                mc.Generate();
        }
    }

    /*public class ADT
    {
        public string Filename { get; private set; }

        public ADTType Type { get; private set; }

        public WDT WDT { get; private set; }
        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public Vector3 TilePosition { get { return new Vector3((32 - X) * Constants.TileSize, (32 - Y) * Constants.TileSize, 0); } }

        private ADT(string filename, ADTType type, WDT wdt = null)
        {
            switch (type)
            {
                case ADTType.Normal: filename += ".adt"; break;
                case ADTType.Objects: filename += "_obj0.adt"; break;
                case ADTType.Textures: filename += "_tex0.adt"; break;
            }

            WDT = wdt;

            Filename = filename;
            Data = new ChunkData(filename);
            Type = type;
        }

        public ADT(string world, int x, int y, ADTType type = ADTType.Normal, WDT wdt = null)
            : this(string.Format(@"World\Maps\{0}\{0}_{1}_{2}", world, x, y), type)
        {
            WDT = wdt;
            World = world;
            X = x;
            Y = y;
        }

        public ChunkData Data { get; private set; }

        public ADT Objects { get; private set; }         // _obj0.adt - Contains information about world objects
        public ADT Textures { get; private set; }      // _tex0.adt - Contains information about world textures

        public MapChunk[] MapChunks { get; private set; }
        public LiquidChunk Liquid { get; private set; }

        public MAMP MAMP { get; private set; }
        public MVER MVER { get; private set; }      // Version
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
                if (WDT == null)
                    WDT = new WDT(string.Format(@"World\Maps\{0}\{0}.wdt", World));

                Objects = new ADT(World, X, Y, ADTType.Objects, WDT);
                Objects.Read();

                Textures = new ADT(World, X, Y, ADTType.Textures, WDT);
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
                    case "MVER":
                        MVER = new MVER(subChunk);
                        break;
                    case "MAMP":
                        MAMP = new MAMP(subChunk);
                        break;
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
            //Textures.Generate();
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
    }*/
}
