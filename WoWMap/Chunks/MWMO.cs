using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MWMO
    {
        public string[] Filenames;

        public void Read(BinaryReader br, uint size)
        {
            var chunk = br.ReadBytes((int)size);
            Filenames = Helpers.SplitStrings(chunk).ToArray();
        }
    }
}
