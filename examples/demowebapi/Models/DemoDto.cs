using nfklib.NDemo;
using nfklib.NMap;

namespace demowebapi.Models
{
    public class DemoDto
    {
        public string? FileName { get; set; }
        public int Duration { get; set; }
        public byte Version { get; set; }
        public PlayerItem[] Players { get; set; } = Array.Empty<PlayerItem>();
        public List<DemoUnitItem>? DemoUnits { get; set; }
        public THeader MapInfo { get; set; }
        public MapItem? Map { get; set; }

        public struct PlayerItem
        {
            public byte ID { get; set; }
            public string? RealName { get; set; }
            public nfklib.TDSpawnPlayerV2 PlayerInfo { get; set; }
            public nfklib.TDStats3 PlayerStats { get; set; }
        }
    }
}
