using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace nfklib.NMap
{
    public class MapItem
    {
        public THeader Header = new THeader();
        public TMapObj Map = new TMapObj();

        public byte[][] Bricks;
        public TMapObj[] Objects;
        public TMapEntry PaletteEntry;
        public Bitmap Palette;
        public TMapEntry LocationEntry;
        public TLocationText[] Locations;
    }
}
