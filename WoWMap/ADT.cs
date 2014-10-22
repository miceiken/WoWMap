using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using WoWMap.Chunks;
using WoWMap.Geometry;
using System.Globalization;

namespace WoWMap
{
    public class ADT
    {
        public ADT(string filename)
        {
            Data = new ChunkData(filename);
        }

        public ADT(string world, int x, int y)
            : this(string.Format(@"World\Maps\{0}\{0}_{1}_{2}.adt", world, x, y))
        { }

        public ChunkData Data { get; private set; }
        public MapChunk[] MapChunks { get; private set; }
        public Liquid Liquid { get; private set; }
        public MHDR Header { get; private set; }

        public void Read()
        {
            Header = new MHDR();
            Header.Read(Data.GetChunkByName("MHDR").GetReader());

            MapChunks = new MapChunk[16 * 16];
            int idx = 0;
            foreach (var mapChunk in Data.Chunks.Where(c => c.Name == "MCNK"))
                MapChunks[idx++] = new MapChunk(this, mapChunk);

            Liquid = new Liquid(this, Data.GetChunkByName("MH2O"));

            foreach (var mapChunk in MapChunks)
                mapChunk.GenerateTriangles();
        }

        public void SaveObj(string filename)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<Triangle<uint>>();
            foreach (var mapChunk in MapChunks)
            {
                var vo = (uint)vertices.Count;
                vertices.AddRange(mapChunk.Vertices);
                foreach (var triangle in mapChunk.Triangles)
                    triangles.Add(new Triangle<uint>(triangle.Type, triangle.V0 + vo, triangle.V1 + vo, triangle.V2 + vo));
            }

            using (var sw = new StreamWriter(filename, false))
            {
                foreach (var v in vertices)
                    sw.WriteLine("v " + v.X.ToString(CultureInfo.InvariantCulture) + " " + v.Z.ToString(CultureInfo.InvariantCulture) + " " + v.Y.ToString(CultureInfo.InvariantCulture));
                foreach (var t in triangles)
                    sw.WriteLine("f " + (t.V0+1) + " " + (t.V1+1) + " " + (t.V2+1));
            }
        }
    }
}
