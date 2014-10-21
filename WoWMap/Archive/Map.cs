using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMap.Archive
{
    public class MapRecord
    {
        public int ID { get; private set; }
        public string Directory { get; private set; }
        public int InstanceType { get; private set; }
        public int Flags { get; private set; }
        public int unk { get; private set; }
        public int MapType { get; private set; }
        public string Mapname_lang { get; private set; }
        public int areaTableID { get; private set; }
        public string MapDescription0_lang { get; private set; }
        public string MapDescription1_lang { get; private set; }
        public int LoadingScreenID { get; private set; }
        public float minimapIconScale { get; private set; }
        public int corpseMapID { get; private set; }
        public float corpse_x { get; private set; }
        public float corpse_y { get; private set; }
        public int timeOfDayoverride { get; private set; }
        public int expansionID { get; private set; }
        public int raidOffset { get; private set; }
        public int maxPlayers { get; private set; }
        public int parentMapID { get; private set; }
        public int cosmeticParentMapID { get; private set; }
        public int timeOffset { get; private set; }
    }
}
