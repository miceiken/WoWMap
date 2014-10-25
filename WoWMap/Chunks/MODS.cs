using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;
using WoWMap.Geometry;

namespace WoWMap.Chunks
{
    public class MODS : ChunkReader
    {
        public MODS(Chunk c) : base(c, c.Size) { }
        public MODS(Chunk c, uint h) : base(c, h) { }

        public string SetName;
        public uint FirstInstanceIndex;
        public uint nDoodads;
        private uint unk0;

        public override void Read()
        {
            var br = Chunk.GetReader();

            SetName = Encoding.ASCII.GetString(br.ReadBytes(20));
            FirstInstanceIndex = br.ReadUInt32();
            nDoodads = br.ReadUInt32();
            unk0 = br.ReadUInt32();
        }
    }
}
