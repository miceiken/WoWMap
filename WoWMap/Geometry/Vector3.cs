using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWMap.Geometry
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(BinaryReader br)
            : this(br.ReadSingle(), br.ReadSingle(), br.ReadSingle())
        { }

        public static Vector3 Read(BinaryReader br)
        {
            return new Vector3(br);
        }
    }
}
