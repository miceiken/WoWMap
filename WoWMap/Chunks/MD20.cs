using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Readers;
using WoWMap.Geometry;
using OpenTK;

namespace WoWMap.Chunks
{
    // Technicly not a chunk, but who cares
    public class MD20 : ChunkReader
    {
        public MD20(Chunk c) : base(c) { }
        public MD20(Chunk c, uint s) : base(c, s) { }

        public byte[] Magic;
        public uint Version;
        public uint LengthModelName;
        public uint OffsetName;
        public uint ModelFlags;
        public uint CountGlobalSequences;
        public uint OffsetGlobalSequences;
        public uint CountAnimations;
        public uint OffsetAnimations;
        public uint CountAnimationLookup;
        public uint OffsetAnimationLookup;
        public uint CountBones;
        public uint OffsetBones;
        public uint CountKeyBoneLookup;
        public uint OffsetKeyBoneLookup;
        public uint CountVertices;
        public uint OffsetVertices;
        public uint CountViews;
        public uint CountColors;
        public uint OffsetColors;
        public uint CountTextures;
        public uint OffsetTextures;
        public uint CountTransparency;
        public uint OffsetTransparency;
        public uint CountUvAnimation;
        public uint OffsetUvAnimation;
        public uint CountTexReplace;
        public uint OffsetTexReplace;
        public uint CountRenderFlags;
        public uint OffsetRenderFlags;
        public uint CountBoneLookup;
        public uint OffsetBoneLookup;
        public uint CountTexLookup;
        public uint OffsetTexLookup;
        public uint CountTexUnits;
        public uint OffsetTexUnits;
        public uint CountTransLookup;
        public uint OffsetTransLookup;
        public uint CountUvAnimLookup;
        public uint OffsetUvAnimLookup;
        public Vector3[] VertexBox;
        public float VertexRadius;
        public Vector3[] BoundingBox;
        public float BoundingRadius;
        public uint CountBoundingTriangles;
        public uint OffsetBoundingTriangles;
        public uint CountBoundingVertices;
        public uint OffsetBoundingVertices;
        public uint CountBoundingNormals;
        public uint OffsetBoundingNormals;

        public static readonly uint ChunkHeaderSize = 0x144;

        public override void Read()
        {
            var br = Chunk.GetReader();
            Magic = br.ReadBytes(4);
            Version = br.ReadUInt32();
            LengthModelName = br.ReadUInt32();
            OffsetName = br.ReadUInt32();
            ModelFlags = br.ReadUInt32();
            CountGlobalSequences = br.ReadUInt32();
            OffsetGlobalSequences = br.ReadUInt32();
            CountAnimations = br.ReadUInt32();
            OffsetAnimations = br.ReadUInt32();
            CountAnimationLookup = br.ReadUInt32();
            OffsetAnimationLookup = br.ReadUInt32();
            CountBones = br.ReadUInt32();
            OffsetBones = br.ReadUInt32();
            CountKeyBoneLookup = br.ReadUInt32();
            OffsetKeyBoneLookup = br.ReadUInt32();
            CountVertices = br.ReadUInt32();
            OffsetVertices = br.ReadUInt32();
            CountViews = br.ReadUInt32();
            CountColors = br.ReadUInt32();
            OffsetColors = br.ReadUInt32();
            CountTextures = br.ReadUInt32();
            OffsetTextures = br.ReadUInt32();
            CountTransparency = br.ReadUInt32();
            OffsetTransparency = br.ReadUInt32();
            CountUvAnimation = br.ReadUInt32();
            OffsetUvAnimation = br.ReadUInt32();
            CountTexReplace = br.ReadUInt32();
            OffsetTexReplace = br.ReadUInt32();
            CountRenderFlags = br.ReadUInt32();
            OffsetRenderFlags = br.ReadUInt32();
            CountBoneLookup = br.ReadUInt32();
            OffsetBoneLookup = br.ReadUInt32();
            CountTexLookup = br.ReadUInt32();
            OffsetTexLookup = br.ReadUInt32();
            CountTexUnits = br.ReadUInt32();
            OffsetTexUnits = br.ReadUInt32();
            CountTransLookup = br.ReadUInt32();
            OffsetTransLookup = br.ReadUInt32();
            CountUvAnimLookup = br.ReadUInt32();
            OffsetUvAnimLookup = br.ReadUInt32();
            VertexBox = new Vector3[2];
            VertexBox[0] = br.ReadVector3();
            VertexBox[1] = br.ReadVector3();
            VertexRadius = br.ReadSingle();
            BoundingBox = new Vector3[2];
            BoundingBox[0] = br.ReadVector3();
            BoundingBox[1] = br.ReadVector3();
            BoundingRadius = br.ReadSingle();
            CountBoundingTriangles = br.ReadUInt32();
            OffsetBoundingTriangles = br.ReadUInt32();
            CountBoundingVertices = br.ReadUInt32();
            OffsetBoundingVertices = br.ReadUInt32();
            CountBoundingNormals = br.ReadUInt32();
            OffsetBoundingNormals = br.ReadUInt32();
        }
    }
}
