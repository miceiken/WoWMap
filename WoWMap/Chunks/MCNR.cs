using System.IO;
using WoWMap.Readers;
using OpenTK;

namespace WoWMap.Chunks
{
    public class MCNR : ChunkReader
    {
        public MCNR(Chunk c, uint h) : base(c, h) { }
        public MCNR(Chunk c) : base(c, c.Size) { }

        public MCNREntry[] Entries;
        private ushort[] unk0;

        public override void Read()
        {
            var br = Chunk.GetReader();

            Entries = new MCNREntry[145];
            for (int i = 0; i < Entries.Length; i++)
                Entries[i] = MCNREntry.Read(br);

            unk0 = new ushort[13];
            for (int i = 0; i < 13; i++)
                unk0[i] = br.ReadUInt16();
        }

        public class MCNREntry
        {
            public Vector3 Normal;

            public static MCNREntry Read(BinaryReader br)
            {
                var entry = new MCNREntry();
                entry.Normal = new Vector3
                {
                    X = br.ReadInt16()/127.0f,
                    Y = br.ReadInt16()/127.0f,
                    Z = br.ReadInt16()/127.0f
                };
                return entry;
            }
        }
    }
}
