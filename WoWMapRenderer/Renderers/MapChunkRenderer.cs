using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using WoWMap.Layers;
using WoWMap.Chunks;
using System.Linq;

namespace WoWMapRenderer.Renderers
{
    public class MapChunkRenderer
    {
        public class TextureInfo
        {
            public uint TextureID; // Offset into MCAL
            public MCLY.MCLYFlags Flags;
            public int AlphaMap;
            public Texture Texture;
            // public int EffectId;
        }

        public int Offset { get; private set; }
        public int Count { get; private set; }

        public List<TextureInfo> Textures { get; private set; }

        public MapChunkRenderer(MapChunk mapChunk)
        {
            Textures = new List<TextureInfo>(mapChunk.MCLY.Count);
            for (var i = 0; i < mapChunk.MCLY.Count; ++i)
            {
                Textures.Add(new TextureInfo
                {
                    TextureID = mapChunk.MCLY[i].TextureId,
                    Flags = mapChunk.MCLY[i].Flags,
                    AlphaMap = (int)mapChunk.MCLY[i].ofsMCAL
                });
            }
        }

        public void SetIndices(int indiceCount, int indiceOffset)
        {
            Offset = indiceOffset * sizeof(uint);
            Count = indiceCount * sizeof(uint);
        }

        public void AddTextureNames(MTEX mtexChunk)
        {
            for (var i = 0; i < Textures.Count; ++i)
                Textures[i].Texture = TextureCache.GetRawTexture(mtexChunk.Filenames[(int)Textures[i].TextureID]);
        }

        public void ApplyAlphaMap(MCAL mcalChunk)
        {
            for (var i = 0; i < Textures.Count; ++i)
            {
                byte[] alphaMap = null;
                if (mcalChunk != null && mcalChunk.HasAlpha(Textures[i].AlphaMap))
                    alphaMap = mcalChunk.GetAlpha(Textures[i].AlphaMap);

                Textures[i].Texture = Textures[i].Texture.ApplyAlpha(alphaMap);
                // Textures are not bound until rendering.
            }
        }

        public void Render(Shader shader)
        {
            // Bind textures and samplers
            var samplers = new int[4];
            GL.GenSamplers(4, samplers);

            // TODO Assess how bad this is during render
            // I can't figure out anything better. Fucking GPU limitations need to be standardized.
            for (var i = 0; i < Textures.Count; ++i)
            {
                var textureInfo = Textures[i];
                if (textureInfo.Texture.Unit != i)
                {
                    textureInfo.Texture.BindTexture(TextureUnit.Texture0 + i);
                    GL.BindSampler(i, samplers[i]);
                }
                GL.Uniform1(shader.GetUniformLocation("texture_sampler" + i), samplers[i]);
            }

            GL.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, (IntPtr)Offset);

            for (var i = 0; i < Textures.Count; ++i)
                GL.BindSampler(i, 0);

            GL.DeleteSamplers(4, samplers);
        }
    }
}
