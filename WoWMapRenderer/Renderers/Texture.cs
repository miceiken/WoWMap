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

        public string Filename { get; private set; }

        public bool Loaded { get; private set; }

        public int MinFilter { get; set; }
        public int MagFilter { get; set; }

        public int WrapS { get; set; }
        public int WrapT { get; set; }

        public int Width;
        public int Height;

        public PixelFormat Format { get; set; }
        public PixelInternalFormat InternalFormat { get; set; }

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

            CurrentUnit = TextureUnit.Texture31; // Default. Will never stay like this due to subsequent calls
        }

        public Texture(string filePath)
        {
            using (var blp = new BlpFile(CASC.OpenFile(filePath)))
            {
                Filename = filePath;

                Width = blp.Width;
                Height = blp.Height;

                Data = blp.GetByteBuffer(0);

                WrapT = (int)All.ClampToEdge;
                WrapS = (int)All.ClampToEdge;

                MagFilter = (int)All.Linear;
                MinFilter = (int)All.Linear;

                CurrentUnit = TextureUnit.Texture31; // Default. Will never stay like this due to subsequent calls
            }
        }

        public bool Load()
        {
            // Don't waste cycles reloading the texture if it is already loaded
            if (Loaded)
                return false;

            // Cleanup!
            if (GL.IsTexture(TextureId))
                GL.DeleteTexture(TextureId);

            TextureId = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, WrapT);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
            GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Width, Height, 0, Format, PixelType.UnsignedByte, Data);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            Data = null;

            Loaded = true;
            return true;
        }

        public bool Bind(TextureUnit unit, int uniformLocation)
        {
            // TODO: Unbind needs to be properly called.
            // Right now this breaks stuff.
            if (/*unit == CurrentUnit || */!Loaded)
                return false;

            CurrentUnit = unit;
            GL.ActiveTexture(CurrentUnit);
            GL.BindTexture(TextureTarget.Texture2D, TextureId);
            GL.Uniform1(uniformLocation, CurrentUnit - TextureUnit.Texture0);
            return true;
        }

        public void Unbind()
        {
            if (!Loaded)
                return;

            GL.ActiveTexture(CurrentUnit);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            CurrentUnit = TextureUnit.Texture31;
        }

        public void Delete()
        {
            if (!GL.IsTexture(TextureId) || !Loaded)
                return;

            Loaded = false;
            GL.DeleteTexture(TextureId);
        }
    }
}
