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
            VerticeVBO = GL.GenBuffer();
            IndiceVBO = GL.GenBuffer();
            VAO = GL.GenVertexArray();
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
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceVBO);
            GL.Enable(EnableCap.Texture2D);

            for (var i = 0; i < _textures.Count; ++i)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                if (_textures[i] != null)
                {
                    var uniform = shader.GetUniformLocation("texture_sampler" + i);
                    // TODO: Move this call to preparations, not rendering
                    _textures[i].BindTexture();
                    GL.BindTexture(TextureTarget.Texture2D, _textures[i].ID);
                    GL.BindSampler(i, _textureSamplers[i]);
                    GL.Uniform1(uniform, i);
                }
                else
                    GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            GL.DrawElements(PrimitiveType.Triangles, TriangleCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }
    }
}
