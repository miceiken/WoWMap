using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using SereniaBLPLib;
using WoWMap.Archive;

namespace WoWMapRenderer
{
    class Texture
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
            if (GL.IsTexture(ID))
                GL.DeleteTexture(ID);

            ID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new[] { (int)All.Linear });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new[] { (int)All.Nearest });
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public bool Load(string fileName)
        {
            if (GL.IsTexture(ID))
                GL.DeleteTexture(ID);

            ID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ID);

            using (var blp = new BlpFile(CASC.OpenFile(fileName)))
            {
                Width = blp.Width;
                Height = blp.Height;
                byte[] bgra;
                blp.GetByteBuffer(0, out bgra);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bgra);
                GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new[] { (int)All.Linear });
                GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new[] { (int)All.Nearest });
            }
            return true;
        }

        public int ID { get; private set; }

        public PixelFormat Format { get; private set; }
        public PixelInternalFormat InternalFormat { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool Empty { get; private set; }

        public Texture(string filePath)
        {
            using (var blp = new BlpFile(CASC.OpenFile(filePath)))
            {
                if (GL.IsTexture(ID))
                    GL.DeleteTexture(ID);

                Width = blp.Width;
                Height = blp.Height;

                ID = GL.GenTexture();

                byte[] BGRA;
                blp.GetByteBuffer(0, out BGRA);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, ID);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, BGRA);
                GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new[] { (int)All.Linear });
                GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new[] { (int)All.Nearest });
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }
    }
}
