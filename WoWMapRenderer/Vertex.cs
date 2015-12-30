using System.Runtime.InteropServices;
using OpenTK;

namespace WoWMapRenderer
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal struct Vertex
    {
        public Vector3 Color;
        public Vector3 Position;
        public Vector2 TextureCoordinates;
    }
}
