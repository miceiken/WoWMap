using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace WoWMapRenderer.Renderers
{
    public class MapChunkRenderer
    {
        private List<int> _textureSamplers = new List<int>();
        private List<Texture> _textures = new List<Texture>();

        public MapChunkRenderer()
        {
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
            foreach (var sampler in _textureSamplers)
                GL.DeleteSampler(sampler);
            _textureSamplers.Clear();
        }

        public void Render(Shader shader)
        {
            for (var i = 0; i < _textureSamplers.Count; ++i)
                GL.Uniform1(shader.GetUniformLocation("texture_sampler" + i), _textureSamplers[i]); 
        }
    }
}
