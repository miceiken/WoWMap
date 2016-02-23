using System.Runtime.InteropServices;
using OpenTK;
using System.Drawing;
using WoWMap.Geometry;
using System.Collections.Generic;

namespace WoWMapRenderer
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector4 Color;
    }
}
