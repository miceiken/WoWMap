using System.Runtime.InteropServices;
using OpenTK;

namespace WoWMapRenderer
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Vertex
    {
        public int Type; // 0 = Terrain, 1 = WMO, 2 = M2
        public Vector3 Position;
    }
}
