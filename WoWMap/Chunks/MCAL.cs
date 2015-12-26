using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    class MCAL: ChunkReader
    {
        public MCAL(MapChunk chunk, WDT wdt, Chunk c) : base(c)
        {
            _mapChunk = chunk;
            _wdt = wdt;
        }

        private MapChunk _mapChunk;
        private WDT _wdt;

        public MCAL(Chunk c, uint h) : base(c, h) { }
        public MCAL(Chunk c) : base(c, c.Size) { }

        private byte[] alphaMap;

        public override void Read()
        {
            var br = Chunk.GetReader();
            // Ah, piss.
        }
    }
}
