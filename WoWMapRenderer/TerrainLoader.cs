using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using WoWMap.Geometry;
using WoWMap.Layers;

namespace WoWMapRenderer
{
    class GeometryLoader
    {
        private Geometry _geometry = new Geometry();
        private List<int> _ownedADTs = new List<int>();

        public int VertexBuffer;
        public int IndiceBuffer;

        public float[] Vertices
        {
            get
            {
                var l = new List<float>();
                foreach (var v in _geometry.Vertices)
                    l.AddRange(new [] { v.X, v.Y, v.Z });
                return l.ToArray();
            }
        }

        public uint[] Indices
        {
            get
            {
                var l = new List<uint>();
                foreach (var v in _geometry.Indices)
                    l.AddRange(new [] { v.V0, v.V1, v.V2 });
                return l.ToArray();
            }
        }

        public void AddADT(ADT adt)
        {
            if (HasADT(adt.X, adt.Y))
                return;

            _ownedADTs.Add((adt.X << 8) | adt.Y);
            _geometry.AddADT(adt);
        }

        public bool HasADT(int x, int y)
        {
            return _ownedADTs.Contains((x << 8) | y);
        }
    }
}
