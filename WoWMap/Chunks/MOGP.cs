using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;
using WoWMap.Geometry;
using OpenTK;

namespace WoWMap.Chunks
{
    public class MOGP : ChunkReader
    {
        public MOGP(Chunk c) : base(c, c.Size) { }
        public MOGP(Chunk c, uint h) : base(c, h) { }

        public uint ofsGroupName;               // ofsMOGN
        public uint ofsDescriptiveGroupName;    // ofsMOGN
        public MOGPFlags Flags;
        public Vector3 BoundingBox1;
        public Vector3 BoundingBox2;
        public ushort PortalIndex;              // index to  MOPR chunk
        public ushort nPortals;
        public ushort[] Batches;
        public byte[] FogIndices;
        public uint LiquidType;
        public uint GroupId;
        private uint unk0;
        private uint unk1;

        public static readonly uint ChunkHeaderSize = 0x44;

        [Flags]
        public enum MOGPFlags : uint
        {
            MOBN_MOBR = 0x1,
            VertexColors = 0x4,
            Exterior = 0x8,
            Unreachable = 0x80,
            HasLights = 0x200,
            LotOfChunks = 0x400,
            HasDoodads = 0x800,
            HasWater = 0x1000,
            Interior = 0x2000,
            MORI_MORB = 0x20000,
            ShowSkybox = 0x40000,
            TwoMOCV = 0x1000000,
            TwoMOTV = 0x2000000,
        };

        public override void Read()
        {
            var br = Chunk.GetReader();

            ofsGroupName = br.ReadUInt32();
            ofsDescriptiveGroupName = br.ReadUInt32();
            Flags = (MOGPFlags)br.ReadUInt32();
            BoundingBox1 = br.ReadVector3();
            BoundingBox2 = br.ReadVector3();
            PortalIndex = br.ReadUInt16();
            nPortals = br.ReadUInt16();
            Batches = new ushort[4];
            for (int i = 0; i < 4; i++)
                Batches[i] = br.ReadUInt16();
            FogIndices = new byte[4];
            for (int i = 0; i < 4; i++)
                FogIndices[i] = br.ReadByte();
            LiquidType = br.ReadUInt32();
            GroupId = br.ReadUInt32();
            unk0 = br.ReadUInt32();
            unk1 = br.ReadUInt32();
        }
    }
}
