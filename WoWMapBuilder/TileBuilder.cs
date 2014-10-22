using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMapBuilder
{
    public class TileBuilder
    {
        public TileBuilder(string world, int x, int y)
        {
            World = world;
            X = x;
            Y = y;
        }

        public TileBuilder(string world, int x, int y, int mapid)
            :this(world, x, y)
        {
            MapId = mapid;
        }

        public string World { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int MapId { get; private set; }
    }
}
