using System.Collections.Generic;
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

        private Dictionary<uint, byte[]> alphaTextures = new Dictionary<uint, byte[]>(4);

        public bool HasAlpha(int mcalOffset)
        {
            return alphaTextures.ContainsKey((uint)mcalOffset);
        }

        public byte[] GetAlpha(int mcalOffset)
        {
            return alphaTextures[(uint)mcalOffset];
        }

        public override void Read()
        {
            var br = Chunk.GetReader();
            var chunkData = br.ReadBytes((int)Chunk.Size);

            for (var i = 0; i < _mapChunk.MCLY.Entries.Length; ++i)
            {
                if (!_mapChunk.MCLY.Entries[i].HasFlag(MCLY.MCLYFlags.AlphaMap))
                    continue;

                var layerRead = 0;
                var alphaOffset = _mapChunk.MCLY.Entries[i].ofsMCAL;
                alphaTextures[_mapChunk.MCLY.Entries[i].ofsMCAL] = new byte[64 * 64];
                var outputOffset = 0;

                if (_mapChunk.MCLY.Entries[i].HasFlag(MCLY.MCLYFlags.CompressedAlphaMap))
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

                            alphaTextures[_mapChunk.MCLY.Entries[i].ofsMCAL][outputOffset++] = chunkData[alphaOffset];
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
                            alphaTextures[_mapChunk.MCLY.Entries[i].ofsMCAL][outputOffset++] = chunkData[alphaOffset++];

                            ++layerRead;
                        }
                    }
                }
                else
                {
                    for (var x = 0; x < 64; ++x)
                    {
                        for (var y = 0; y < 32; ++y)
                        {
                            alphaTextures[_mapChunk.MCLY.Entries[i].ofsMCAL][outputOffset++] = (byte)(((chunkData[alphaOffset] & 0xf0) >> 4) * 17);
                            alphaTextures[_mapChunk.MCLY.Entries[i].ofsMCAL][outputOffset++] = (byte)((chunkData[alphaOffset++] & 0x0f) * 17);

                            layerRead += 2;
                        }
                    }
                }
            }
        }
    }
}
