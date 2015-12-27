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

        public int TriangleCount;

        private List<Texture> _textures = new List<Texture>();

        public MapChunkRenderer()
        {
            VerticeVBO = GL.GenBuffer();
            IndiceVBO = GL.GenBuffer();
            VAO = GL.GenVertexArray();
        }

        public void AddTexture(Texture texture)
        {
            if (!_textures.Contains(texture))
                _textures.Add(texture);
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

        public void Render()
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceVBO);

            for (var i = 0; i < _textures.Count; ++i)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                if (_textures[i] != null)
                    _textures[i].BindToUnit(TextureUnit.Texture0 + i);
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
