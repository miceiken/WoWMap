using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace WoWMap.Archive
{
    public class DBC<T> where T : new()
    {
        private const uint DBCFmtSig = 0x43424457; // WDBC

        public DBC(string filename)
        {
            if (!CASC.Initialized) return;
            if (!CASC.FileExists(filename)) return;

            #region Read DBC
            using (var br = new BinaryReader(CASC.OpenFile(filename)))
            {
                // Make sure we've got a valid DBC
                if (br.BaseStream.Length < DBCHeader.Size) return;
                if (DBCFmtSig != br.ReadUInt32()) return;

                // Read DBC header
                Header = new DBCHeader();
                Header.Read(br);

                // Read strings to a table
                var readPos = br.BaseStream.Position;
                var strTableOffset = br.BaseStream.Position + (Header.RecordCount * Header.RecordSize);
                br.BaseStream.Position = strTableOffset;
                var strTable = new Dictionary<int, string>();
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    var idx = (int)(br.BaseStream.Position - strTableOffset);
                    strTable[idx] = br.ReadCString();
                }

                // Move back to where we were
                br.BaseStream.Position = readPos;
                Rows = new T[Header.RecordCount];
                var tProperties = typeof(T).GetProperties();
                for (var i = 0; i < Header.RecordCount; i++)
                {
                    var row = new T();
                    var startPos = br.BaseStream.Position;

                    foreach (var prop in tProperties)
                    {
                        switch (Type.GetTypeCode(prop.PropertyType))
                        {
                            case TypeCode.Int32:
                                prop.SetValue(row, br.ReadInt32());
                                break;
                            case TypeCode.UInt32:
                                prop.SetValue(row, br.ReadUInt32());
                                break;
                            case TypeCode.Single:
                                prop.SetValue(row, br.ReadSingle());
                                break;
                            case TypeCode.String:
                                prop.SetValue(row, strTable[br.ReadInt32()]);
                                break;
                            default:
                                Console.WriteLine("wat?? {0}", Type.GetTypeCode(prop.PropertyType));
                                break;
                        }
                    }

                    var diffSize = (br.BaseStream.Position - startPos);
                    if (diffSize > Header.RecordSize) return; // We read too much! Struct is wrong
                    if (diffSize < Header.RecordSize) br.ReadBytes((int)(Header.RecordSize - diffSize)); // We read too little! Let's pad!

                    Rows[i] = row;
                }

            }
            #endregion
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

            public static readonly int Size = 48;

            #endregion

            public void Read(BinaryReader br)
            {
                RecordCount = br.ReadInt32();
                FieldCount = br.ReadInt32();
                RecordSize = br.ReadInt32();
                StringBlockSize = br.ReadInt32();
            }
        }

        #endregion
    }
}
