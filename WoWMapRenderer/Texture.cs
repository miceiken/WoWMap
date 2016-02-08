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
    /*public class Texture
    {
        public Texture(int width, int height, PixelInternalFormat internalFormat, PixelFormat format, bool empty = true)
        {
            CanBeBound = false;
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

        public bool CanBeBound { get; private set; }
        public bool Empty { get; private set; }

        public byte[] Data { get; private set; }

        public Texture(string filePath)
        {
            Filename = filePath;
            CanBeBound = false;

            using (var blp = new BlpFile(CASC.OpenFile(filePath)))
            {
                if (GL.IsTexture(ID))
                    GL.DeleteTexture(ID);

                Width = blp.Width;
                Height = blp.Height;

                byte[] bgra;
                blp.GetByteBuffer(0, out bgra);
                Data = bgra;
                Empty = false;

                Format = PixelFormat.Rgba;
                Unit = -1;
                InternalFormat = PixelInternalFormat.Rgba;
            }
        }

        public Texture ApplyAlpha(byte[] alphaMap)
        {
            Debug.Assert(!CanBeBound, "You should not apply an alpha map to an already mapped texture!");
            if (alphaMap == null)
            {
                //! TODO : This is more than likely a temporary hack. 'cause, WTH.
                return this;
            }

            // TODO: According to Wiki, alpha textures upscale via cubic interpolation. This is just expanding pixels size by 4.
            var buffer = new byte[Data.Length];
            Buffer.BlockCopy(Data, 0, buffer, 0, Data.Length);
            if (alphaMap.Length == 64 * 64)
            {
                var pos = 3u;
                for (var i = 0; i < 64 * 64; ++i)
                {
                    buffer[pos] = alphaMap[i];
                    buffer[pos + 4] = alphaMap[i];
                    buffer[pos + 8] = alphaMap[i];
                    buffer[pos + 12] = alphaMap[i];
                    pos += 16;
                }
            }
            return new Texture(buffer, Width, Height, Filename);
        }

        public void UnbindTexture() // Yeaaaah
        {
            CanBeBound = true;
        }

        public void Delete()
        {
            if (GL.IsTexture(ID))
                GL.DeleteTexture(ID);
        }

        public void Unbind()
        {
            CanBeBound = true;
        }

        public void BindTexture(TextureUnit unit)
        {
            // Already bound
            if (unit == Unit + TextureUnit.Texture0)
                return;

            Unit = unit - TextureUnit.Texture0;

            // Debug.Assert(CanBeBound, "Texture cannot be bound to GPU!");

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, ID); // Lock
            int level = 0;
            var fn = (int)All.Linear;
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref fn);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref fn);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, ref level);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, ref level);
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, PixelType.UnsignedByte, Data);
            GL.BindTexture(TextureTarget.Texture2D, 0); // Release
        }

        public Texture(byte[] data, int width, int height, string fileName)
        {
            Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Data, 0, data.Length);

            Format = PixelFormat.Rgba;
            InternalFormat = PixelInternalFormat.Rgba;
            Width = width;
            Height = height;
            Empty = false;
            CanBeBound = true;
            Filename = fileName;
        }
    }*/
}
