using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace WoWMapRenderer.Renderers
{
    public class MapChunkRenderer
    {
        public int VerticeVBO { get; private set; }
        public int IndiceVBO { get; private set; }
        public int VAO { get; private set; }

        private List<int> _textureSamplers = new List<int>();
        private Dictionary<int /* unit */, Texture> _textures = new Dictionary<int, Texture>();

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

            // Unit has already been incremented for the next call - we need current value
            _textures.Add(TextureCache.Unit - 1, texture);
            var sampler = GL.GenSampler();
            GL.BindSampler(TextureCache.Unit - 1, sampler);
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
            _textures.Clear();
        }

        public bool Render(Shader shader)
        {
            try {
                GL.BindVertexArray(VAO);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndiceVBO);

                foreach (var kv in _textures)
                    kv.Value.BindTexture(TextureUnit.Texture0 + kv.Key);

                for (var i = 0; i < _textureSamplers.Count; ++i)
                    GL.Uniform1(shader.GetUniformLocation("texture_sampler" + i), _textureSamplers[i]);

                GL.DrawElements(PrimitiveType.Triangles, TriangleCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

                foreach (var kv in _textures)
                    kv.Value.Unbind();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
