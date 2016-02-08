using System.Diagnostics;
using WoWMap.Layers;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public sealed class MCAL : ChunkReader
    {
        public MCAL(MapChunk chunk, WDT wdt, Chunk c) : base(c, false)
        {
            Debug.Assert(wdt != null, "WDT Cannot be null in order to read MCAL!");
            _mapChunk = chunk;
            _wdt = wdt;

            Read();
        }

        private MapChunk _mapChunk;
        private WDT _wdt;

        public MCAL(Chunk c, uint h) : base(c, h) { }
        public MCAL(Chunk c) : base(c, c.Size) { }

        private byte[][] alphaTextures;

        public byte[] GetAlpha(int alphaTextureIndex)
        {
            return alphaTextures[alphaTextureIndex];
        }

        public override void Read()
        {
            var br = Chunk.GetReader();
            var chunkData = br.ReadBytes((int)Chunk.Size);

            alphaTextures = new byte[_mapChunk.MCLY.Length][];

            for (var i = 0; i < _mapChunk.MCLY.Length; ++i)
            {
                if (_mapChunk.MCLY[i] == null)
                    continue;

                var layerRead = 0;
                var alphaOffset = _mapChunk.MCLY[i].ofsMCAL;
                alphaTextures[i] = new byte[64 * 64];
                var outputOffset = 64 * i;

                if (_mapChunk.MCLY[i].HasFlag(MCLY.MCLYFlags.CompressedAlphaMap))
                {
                    while (layerRead != 4096)
                    {
                        var doFill = (chunkData[alphaOffset] & 0x80) != 0;
                        var maxCount = chunkData[alphaOffset] & 0x7F;
                        ++alphaOffset;

                        for (var k = 0; k < maxCount; ++k)
                        {
                            if (layerRead == 4096)
                                break;

                            alphaTextures[i][outputOffset++] = chunkData[alphaOffset];
                            ++layerRead;

                            if (!doFill)
                                ++alphaOffset;
                        }
                        if (doFill)
                            ++alphaOffset;
                    }
                }
                else if (_wdt.MPHD.HasFlag(MPHD.MPHDFlags.TerrainShaders | MPHD.MPHDFlags.SomethingShader))
                {
                    for (var x = 0; x < 64; ++x)
                    {
                        for (var y = 0; y < 64; ++y)
                        {
                            alphaTextures[i][outputOffset] = chunkData[alphaOffset];

                            ++outputOffset; ++layerRead; ++alphaOffset;
                        }
                    }
                }
                else
                {
                    for (var x = 0; x < 64; ++x)
                    {
                        for (var y = 0; y < 32; ++y)
                        {
                            alphaTextures[i][outputOffset++] = (byte)(((chunkData[alphaOffset] & 0xf0) >> 4) * 17);
                            alphaTextures[i][outputOffset++] = (byte)((chunkData[alphaOffset++] & 0x0f) * 17);

                            layerRead += 2;
                        }
                    }
                }
            }
        }
    }
}
