using nfklib.NMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nfklib.NDemo
{
    public class DemoItem
    {
        public List<TDSpawnPlayerV2> Players = new List<TDSpawnPlayerV2>();
        public List<TDStats3> PlayerStats = new List<TDStats3>();
        public List<DemoUnitItem> DemoUnits = new List<DemoUnitItem>();

        public int Duration;

        public MapItem Map;
    }

    public class DemoUnitItem
    {
        public TDData DData;
        public Object DemoUnit;
    }
}
