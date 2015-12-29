using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SereniaBLPLib;
using WoWMap.Archive;

namespace WoWMapRenderer
{
    public class Texture
    {
        public Texture(int width, int height, PixelInternalFormat internalFormat, PixelFormat format, bool empty = true)
        {
            ID = 0;
            Width = width;
            Height = height;
            InternalFormat = internalFormat;
            Format = format;
            Empty = empty;
        }

        public void LoadEmptyTexture()
        {
            ID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new[] { (int)All.Linear });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new[] { (int)All.Nearest });
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public int ID { get; private set; }
        public int Unit { get; private set; }
        public string Filename { get; private set; }
        public PixelFormat Format { get; private set; }
        public PixelInternalFormat InternalFormat { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool Empty { get; private set; }

        public byte[] OriginalData { get; private set; }
        public byte[] BGRA { get; private set; }

        public Texture(string filePath)
        {
            Filename = filePath;

            using (var blp = new BlpFile(CASC.OpenFile(filePath)))
            {
                if (GL.IsTexture(ID))
                    GL.DeleteTexture(ID);

                Width = blp.Width;
                Height = blp.Height;

                ID = GL.GenTexture();
                byte[] bgra;
                blp.GetByteBuffer(0, out bgra);
                OriginalData = bgra;
                Empty = false;

                Format = PixelFormat.Rgba;
                Unit = -1;
                InternalFormat = PixelInternalFormat.Rgba;
            }
        }

        public void ApplyAlphaAndBind(byte[] alphaMap, TextureUnit unit)
        {
            // TODO: According to Wiki, alpha textures upscale via cubic interpolation. This is just expanding pixels size by 4.
            BGRA = new byte[OriginalData.Length];
            Buffer.BlockCopy(OriginalData, 0, BGRA, 0, OriginalData.Length);
            if (alphaMap.Length == 64 * 64)
            {
                var pos = 3u;
                for (var i = 0; i < 64 * 64; ++i)
                {
                    BGRA[pos] = alphaMap[i];
                    BGRA[pos + 4] = alphaMap[i];
                    BGRA[pos + 8] = alphaMap[i];
                    BGRA[pos + 12] = alphaMap[i];
                    pos += 16;
                }
            }
            BindTexture(unit);
        }

        public void Delete()
        {
            if (GL.IsTexture(ID))
                GL.DeleteTexture(ID);
        }

        public void BindTexture(TextureUnit unit)
        {
            Unit = unit - TextureUnit.Texture0;

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, ID); // Lock
            int level = 0;
            var fn = (int)All.Nearest;
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref fn);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref fn);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, ref level);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, ref level);
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, PixelType.UnsignedByte, BGRA);
            // No release, save some calls
        }

        public Texture(byte[] data, int width, int height)
        {
            OriginalData = data;
            Format = PixelFormat.Alpha;
            InternalFormat = PixelInternalFormat.Alpha;
            Width = width;
            Height = height;
            Empty = false;
        }
    }
}
