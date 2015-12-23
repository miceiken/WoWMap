using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;
using WoWMap.Chunks;
using WoWMap.Geometry;

namespace WoWMap.Builders
{
    public class TileBuilder
    {
        public TileBuilder(string world, int x, int y)
        {
            Source = new ADT(world, x, y);
        }

        private ADT Source;

        public void Build()
        {
            var geo = new Geometry.Geometry();
            Source.Read();
            geo.AddADT(Source);
        }
    }
}
