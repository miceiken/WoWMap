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
            MOHD = new MOHD(Data.GetChunkByName("MOHD"));
            MOGN = new MOGN(Data.GetChunkByName("MOGN"));
            MOGI = new MOGI(Data.GetChunkByName("MOGI"));
            MODD = new MODD(Data.GetChunkByName("MODD"));
            MODN = new MODN(Data.GetChunkByName("MODN"));
            MODS = new MODS(Data.GetChunkByName("MODS"));

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
