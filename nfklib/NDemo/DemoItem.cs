using nfklib.NMap;
using System;
using System.Collections.Generic;

namespace nfklib.NDemo
{
    [Serializable]
    public class DemoItem : IDisposable
    {
        public List<TDSpawnPlayerV2> Players = new List<TDSpawnPlayerV2>();
        public List<TDStats3> PlayerStats = new List<TDStats3>();
        public List<DemoUnitItem> DemoUnits = new List<DemoUnitItem>();

        public int Duration;

        public MapItem? Map;

        /// <summary>
        /// Releases the decoded palette image of the map (all other data stays usable)
        /// </summary>
        public void Dispose()
        {
            Map?.Dispose();
        }
    }

    [Serializable]
    public class DemoUnitItem
    {
        public TDData DData;
        public object DemoUnit;

        public DemoUnitItem(TDData data, object demoUnit)
        {
            DData = data;
            DemoUnit = demoUnit;
        }
    }
}
