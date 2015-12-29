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

        public int TriangleCount;

        private List<string> _textures = new List<string>();

        public MapChunkRenderer()
        {
            VAO = GL.GenVertexArray();
            VerticeVBO = GL.GenBuffer();
            IndiceVBO = GL.GenBuffer();
        }

        public void AddTexture(Texture texture, Shader shader)
        {
            Debug.Assert(_textures.Count <= 4, "MapChunkRenderer: Trying to load too many textures !");
            Debug.Assert(_textureSamplers.Count <= 4, "MapChunkRenderer: Trying to load too many samplers !");

            if (!_textures.Contains(texture.Filename))
                _textures.Add(texture.Filename);

            // Add a sampler anyway
            var sampler = GL.GenSampler();
            _textureSamplers.Add(sampler);
            // Bind to texture right now
            GL.BindSampler(texture.Unit, sampler);
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
