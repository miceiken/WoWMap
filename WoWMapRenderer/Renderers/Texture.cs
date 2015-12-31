using OpenTK.Graphics.OpenGL;
using SereniaBLPLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Archive;

namespace WoWMapRenderer.Renderers
{
    public class Texture
    {
        public int TextureId { get; private set; }

        public int MinFilter;
        public int MagFilter;

        public int WrapS;
        public int WrapT;

        public int Width;
        public int Height;

        public PixelFormat Format;
        public PixelInternalFormat InternalFormat;

        public byte[] Data { get; private set; }

        public TextureUnit CurrentUnit { get; private set; }

        public Texture(byte[] data, int width, int height)
        {
            Width = width;
            Height = height;

            Data = new byte[data.Length];
            Buffer.BlockCopy(data, 0, Data, 0, data.Length);

            WrapT = (int)All.ClampToEdge;
            WrapS = (int)All.ClampToEdge;

            MagFilter = (int)All.Linear;
            MinFilter = (int)All.LinearMipmapLinear;

            CurrentUnit = TextureUnit.Texture31; // Some we will never reach
        }

        public Texture(string filePath)
        {
            using (var blp = new BlpFile(CASC.OpenFile(filePath)))
            {
                Width = blp.Width;
                Height = blp.Height;

                Data = blp.GetByteBuffer(0);

                WrapT = (int)All.ClampToEdge;
                WrapS = (int)All.ClampToEdge;

                MagFilter = (int)All.Linear;
                MinFilter = (int)All.LinearMipmapLinear;

                CurrentUnit = TextureUnit.Texture31; // Some we will never reach
            }
        }

        public bool BindToUnit(TextureUnit unit)
        {
            // Don't waste cycles rebinding if target has not changed.
            if (unit == CurrentUnit)
                return false;

            // Cleanup!
            if (GL.IsTexture(TextureId))
                GL.DeleteTexture(TextureId);

            CurrentUnit = unit;
            GL.ActiveTexture(unit);

            // TODO: is this guaranteed to circle back and not go over implementation limits?
            TextureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, PixelType.Byte, Data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, WrapT);
            return true;
        }
    }
}
