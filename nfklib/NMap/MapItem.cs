using System;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

namespace nfklib.NMap
{
    [Serializable]
    public class MapItem : IDisposable
    {
        public THeader Header = new THeader();
        public TMapObj Map = new TMapObj();

        public byte[][] Bricks = Array.Empty<byte[]>();
        public TMapObj[] Objects = Array.Empty<TMapObj>();
        public TMapEntry PaletteEntry;
        /// <summary>
        /// Decoded palette image (unmanaged memory, dispose it when not needed); the raw bytes are in <see cref="PaletteBytes"/>
        /// </summary>
        [JsonIgnore]
        public Image? Palette;
        public byte[]? PaletteBytes;
        public TMapEntry LocationEntry;
        public TLocationText[]? Locations;

        /// <summary>
        /// Releases the decoded palette image (all other data stays usable)
        /// </summary>
        public void Dispose()
        {
            Palette?.Dispose();
            Palette = null;
        }
    }
}
