using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nfklib.NDemo;
using nfklib.NMap;

namespace WindowsServiceTemplate
{
    public class Demo
    {
        public string FileName;
        public int Duration;
        public byte Version;
        public PlayerItem[] Players;
        public List<DemoUnitItem> DemoUnits;
        public THeader MapInfo;
        public MapItem Map;

        public struct PlayerItem
        {
            public byte ID;
            public string RealName;
            public nfklib.TDSpawnPlayerV2 PlayerInfo;
            public nfklib.TDStats3 PlayerStats;
        }
    }
}
