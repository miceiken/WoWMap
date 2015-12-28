using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer.Renderers
{
    public class MapChunkRenderer
    {
        public int VerticeVBO { get; private set; }
        public int IndiceVBO { get; private set; }
        public int VAO { get; private set; }

        private List<int> _textureSamplers;

        public int TriangleCount;

        private List<Texture> _textures = new List<Texture>();

        public MapChunkRenderer()
        {
            VAO = GL.GenVertexArray();
            VerticeVBO = GL.GenBuffer();
            IndiceVBO = GL.GenBuffer();
            _textureSamplers = new List<int>();
        }


        public void AddTexture(Texture texture)
        {
            if (!_textures.Contains(texture))
            {
                _textures.Add(texture);
                _textureSamplers.Add(GL.GenSampler());
            }
        }

        public void Delete()
        {
            // GL.DeleteBuffer(IndiceVBO);
            // GL.DeleteBuffer(VerticeVBO);
            GL.DeleteVertexArray(VAO);
            foreach (var t in _textures)
                t.Delete();
            _textures.Clear();
        }

        public void Render(Shader shader)
        {
            GL.BindVertexArray(VAO);
            System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceVBO);
            System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");

            for (var i = 0; i < _textures.Count; ++i)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
                if (_textures[i] != null)
                {
                    var uniform = shader.GetUniformLocation("texture_sampler" + i);
                    _textures[i].BindTexture();
                    // TODO: Move this call to preparations, not rendering
                    GL.BindTexture(TextureTarget.Texture2D, _textures[i].ID);
                    System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
                    GL.Uniform1(uniform, i);
                    System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
                }
                else
                    GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            GL.DrawElements(PrimitiveType.Triangles, TriangleCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
            GL.BindTexture(TextureTarget.Texture2D, 0);
            System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
            GL.BindVertexArray(0);
            System.Diagnostics.Debug.Assert(GL.GetError() == ErrorCode.NoError, "An error code was thrown, debug me");
        }
    }
}
