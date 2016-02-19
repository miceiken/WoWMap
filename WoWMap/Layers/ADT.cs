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
            MapChunks[mapChunkIndex].Merge(this, subChunk);
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
}
