using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using OpenTK;

namespace WoWMap
{
    public static class Helpers
    {
        public static void Each<T>(this IEnumerable<T> ie, Action<T, int> action)
        {
            var i = 0;
            foreach (var e in ie) action(e, i++);
        }

        public static Dictionary<uint, string> GetIndexedStrings(byte[] data)
        {
            var ret = new Dictionary<uint, string>();

            var sb = new StringBuilder();
            var offset = 0u;
            for (uint i = 0; i < data.Length; i++)
            {
                if (data[i] == '\0') // Terminate string
                {
                    if (sb.Length > 1)
                        ret.Add(offset, sb.ToString());
                    offset = i+1;
                    sb = new StringBuilder();

                    continue;
                }

                sb.Append((char)data[i]);
            }

            return ret;
        }

        public static string ReadCString(byte[] data)
        {
            var sb = new StringBuilder(0x100);
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '\0') // Terminate string
                    if (sb.Length > 1) break;

                sb.Append((char)data[i]);
            }
            return sb.ToString();
        }

        public static string ReadCString(this BinaryReader br)
        {
            var buffer = new List<byte>();
            byte b = 0;
            while ((b = br.ReadByte()) != 0)
                buffer.Add(b);

            if (buffer.Count <= 0)
                return null;

            return Encoding.ASCII.GetString(buffer.ToArray());
        }

        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        /*public static OpenTK.Vector3 ToV3(this SharpDX.Vector3 v)
        {
            return new SharpNav.Vector3(v.X, v.Y, v.Z);
        }*/

        public static float ToRadians(this float angle)
        {
            return (float)(Math.PI / 180) * angle;
        }

        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
