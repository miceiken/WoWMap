using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer
{
    class Framebuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private int FBO;
        public int TexColorBuffer { get; private set; }
        private int RboDepthStencil;

        public FramebufferErrorCode ErrorCode { get; private set; }

        public Framebuffer(int width, int height)
        {
            Width = width;
            Height = height;

            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);

            TexColorBuffer = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TexColorBuffer);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Clamp);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TexColorBuffer, 0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            RboDepthStencil = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RboDepthStencil);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width, Height);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RboDepthStencil);

            ErrorCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (ErrorCode != FramebufferErrorCode.FramebufferComplete)
            {
                GL.DeleteFramebuffer(FBO);
                GL.DeleteRenderbuffer(RboDepthStencil);
                GL.DeleteTexture(TexColorBuffer);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        }

        /// <summary>
        /// Renders to back buffer.
        /// </summary>
        public void Release()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        ~Framebuffer()
        {
            /* GL.DeleteFramebuffer(FBO);
            GL.DeleteRenderbuffer(RboDepthStencil);
            GL.DeleteTexture(TexColorBuffer); */
        }
    }
}
