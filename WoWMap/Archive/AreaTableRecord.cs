using System.Runtime.InteropServices;

namespace WoWMap.Archive
{
    // Structure is highly fucked, don't care
    [StructLayout(LayoutKind.Sequential)]
    public class AreaTableRecord
    {
        public int ID { get; set; }
        public int ContinentID { get; set; }
        public int ParentAreaID { get; set; }
        public int AreaBit { get; set; }
        public int Flags1 { get; set; }
        public int Flags2 { get; set; }
        public int SoundProviderPref { get; set; }
        public int SoundProviderPrefUnderwater { get; set; }
        public int AmbienceID { get; set; }
        public int ZoneMusic { get; set; }
        public string ZoneName { get; set; }
        public int IntroSound { get; set; }
        public int ExplorationLevel { get; set; }
        public string AreaName { get; set; }
        public int FactionGroupMask { get; set; }
        public int LiquidTypeID1 { get; set; }
        public int LiquidTypeID2 { get; set; }
        public int LiquidTypeID3 { get; set; }
        public int LiquidTypeID4 { get; set; }
        public float MinElevation { get; set; }
        public float AmbientMultiplier { get; set; }
        public int LightId { get; set; }
        public int MountFlags { get; set; }
        public int uwIntroSound { get; set; }
        public int uwZoneMusic { get; set; }
        public int Ambience { get; set; }
        public int WorldPvpId { get; set; }
        public int PvpCombatWorldStateID { get; set; }
        public int WildBattlePetLevelMin { get; set; }
        //public int WildBattlePetLevelMax { get; set; }
        //public int WindSettingsID { get; set; }
    }
}
