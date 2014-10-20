using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCNR : IChunkReader
    {
        public MCNREntry[] Entries;
        private ushort[] unk0;

        public void Read(ChunkHeader header, BinaryReader br)
        {
            for (int i = 0; i < 9 * 9 + 8 * 8; i++)
            {
                var entry = new MCNREntry();
                entry.Read(header, br);
                Entries[i] = entry;
            }

            for (int i = 0; i < 13; i++)
                unk0[i] = br.ReadUInt16();
        }

        public class MCNREntry : IChunkReader
        {
            public short[] Normal;

            public void Read(ChunkHeader header, BinaryReader br)
            {
                for (int i = 0; i < 3; i++)
                    Normal[i] = br.ReadInt16();
            }
        }
    }
}
