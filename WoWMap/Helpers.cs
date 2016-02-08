using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Geometry;
using OpenTK;
using System.Drawing;

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
                    offset = i + 1;
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

        public static float ToRadians(this float angle)
        {
            return (float)(Math.PI / 180) * angle;
        }

        public static Vector3 ToVector3(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector4 UnProject(this Vector2 mouse, Matrix4 projection, Matrix4 view, Size viewport)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / (float)viewport.Width - 1;
            vec.Y = -(2.0f * mouse.Y / (float)viewport.Height - 1);
            vec.Z = 0;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec;
        }
    }
}
