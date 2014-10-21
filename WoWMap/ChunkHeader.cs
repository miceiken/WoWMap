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

        public string Name
        {
            get { return new string(Header); }
        }

        public ChunkHeader()
        {
            Header = new char[4];
            Size = 0;
        }

        public ChunkHeader(char[] header, uint size)
        {
            Header = header;
            Array.Reverse(Header);
            Size = size;            
        }

        public ChunkHeader(BinaryReader br)
        {
            Read(br);
        }

        public override string ToString()
        {
            return Name;
        }

        public void Read(BinaryReader br)
        {
            Header = br.ReadChars(4);
            Array.Reverse(Header);
            Size = br.ReadUInt32();
        }
    }
}
