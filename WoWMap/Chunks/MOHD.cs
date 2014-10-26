using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using WoWMap.Readers;
using SharpDX;

namespace WoWMap.Chunks
{
    public class MOHD : ChunkReader
    {
        public MOHD(Chunk c, uint h) : base(c, h) { }
        public MOHD(Chunk c) : base(c, c.Size) { }

        public uint nTextures;
        public uint nGroups;
        public uint nPortals;
        public uint nLights;
        public uint nDoodadNames;
        public uint nDoodadDefs;
        public uint nDoodadSets;
        public byte colR;
        public byte colG;
        public byte colB;
        public byte colX;
        public uint WmoId;
        public Vector3[] BoundingBox;
        public uint Flags;

        public override void Read()
        {
            var br = Chunk.GetReader();

            nTextures = br.ReadUInt32();
            nGroups = br.ReadUInt32();
            nPortals = br.ReadUInt32();
            nLights = br.ReadUInt32();
            nDoodadNames = br.ReadUInt32();
            nDoodadDefs = br.ReadUInt32();
            nDoodadSets = br.ReadUInt32();
            colR = br.ReadByte();
            colG = br.ReadByte();
            colB = br.ReadByte();
            colX = br.ReadByte();
            WmoId = br.ReadUInt32();
            BoundingBox = new Vector3[2];
            for (int i = 0; i < 2; i++)
                BoundingBox[i] = br.ReadVector3();
            Flags = br.ReadUInt32();
        }
    }
}
