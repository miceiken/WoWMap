using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMap.Archive
{
    public class MapRecord
    {
        public int ID { get; set; }
        public string Directory { get; set; }
        public int InstanceType { get; set; }
        public int Flags { get; set; }
        public int unk { get; private set; } // <---
        public int MapType { get; set; }
        public string MapNameLang { get; set; }
        public int AreaTableID { get; set; }
        public string MapDescription0Lang { get; set; }
        public string MapDescription1Lang { get; set; }
        public int LoadingScreenID { get; set; }
        public float MinimapIconScale { get; set; }
        public int CorpseMapID { get; set; }
        public float CorpseX { get; set; }
        public float CorpseY { get; set; }
        public int TimeOfDayOverride { get; set; }
        public int ExpansionID { get; set; }
        public int RaidOffset { get; set; }
        public int MaxPlayers { get; set; }
        public int ParentMapID { get; set; }
        public int CosmeticParentMapID { get; set; }
        public int TimeOffset { get; set; }
    }
}
