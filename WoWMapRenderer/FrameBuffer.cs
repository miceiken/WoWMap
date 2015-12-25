using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer
{
    class FrameBuffer
    {
        private Dictionary<int, Texture>  _textures = new Dictionary<int, Texture>();

        private int _depthBufferID = 0;
        private int _frameBufferID = 0;

        public int FrameBufferID { get { return _frameBufferID; } }
        public int DepthBufferID { get { return _depthBufferID; } }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public FrameBuffer() : this(0, 0)
        {
            
        }

        ~FrameBuffer()
        {
            if (GL.IsFramebuffer(_frameBufferID))
                GL.DeleteFramebuffer(_frameBufferID);
            if (GL.IsRenderbuffer(_depthBufferID))
            GL.DeleteRenderbuffer(_depthBufferID);
            _textures.Clear();
        }

        public FrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            _frameBufferID = 0;
            _depthBufferID = 0;
        }

        public int AddTexture(int key, string tex)
        {
            var text = new Texture(tex);
            _textures[key] = text;
            return text.ID;
        }

        public bool Load()
        {
            _frameBufferID = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _frameBufferID);

            var colorBuffer = new Texture(Width, Height, PixelInternalFormat.Rgba, PixelFormat.Bgra, true);
            colorBuffer.LoadEmptyTexture();
            _textures.Add(0, colorBuffer);

            CreateRenderbuffer(RenderbufferStorage.Depth24Stencil8, ref _depthBufferID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, colorBuffer.ID, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, DepthBufferID);

            var errCode = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (errCode != FramebufferErrorCode.FramebufferComplete)
            {
                GL.DeleteFramebuffer(FrameBufferID);
                GL.DeleteRenderbuffer(DepthBufferID);
                _textures.Clear();
                Debug.Assert(false, "Error while constructing FBO !");
                return false;
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return true;
        }

        public void CreateRenderbuffer(RenderbufferStorage internalFmt, ref int id)
        {
            if (GL.IsRenderbuffer(id))
                GL.DeleteRenderbuffer(id);

            id = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, id);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFmt, Width, Height);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
        }

        public Texture this[int key]
        {
            get { return _textures[key]; }
        }
    }
}
