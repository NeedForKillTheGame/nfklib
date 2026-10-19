using demowebapi.Models;
using nfklib;
using nfklib.NDemo;

namespace demowebapi.Services
{
    /// <summary>
    /// Converts a NFK demo file into the <see cref="DemoDto"/> the demo viewer expects
    /// </summary>
    public static class DemoParser
    {
        /// <summary>
        /// Known values of the "type" parameter; anything else means "summary only"
        /// </summary>
        public static string NormalizeType(string? type) => (type ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "chat" => "chat",
            "map" => "map",
            "full" => "full",
            _ => string.Empty,
        };

        /// <summary>
        /// Parse the demo
        /// </summary>
        /// <param name="stream">demo file</param>
        /// <param name="type">normalized "type" parameter, see <see cref="NormalizeType"/></param>
        /// <param name="fileName">reported as <see cref="DemoDto.FileName"/></param>
        /// <exception cref="InvalidDataException">the file is not a demo</exception>
        public static DemoDto Parse(Stream stream, string type, string? fileName)
        {
            // the decoded palette image is released when parsing is done, only its raw bytes are returned
            using var demo = new NFKDemo().Read(stream);
            if (demo.Map == null)
                throw new InvalidDataException("Bad demo file");

            var data = new DemoDto
            {
                FileName = fileName,
                Duration = demo.Duration,
                Players = new DemoDto.PlayerItem[demo.Players.Count],
                Version = demo.Map.Header.Version,
                MapInfo = demo.Map.Header,
            };

            for (int i = 0; i < demo.Players.Count; i++)
            {
                data.Players[i].ID = (byte)(i + 1);
                data.Players[i].RealName = demo.Players[i].realname;
                data.Players[i].PlayerInfo = demo.Players[i];

                foreach (var s in demo.PlayerStats)
                    if (s.DXID == demo.Players[i].DXID)
                        data.Players[i].PlayerStats = s;
            }

            switch (type)
            {
                case "chat":
                    data.DemoUnits = demo.DemoUnits.Where(t => t.DData.type0 == DemoUnit.DDEMO_CHATMESSAGE).ToList();
                    break;
                case "map":
                    data.Map = demo.Map;
                    break;
                case "full":
                    data.DemoUnits = demo.DemoUnits;
                    data.Map = demo.Map;
                    break;
            }

            return data;
        }
    }
}
