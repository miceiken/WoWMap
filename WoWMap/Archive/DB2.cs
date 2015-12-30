using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WoWMap.Archive
{
    public class DB2<T> where T : class, new()
    {
        private const uint DB2FmtSig = 0x32424457; // WDBC

        private int[] Indices;
        private short[] StringLengths;

        public DB2(string filename)
        {
            if (!CASC.Initialized) return;
            if (!CASC.FileExists(filename))
                return;

            #region Read DB2
            using (var br = new BinaryReader(CASC.OpenFile(filename)))
            {
                // Make sure we've got a valid DB2
                if (br.BaseStream.Length < DB2Header.Size) return;
                if (DB2FmtSig != br.ReadUInt32()) return;

                // Read DBC header
                Header = new DB2Header();
                Header.Read(br);

                if (Header.MaxId != 0)
                {
                    Indices = new int[Header.MaxId - Header.MinId + 1];
                    StringLengths = new short[Header.MaxId - Header.MinId + 1];
                    for (var i = 0; i < Header.MaxId - Header.MinId + 1; ++i)
                        Indices[i] = br.ReadInt32();

                    for (var i = 0; i < Header.MaxId - Header.MinId + 1; ++i)
                        StringLengths[i] = br.ReadInt16();
                }

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

                Debug.Assert(Marshal.SizeOf(typeof(T)) == Header.RecordSize,
                    $"Invalid record size, got {Marshal.SizeOf(typeof(T))}, expected {Header.RecordSize}.");

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

        public DB2Header Header
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
            get { return Rows[Header.MaxId != 0 ? Indices[row] : row]; }
        }

        #region Nested class: DB2Header

        public class DB2Header
        {
            #region Properties

            public int RecordCount { get; private set; }
            public int FieldCount { get; private set; }
            public int RecordSize { get; private set; }
            public int StringBlockSize { get; private set; }
            public int TableHash { get; private set; }
            public int Build { get; private set; }
            public int TimestampLastWritten { get; private set; }
            public int MinId { get; private set; }
            public int MaxId { get; private set; }
            public int Locale { get; private set; }
            public int CopyTableSize { get; private set; }

            public static readonly int Size = 11 * 4;

            #endregion

            public void Read(BinaryReader br)
            {
                RecordCount = br.ReadInt32();
                FieldCount = br.ReadInt32();
                RecordSize = br.ReadInt32();
                StringBlockSize = br.ReadInt32();
                TableHash = br.ReadInt32();
                Build = br.ReadInt32();
                TimestampLastWritten = br.ReadInt32();
                MinId = br.ReadInt32();
                MaxId = br.ReadInt32();
                Locale = br.ReadInt32();
                CopyTableSize = br.ReadInt32();
            }
        }

        #endregion
    }
}
