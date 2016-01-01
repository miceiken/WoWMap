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
            public MCLY.MCLYFlags Flags;
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
            Materials = new List<Material>(mapChunk.MCLY.Count);
            for (var i = 0; i < mapChunk.MCLY.Count; ++i)
            {
                Materials.Add(new Material
                {
                    TextureID = mapChunk.MCLY[i].TextureId,
                    Flags = mapChunk.MCLY[i].Flags,
                    AlphaMapId = (int)mapChunk.MCLY[i].ofsMCAL
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

                Materials[i].Texture = TextureCache.GetRawTexture(textureName);
                Materials[i].Texture.InternalFormat = PixelInternalFormat.Rgba;
                Materials[i].Texture.Format = PixelFormat.Rgba;
                Materials[i].Texture.WrapS = (int)All.Repeat;
                Materials[i].Texture.WrapT = (int)All.Repeat;
            }
        }

        public void ApplyAlphaMap(MCAL mcalChunk)
        {
            for (var i = 0; i < Materials.Count; ++i)
            {
                if (mcalChunk == null || !mcalChunk.HasAlpha(Materials[i].AlphaMapId))
                    continue;

                Materials[i].AlphaTexture = new Texture(mcalChunk.GetAlpha(Materials[i].AlphaMapId), 64, 64);
                // TODO: Luminance is legacy.
                Materials[i].AlphaTexture.InternalFormat = PixelInternalFormat.Luminance;
                Materials[i].AlphaTexture.Format = PixelFormat.Luminance;
                Materials[i].AlphaTexture.MagFilter = (int)All.Linear;
                Materials[i].AlphaTexture.MinFilter = (int)All.Linear;
            }
        }

        public void Render(Shader shader)
        {
            // Bind textures and samplers
            GL.Uniform1(shader.GetUniformLocation("layerCount"), LayerCount);

            // This should use up at most 7 texture units - should be fine on even the shittyest GPU.
            for (var i = 0; i < Materials.Count; ++i)
            {
                var textureInfo = Materials[i];
                textureInfo.Texture.BindToUnit(TextureUnit.Texture1 + 2 * i);

                if (i > 0 && textureInfo.AlphaTexture != null)
                    textureInfo.AlphaTexture.BindToUnit(TextureUnit.Texture1 + 2 * i - 1);
            }

            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedShort, (IntPtr)IndicesOffset);
        }
    }
}
