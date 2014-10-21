using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Chunks
{
    public class MMID
    {
        public uint[] Offsets;

        public void Read(BinaryReader br, uint size)
        {
            Offsets = new uint[size / 4];
            for (int i = 0; i < Offsets.Length; i++)
                Offsets[i] = br.ReadUInt32();
        }
    }
}
