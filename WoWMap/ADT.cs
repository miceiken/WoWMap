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
        public ChunkLiquid Liquid { get; private set; }
        public MHDR Header { get; private set; }

        public void Read()
        {
            Header = new MHDR();
            Header.Read(Data.GetChunkByName("MHDR").GetReader());

            MapChunks = new MapChunk[16 * 16];
            int idx = 0;
            foreach (var mapChunk in Data.Chunks.Where(c => c.Name == "MCNK"))
                MapChunks[idx++] = new MapChunk(this, mapChunk);

            Liquid = new ChunkLiquid(this, Data.GetChunkByName("MH2O"));

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
                //vertices.AddRange(mapChunk.Vertices.Select(v => v.ToRecast()));
                vertices.AddRange(mapChunk.Vertices);
                triangles.AddRange(mapChunk.Triangles.Select(t => new Triangle<uint>(t.Type, t.V0 + vo, t.V1 + vo, t.V2 + vo)));
            }

            using (var sw = new StreamWriter(filename, false))
            {
                sw.WriteLine("o " + filename);
                var nf = CultureInfo.InvariantCulture.NumberFormat;
                foreach (var v in vertices)
                    sw.WriteLine("v " + v.X.ToString(nf) + " " + v.Y.ToString(nf) + " " + v.Z.ToString(nf));
                foreach (var t in triangles)
                    sw.WriteLine("f " + (t.V0 + 1) + " " + (t.V1 + 1) + " " + (t.V2 + 1));
            }
        }
    }
}
