using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;

namespace WoWMap.Chunks
{
    public class MODF
    {
        public uint MWIDEntry;
        public uint UniqueId;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 LowerBounds;
        public Vector3 UpperBounds;
        public ushort Flags; // MODFFlags
        public ushort DoodadSet;
        public ushort NameSet;
        private ushort padding;

        public void Read(BinaryReader br)
        {
            MWIDEntry = br.ReadUInt32();
            UniqueId = br.ReadUInt32();
            Position = new Vector3(br);
            Rotation = new Vector3(br);
            LowerBounds = new Vector3(br);
            UpperBounds = new Vector3(br);
            Flags = br.ReadUInt16();
            DoodadSet = br.ReadUInt16();
            NameSet = br.ReadUInt16();
            padding = br.ReadUInt16();
        }

        public enum MODFFlags : ushort
        {
            Destroyable = 1
        };
    }
}
