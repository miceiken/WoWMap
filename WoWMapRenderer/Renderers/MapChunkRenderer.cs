using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace WoWMapRenderer.Renderers
{
    public class MapChunkRenderer
    {
        public int VerticeVBO { get; private set; }
        public int IndiceVBO { get; private set; }
        public int VAO { get; private set; }

        private List<int> _textureSamplers = new List<int>();
        private List<Texture> _textures = new List<Texture>();

        public int TriangleCount;

        public MapChunkRenderer()
        {
            VAO = GL.GenVertexArray();
            VerticeVBO = GL.GenBuffer();
            IndiceVBO = GL.GenBuffer();
        }

        public void AddTexture(Texture texture)
        {
            Debug.Assert(_textureSamplers.Count <= 4, "MapChunkRenderer: Trying to load too many samplers !");

            _textures.Add(texture);
            texture.BindTexture(TextureUnit.Texture0 + TextureCache.Unit);

            var sampler = GL.GenSampler();
            GL.BindSampler(texture.Unit, sampler);
            _textureSamplers.Add(sampler);
        }

        public void Delete()
        {
            if (GL.IsBuffer(IndiceVBO))
                GL.DeleteBuffer(IndiceVBO);
            if (GL.IsBuffer(VerticeVBO))
                GL.DeleteBuffer(VerticeVBO);
            if (GL.IsVertexArray(VAO))
                GL.DeleteVertexArray(VAO);

            foreach (var sampler in _textureSamplers)
                GL.DeleteSampler(sampler);
            _textureSamplers.Clear();
        }

        public void Render(Shader shader)
        {
            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceVBO);

            for (var i = 0; i < _textureSamplers.Count; ++i)
            {
                // GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.Uniform1(shader.GetUniformLocation("texture_sampler" + i), _textureSamplers[i]); 
            }

            GL.DrawElements(PrimitiveType.Triangles, TriangleCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
    }
}
