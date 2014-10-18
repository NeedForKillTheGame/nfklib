using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using nfklib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace nfklib.NMap
{
    class NFKMap
    {
        public const int BrickWidth = 32;
        public const int BrickHeight = 16;
        const string MapHeader = "NMAP"; // normal map file
        const string MapInDemoHeader = "NDEM"; // map nested in demo file

        private MapInfo map;

        public NFKMap()
        {
            map = new MapInfo();
        }

        public MapInfo Read(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                using (var br = new BinaryReader(fs, Encoding.ASCII))
                {
                    map = Read(br);
                }
            }
            return map;
        }

        public MapInfo Read(BinaryReader br)
        {
            // map header
            map.Header = br.BaseStream.ReadStruct<THeader>();

            map.Bricks = new byte[map.Header.MapSizeX][];
            // read bricks (start at pos 154)
            for (int y = 0; y < map.Header.MapSizeY; y++)
            {
                for (int x = 0; x < map.Header.MapSizeX; x++)
                {
                    if (map.Bricks[x] == null)
                        map.Bricks[x] = new byte[map.Header.MapSizeY];
                    map.Bricks[x][y] = br.ReadByte();
                }
            }

            map.Objects = new TMapObj[map.Header.numobj];
            // read objects
            for (int i = 0; i < map.Header.numobj; i++)
                map.Objects[i] = br.BaseStream.ReadStruct<TMapObj>();

            // read pal and loc blocks
            while (br.BaseStream.Length > br.BaseStream.Position)
            {
                var entry = br.BaseStream.ReadStruct<TMapEntry>();

                // palette
                if (new string(entry.EntryType).EndsWith("pal"))
                {
                    map.PaletteEntry = entry;

                    var palette_data = br.ReadBytes(entry.DataSize);
                    // map nested in demo is not compressed
                    var palette_bin = (new string(map.Header.ID) == MapInDemoHeader)
                        ? palette_data
                        : Helper.BZDecompress(palette_data);

#if DEBUG
                    map.Palette = new Bitmap(new MemoryStream(palette_bin));
                    map.Palette.Save("pal.bmp", ImageFormat.Bmp);
#endif
                    if (entry.Reserved6 == 1)
                    {
                        // TODO: set transparent color
                    }
                }
                // locations
                else if (new string(entry.EntryType).EndsWith("loc"))
                {
                    var loc_count = entry.DataSize / Marshal.SizeOf(typeof(TLocationText));
                    map.Locations = new TLocationText[loc_count];
                    map.LocationEntry = entry;
                    for (var i = 0; i < loc_count; i++)
                    {
                        map.Locations[i] = br.BaseStream.ReadStruct<TLocationText>();
                    }
                }
                // end of map
                else
                {
                    break;
                }
            }
            return map;

        }
    }
}
