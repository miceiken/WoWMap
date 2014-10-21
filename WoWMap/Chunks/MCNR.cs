using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MCNR
    {
        public MCNREntry[] Entries;
        private ushort[] unk0;

        public void Read(BinaryReader br)
        {
            Entries = new MCNREntry[145];
            for (int i = 0; i < Entries.Length; i++)
            {
                var entry = new MCNREntry();
                entry.Read(br);
                Entries[i] = entry;
            }

            unk0 = new ushort[13];
            for (int i = 0; i < 13; i++)
                unk0[i] = br.ReadUInt16();
        }

        public class MCNREntry
        {
            public short[] Normal;

            public void Read( BinaryReader br)
            {
                Normal = new short[3];
                for (int i = 0; i < 3; i++)
                    Normal[i] = br.ReadInt16();
            }
        }
    }
}
