using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap
{
    // Shamelessly stolen and modified from
    // https://github.com/Marlamin/WoWFormatTest/blob/master/WoWFormatLib/Utils/BlizzHeader.cs
    public class ChunkHeader
    {
        public char[] Header
        {
            get;
            set;
        }

        public uint Size
        {
            get;
            set;
        }

        public ChunkHeader()
        {
            Header = new char[4];
            Size = 0;
        }

        public ChunkHeader(char[] header, uint size)
        {
            Header = header;
            Size = size;
        }

        public void Flip()
        {
            Array.Reverse(Header);
        }

        public override string ToString()
        {
            return Header.ToString();
        }

        public void Read(BinaryReader br)
        {
            Header = br.ReadChars(4);
            Size = br.ReadUInt32();
        }
    }
}
