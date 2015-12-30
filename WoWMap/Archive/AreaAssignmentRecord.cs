using System.Runtime.InteropServices;

namespace WoWMap.Archive
{
    [StructLayout(LayoutKind.Sequential)]
    public class AreaAssignmentRecord
    {
        public int AreaID { get; set; }
        public int MapID { get; set; }

        public int Unk { get; set; }

        public int ChunkX { get; set; }
        public int ChunkY { get; set; }
    }
}
