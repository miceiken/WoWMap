using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Archive;
using WoWMap.Chunks;
using WoWMap.Geometry;
using WoWMap.Readers;
using System.IO;

namespace WoWMap.Layers
{
    public class WMORoot
    {
        public WMORoot(string filename)
        {
            Filename = filename;
            Data = new ChunkData(filename);

            Read();
        }

        public string Filename { get; private set; }
        public ChunkData Data { get; private set; }

        public MOHD MOHD { get; private set; }
        public MOGN MOGN { get; private set; }
        public MOGI MOGI { get; private set; }
        public MODD MODD { get; private set; }
        public MODN MODN { get; private set; }
        public MODS MODS { get; private set; }

        public List<WMOGroup> Groups { get; private set; }

        public void Read()
        {
            foreach (var subChunk in Data.Chunks)
            {
                switch (subChunk.Name)
                {
                    case "MOHD":
                        MOHD = new MOHD(subChunk);
                        break;
                    case "MOGN":
                        MOGN = new MOGN(subChunk);
                        break;
                    case "MOGI":
                        MOGI = new MOGI(subChunk);
                        break;
                    case "MODD":
                        MODD = new MODD(subChunk);
                        break;
                    case "MODN":
                        MODN = new MODN(subChunk);
                        break;
                    case "MODS":
                        MODS = new MODS(subChunk);
                        break;
                }
            }

            ReadGroups();
        }

        public void ReadGroups()
        {
            if (MOHD == null || MOHD.nGroups == 0)
                return;

            var directory = Filename.Substring(0, Filename.LastIndexOf('.'));
            Groups = new List<WMOGroup>();
            for (int i = 0; i < MOHD.nGroups; i++)
            {
                try
                {
                    Groups.Add(new WMOGroup(string.Format("{0}_{1:000}.wmo", directory, i)));
                }
                catch (FileNotFoundException e) { }
            }
        }
    }
}
