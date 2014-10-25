using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Archive;
using WoWMap.Chunks;
using WoWMap.Geometry;

namespace WoWMap.Layers
{
    public class WMO
    {
        public WMO(ADT adt, Chunk chunk)
        {
            ADT = adt;
            Chunk = chunk;
            var stream = chunk.GetStream();
        }

        public ADT ADT { get; private set; }
        public Chunk Chunk { get; private set; }
        public MCNK MCNK { get; private set; }
    }
}
