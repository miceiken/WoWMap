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
        public class Material
        {
            public uint TextureID; // Offset into MCAL
            public int AlphaMapId;

            public Texture AlphaTexture;
            public Texture Texture;
        }

        public int IndicesOffset { get; private set; } // For rendering vertices
        public int IndicesCount { get; private set; } // For rendering vertices

        public int LayerCount { get { return Materials.Count; } } // Sent to shader program

        public List<Material> Materials { get; private set; }

        public MapChunkRenderer(MapChunk mapChunk)
        {
            Materials = new List<Material>(mapChunk.MCLY.Entries.Length);
            for (var i = 0; i < mapChunk.MCLY.Entries.Length; ++i)
            {
                Materials.Add(new Material
                {
                    TextureID = mapChunk.MCLY.Entries[i].TextureId,
                    AlphaMapId = (int)mapChunk.MCLY.Entries[i].ofsMCAL
                });
            }
        }

        public void SetIndices(int indiceCount, int indiceOffset)
        {
            IndicesOffset = indiceOffset * sizeof(uint);
            IndicesCount = indiceCount * sizeof(uint);
        }

        public void AddTextureNames(MTEX mtexChunk)
        {
            for (var i = 0; i < Materials.Count; ++i)
            {
                var textureName = mtexChunk.Filenames[(int)Materials[i].TextureID];

                // Get the texture and define a default slot for it
                Materials[i].Texture = TextureCache.GetRawTexture(textureName);
            }
        }

        public void ApplyAlphaMap(MCAL mcalChunk)
        {
            for (var i = 0; i < Materials.Count; ++i)
            {
                if (mcalChunk == null || !mcalChunk.HasAlpha(Materials[i].AlphaMapId))
                    continue;

                Materials[i].AlphaTexture = new Texture(mcalChunk.GetAlpha(Materials[i].AlphaMapId), 64, 64);
                // TODO: Luminance is legacy. Use Alpha.
                Materials[i].AlphaTexture.InternalFormat = PixelInternalFormat.Luminance;
                Materials[i].AlphaTexture.Format = PixelFormat.Luminance;
                Materials[i].AlphaTexture.MagFilter = (int)All.Linear;
                Materials[i].AlphaTexture.MinFilter = (int)All.Linear;
                Materials[i].AlphaTexture.Load();
            }
        }

        public void Render(Shader shader, int[] terrainSamplers, int[] alphaMapSamplers)
        {
            GL.Uniform1(shader.GetUniformLocation("layerCount"), LayerCount);

            for (var i = 0; i < Materials.Count; ++i)
            {
                var textureInfo = Materials[i];
                textureInfo.Texture.Bind(TextureUnit.Texture0 + 2 * i,
                    shader.GetUniformLocation("texture" + i));

                if (i > 0 && textureInfo.AlphaTexture != null)
                {
                    textureInfo.AlphaTexture.Bind(TextureUnit.Texture0 + 2 * i - 1,
                        shader.GetUniformLocation("alphaMap" + (i - 1)));
                }
            }

            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedShort, (IntPtr)IndicesOffset);
        }
    }
}
