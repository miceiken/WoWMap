using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Archive
{
    public class DBC<T> where T : new()
    {
        private const uint DB2FmtSig = 0x32424457; // WDB2

        public DBC(string filename)
        {
            if (!CASC.Initialized) return;
            if (!CASC.FileExists(filename)) return;

            using (var br = new BinaryReader(CASC.OpenFile(filename)))
            {
                // Make sure we've got a valid DBC
                if (br.BaseStream.Length < DBCHeader.Size) return;
                if (DB2FmtSig != br.ReadUInt32()) return;

                // Read DBC header
                Header = new DBCHeader();
                Header.Read(br);

                // Read strings to a table
                var readPos = br.BaseStream.Position;
                var strTableOffset = br.BaseStream.Position + (Header.RecordCount * Header.RecordSize);
                var strTable = new Dictionary<int, string>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                    strTable[(int)(br.BaseStream.Position - strTableOffset)] = br.ReadCString();

                // Move back to where we were
                br.BaseStream.Position = readPos;
                Rows = new T[Header.RecordCount];
                var tProperties = typeof(T).GetProperties();
                for (int i = 0; i < Header.RecordCount; i++)
                {
                    var row = new T();
                    var startPos = br.BaseStream.Position;

                    // Can't we use a foreach here?
                    for (int j = 0; j < tProperties.Length; j++)
                    {
                        switch (Type.GetTypeCode(tProperties[j].PropertyType))
                        {
                            case TypeCode.Int32:
                                tProperties[j].SetValue(row, br.ReadInt32());
                                break;
                            case TypeCode.UInt32:
                                tProperties[j].SetValue(row, br.ReadUInt32());
                                break;
                            case TypeCode.Single:
                                tProperties[j].SetValue(row, br.ReadSingle());
                                break;
                            case TypeCode.String:
                                tProperties[j].SetValue(row, strTable[br.ReadInt32()]);
                                break;
                        }
                    }

                    var diffSize = (br.BaseStream.Position - startPos);
                    if (diffSize > Header.RecordSize) return; // We read too much! Struct is wrong
                    if (diffSize < Header.RecordSize) br.ReadBytes((int)(Header.RecordSize - diffSize)); // We read too little! Let's pad!

                    Rows[i] = row;
                }

            }
        }

        public DBCHeader Header
        {
            get;
            private set;
        }

        public T[] Rows
        {
            get;
            private set;
        }

        public T this[int row]
        {
            get { return Rows[row]; }
        }

        #region Nested class: DBCHeader

        public class DBCHeader
        {
            #region Properties

            public string Filename
            {
                get;
                private set;
            }

            public int RecordCount
            {
                get;
                private set;
            }

            public int FieldCount
            {
                get;
                private set;
            }

            public int RecordSize
            {
                get;
                private set;
            }

            public int StringBlockSize
            {
                get;
                private set;
            }

            public int TableHash
            {
                get;
                private set;
            }

            public int Build
            {
                get;
                private set;
            }

            public int LastWritten
            {
                get;
                private set;
            }

            public int MinId
            {
                get;
                private set;
            }

            public int MaxId
            {
                get;
                private set;
            }

            public int Locale
            {
                get;
                private set;
            }

            private int unk0
            {
                get;
                set;
            }

            public static readonly int Size = 48;

            #endregion

            public void Read(BinaryReader br)
            {
                RecordCount = br.ReadInt32();
                FieldCount = br.ReadInt32();
                RecordSize = br.ReadInt32();
                StringBlockSize = br.ReadInt32();
                TableHash = br.ReadInt32();
                Build = br.ReadInt32();
                LastWritten = br.ReadInt32();
                MinId = br.ReadInt32();
                MaxId = br.ReadInt32();
                Locale = br.ReadInt32();
                unk0 = br.ReadInt32();
            }
        }

        #endregion
    }
}
