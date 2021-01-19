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
        public const string MAPHEADER = "NMAP"; // normal map file
        public const string MAPINDEMOHEADER = "NDEM"; // map nested in demo file
        public const byte MAPVERSION = 3;
        public readonly char[] PALHEADER = new char[] { (char)3, 'p', 'a', 'l' };
        public MapItem map;

        public NFKMap()
        {
            map = new MapItem();
        }

        public MapItem NewMap(byte width = 20, byte height = 30)
        {
            map.Header.ID = MAPHEADER.ToCharArray();
            map.Header.Version = MAPVERSION;

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
                return Read(fs);
            }
        }

        public MapItem Read(Stream stream)
        {
            using (var br = new BinaryReader(stream, Encoding.ASCII))
            {
                return Read(br);
            }
        }

        public MapItem Read(BinaryReader br)
        {
            // map header
            map.Header = br.BaseStream.ReadStruct<THeader>();
            map.Header.MapName = Helper.GetDelphiString(map.Header.MapName);
            map.Header.Author = Helper.GetDelphiString(map.Header.Author);

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
                    map.PaletteBytes = (new string(map.Header.ID) == MAPINDEMOHEADER)
                        ? palette_data
                        : Helper.BZDecompress(palette_data);

#if DEBUG
                    File.WriteAllBytes("map_palette.pal", map.PaletteBytes);
#endif
                    try
                    {
                        // 1) try without fixing
                        map.Palette = new Bitmap(new MemoryStream(map.PaletteBytes));
                    }
                    catch
                    {
                        try
                        {
                            // 1) fix it and try again
                            fixBitmapBin(ref palette_data);
#if DEBUG
                            File.WriteAllBytes("map_palette_fixed.pal", map.PaletteBytes);
#endif
                            map.Palette = new Bitmap(new MemoryStream(map.PaletteBytes));
                        }
                        catch
                        {
                            // this code should never be thrown
                            throw new Exception("Could not read palette");
                        }
                    }
                    

                    if (entry.Reserved6 == 1)
                    {
                        // set transparent color
                        var color = Color.FromArgb(entry.Reserved5);
                        if (map.Palette != null)
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
                        map.Locations[i].text = Helper.GetDelphiString(map.Locations[i].text);
                    }
                }
                // end of map
                else
                {
                    // restore position to correctly read demo
                    br.BaseStream.Seek(-(Marshal.SizeOf(typeof(TMapEntry))), SeekOrigin.Current);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bw"></param>
        /// <param name="demoMap">true for map inside demo</param>
        public void Write(BinaryWriter bw, bool demoMap = false)
        {
            if (demoMap)
            {
                map.Header.ID = MAPINDEMOHEADER.ToCharArray();
                map.Header.Version = 7; // FIXME: if demo was written in previous versions it overwrite
            }
            else
            {
                map.Header.ID = MAPHEADER.ToCharArray();
                map.Header.Version = MAPVERSION;
            }
            map.Header.MapName = Helper.SetDelphiString(map.Header.MapName, 71);
            map.Header.Author = Helper.SetDelphiString(map.Header.Author, 71);
            if (map.Objects != null)
                map.Header.numobj = (byte)map.Objects.Length;

            // map header
            bw.Write(StreamExtensions.ToByteArray<THeader>(map.Header));

            // restore strings for further usage
            map.Header.MapName = Helper.GetDelphiString(map.Header.MapName);
            map.Header.Author = Helper.GetDelphiString(map.Header.Author);
            
            // write bricks 
            for (int y = 0; y < map.Header.MapSizeY; y++)
            {
                for (int x = 0; x < map.Header.MapSizeX; x++)
                {
                    bw.BaseStream.WriteByte(map.Bricks[x][y]);
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
                // bitmap bytes
                var paletteBytes = map.Palette.ToByteArray(ImageFormat.Bmp);
                var palettebin = demoMap
                    ? paletteBytes
                    : Helper.BZCompress(paletteBytes);

                map.PaletteEntry.EntryType = PALHEADER;
                map.PaletteEntry.DataSize = palettebin.Length;
                // entries
                bw.Write(StreamExtensions.ToByteArray<TMapEntry>(map.PaletteEntry));
                bw.Write(palettebin);
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
                    // restore string for further usage
                    map.Locations[i].text = Helper.GetDelphiString(map.Locations[i].text);
                }
            }
        }

        /// <summary>
        /// https://github.com/HarpyWar/nfkmap-viewer/wiki/BMP-картинка-палитры
        /// </summary>
        /// <param name="data"></param>
        private void fixBitmapBin(ref byte[] data)
        {
            int dataSize = data.Length;
            int bitmapDataSize;

            using (var ms = new MemoryStream(data))
            {
                using (var br = new BinaryReader(ms))
                {
                    // read bitmap data length
                    ms.Seek(0x22, SeekOrigin.Begin);
                    bitmapDataSize = br.ReadInt32();

                    using (var bw = new BinaryWriter(ms))
                    {
                        // write whole data size
                        bw.Seek(0x02, SeekOrigin.Begin);
                        bw.Write(dataSize);

                        // do nothing if BMP Version 2 or 3
                        //ms.Seek(0x0E, SeekOrigin.Begin);
                        //var header_ver = br.ReadByte();
                        // FIXME: sometimes delphi palette header version is messed up and it does not work
                        //        I use the function only after exception above, if loading bmp without fix failed
                        //if (header_ver != 0xC) // 12
                        //{
                            // write fixed data offset
                            bw.Seek(0x0A, SeekOrigin.Begin);
                            bw.Write(dataSize - bitmapDataSize);
                        //}
                    }
                }
            }
        }
    }
}
