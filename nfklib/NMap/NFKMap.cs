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
    public class NFKMap
    {
        public const int BrickWidth = 32;
        public const int BrickHeight = 16;
        const string MapHeader = "NMAP"; // normal map file
        const string MapInDemoHeader = "NDEM"; // map nested in demo file

        public MapItem map;

        public NFKMap()
        {
            map = new MapItem();
        }

        public MapItem NewMap(byte width = 20, byte height = 30)
        {
            map.Header.ID = MapHeader.ToCharArray();
            map.Header.Version = 3;

            map.Header.MapSizeX = width;
            map.Header.MapSizeY = height;
            map.Header.MapName = "test map";
            map.Header.Author = "unnamed";

            // allocate bricks array
            map.Bricks = new byte[width][];
            for (int y = 0; y < map.Header.MapSizeY; y++)
            {
                for (int x = 0; x < map.Header.MapSizeX; x++)
                {
                    if (map.Bricks[x] == null)
                        map.Bricks[x] = new byte[map.Header.MapSizeY];
                }
            }

            return map;
        }

        public MapItem Read(string fileName)
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

        public MapItem Read(BinaryReader br)
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

                    map.Palette = new Bitmap(new MemoryStream(palette_bin));
                    if (entry.Reserved6 == 1)
                    {
                        // set transparent color
                        var color = Color.FromArgb(entry.Reserved5);
                        map.Palette.MakeTransparent(color);
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

        public void Write(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                using (var bw = new BinaryWriter(fs, Encoding.ASCII))
                {
                    Write(bw);
                }
            }
        }

        public void Write(BinaryWriter bw)
        {
            map.Header.MapName = Helper.SetDelphiString(map.Header.MapName, 71);
            map.Header.Author = Helper.SetDelphiString(map.Header.Author, 71);
            if (map.Objects != null)
                map.Header.numobj = (byte)map.Objects.Length;

            // map header
            bw.Write(StreamExtensions.ToByteArray<THeader>(map.Header));

            // write bricks 
            for (int y = 0; y < map.Header.MapSizeY; y++)
            {
                for (int x = 0; x < map.Header.MapSizeX; x++)
                {
                    bw.Write(map.Bricks[x][y]);
                }
            }
            // write objects
            if (map.Objects != null)
            {
                for (int i = 0; i < map.Header.numobj; i++)
                {
                    map.Objects[i].unknown0 = map.Objects[i].unknown1 = 0x03;
                    bw.Write(StreamExtensions.ToByteArray<TMapObj>(map.Objects[i]));
                }
            }

            if (map.Palette != null)
            {
                // entry
                bw.Write(StreamExtensions.ToByteArray<TMapEntry>(map.PaletteEntry));
                // bitmap bytes
                var palettebin = map.Palette.ToByteArray(ImageFormat.Bmp);
                bw.Write(Helper.BZCompress(palettebin));
            }
            if (map.Locations != null)
            {
                // entry
                bw.Write(StreamExtensions.ToByteArray<TMapEntry>(map.LocationEntry));
                // locations array
                for (var i = 0; i < map.Locations.Length; i++)
                {
                    map.Locations[i].text = Helper.SetDelphiString(map.Locations[i].text, 65);
                    bw.Write(StreamExtensions.ToByteArray<TLocationText>(map.Locations[i]));
                }
            }
        }
    }
}
