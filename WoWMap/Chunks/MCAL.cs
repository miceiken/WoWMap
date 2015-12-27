using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;
using WoWMap.Readers;

namespace WoWMap.Chunks
{
    public class MCAL : ChunkReader
    {
        public MCAL(MapChunk chunk, WDT wdt, Chunk c) : base(c)
        {
            Debug.Assert(wdt != null, "WDT Cannot be null in order to read MCAL!");
            _mapChunk = chunk;
            _wdt = wdt;
        }

        private MapChunk _mapChunk;
        private WDT _wdt;

        public MCAL(Chunk c, uint h) : base(c, h) { }
        public MCAL(Chunk c) : base(c, c.Size) { }

        private byte[][] alphaTextures;

        public byte[] this[int layerIndex]
        {
            get { return alphaTextures[Math.Min(_mapChunk.MCLY.Count, Math.Max(0, layerIndex))]; }
        }

        public override void Read()
        {
            var br = Chunk.GetReader();
            var chunkData = br.ReadBytes((int)Chunk.Size);

            alphaTextures = new byte[_mapChunk.MCLY.Count][];

            for (var i = 0; i < _mapChunk.MCLY.Count; ++i)
            {
                alphaTextures[i] = new byte[64 * 64 * 4]; // TODO wat * 4
                var layerRead = 0;
                var alphaOffset = _mapChunk.MCLY[i].ofsMCAL;
                var outputOffset = 64 * i;
                var readCount = 0;

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
                            ++readCount;
                            ++layerRead;

                            if (readCount >= 64)
                            {
                                outputOffset += 64 * 3;
                                readCount = 0;
                                ++layerRead;
                            }

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

                            ++outputOffset; ++readCount; ++layerRead; ++alphaOffset;
                            if (readCount < 64)
                                continue;

                            outputOffset += 64 * 3;
                            readCount = 0;
                        }
                    }
                }
                else
                {
                    for (var x = 0; x < 64; ++x)
                    {
                        for (var y = 0; y < 64; ++y)
                        {
                            alphaTextures[i][outputOffset++] = (byte)(((chunkData[alphaOffset] & 0xf0) >> 4) * 17);
                            alphaTextures[i][outputOffset++] = (byte)((chunkData[alphaOffset] & 0x0f) * 17);

                            readCount += 2; layerRead += 2;
                            ++alphaOffset;
                            if (readCount != 64)
                                continue;

                            outputOffset += 63 * 3;
                            readCount = 0;
                        }
                    }
                }
            }
        }
    }
}
