using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using nfklib;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Processing;

namespace nfklib.NMap
{
    public class NFKMap
    {
        public const int BrickWidth = 32;
        public const int BrickHeight = 16;
        public const string MAPHEADER = "NMAP"; // normal map file
        public const string MAPINDEMOHEADER = "NDEM"; // map nested in demo file
        public const byte MAPVERSION = 3;
        public const string PALHEADER = "pal";
        public const string LOCHEADER = "loc";
        public MapItem map;

        /// <summary>
        /// Upper bound for the decoded palette image (width * height). Palettes of real maps are a few
        /// hundred pixels wide, the limit only guards against image bombs in crafted files.
        /// </summary>
        public static long MaxPalettePixels { get; set; } = 4096L * 4096;

        /// <summary>
        /// Hard cap for the memory the image decoder may allocate for one palette
        /// </summary>
        public const int MaxPaletteDecoderMegabytes = 256;

        private static readonly Lazy<Configuration> _paletteConfiguration = new(() =>
        {
            var configuration = Configuration.Default.Clone();
            configuration.MemoryAllocator = MemoryAllocator.Create(new MemoryAllocatorOptions
            {
                AllocationLimitMegabytes = MaxPaletteDecoderMegabytes
            });
            return configuration;
        });

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
            using (var br = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
            {
                return Read(br);
            }
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
            var entrySize = Marshal.SizeOf<TMapEntry>();
            while (br.BaseStream.Length - br.BaseStream.Position >= entrySize)
            {
                var entry = br.BaseStream.ReadStruct<TMapEntry>();
                var entryType = new string(entry.EntryType);
                var isPalette = entryType.EndsWith(PALHEADER);
                var isLocations = entryType.EndsWith(LOCHEADER);

                // end of map
                if (!isPalette && !isLocations)
                {
                    // restore position to correctly read demo
                    br.BaseStream.Seek(-entrySize, SeekOrigin.Current);
                    break;
                }

                if (entry.DataSize < 0 || entry.DataSize > br.BaseStream.Length - br.BaseStream.Position)
                    throw new InvalidDataException($"Bad size of the map \"{Helper.GetDelphiString(entryType)}\" entry: {entry.DataSize}");

                // palette
                if (isPalette)
                {
                    map.PaletteEntry = entry;

                    var palette_data = br.ReadBytes(entry.DataSize);
                    // map nested in demo is not compressed
                    var paletteBytes = (new string(map.Header.ID) == MAPINDEMOHEADER)
                        ? palette_data
                        : Helper.BZDecompress(palette_data);

                    map.Palette = LoadPalette(paletteBytes);
                    map.PaletteBytes = paletteBytes;

                    if (entry.Reserved6 == 1)
                    {
                        int a = (entry.Reserved5 >> 24) & 0xff; // or color >>> 24
                        int r = (entry.Reserved5 >> 16) & 0xff;
                        int g = (entry.Reserved5 >>  8) & 0xff;
                        int b = (entry.Reserved5) & 0xff;
                        // set transparent color
                        var color = Color.FromRgba((byte)r, (byte)g, (byte)b, (byte)a);
                        map.Palette.Mutate(x => x.BackgroundColor(color));
                    }
                }
                // locations
                else
                {
                    var loc_count = entry.DataSize / Marshal.SizeOf<TLocationText>();
                    map.Locations = new TLocationText[loc_count];
                    map.LocationEntry = entry;
                    for (var i = 0; i < loc_count; i++)
                    {
                        map.Locations[i] = br.BaseStream.ReadStruct<TLocationText>();
                    }
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

            if (map.Objects != null)
                map.Header.numobj = (byte)map.Objects.Length;
            
            // map header
            bw.Write(StreamExtensions.ToByteArray<THeader>(map.Header));
            
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
                var paletteBytes = map.Palette.ToByteArray(new BmpEncoder());
                var palettebin = demoMap
                    ? paletteBytes
                    : Helper.BZCompress(paletteBytes);

                // pal header
                map.PaletteEntry.EntryType = Helper.SetDelphiString(PALHEADER, 4).ToCharArray();
                map.PaletteEntry.DataSize = palettebin.Length;
                bw.Write(StreamExtensions.ToByteArray<TMapEntry>(map.PaletteEntry));
                // pal body
                bw.Write(palettebin);
            }
            if (map.Locations != null)
            {
                // loc header
                map.LocationEntry.EntryType = Helper.SetDelphiString(LOCHEADER, 4).ToCharArray();
                map.LocationEntry.DataSize = map.Locations.Length * 68; // 68 = sizeof(TLocationText)
                bw.Write(StreamExtensions.ToByteArray<TMapEntry>(map.LocationEntry));
                // locations array
                for (var i = 0; i < map.Locations.Length; i++)
                {
                    bw.Write(StreamExtensions.ToByteArray<TLocationText>(map.Locations[i]));
                }
            }
        }

        /// <summary>
        /// Decode the palette image. Any format ImageSharp knows is accepted, but the image must fit into
        /// <see cref="MaxPalettePixels"/> and the decoder works with a capped allocator and a single frame.
        /// </summary>
        /// <param name="data">image bytes; a broken Delphi BMP header is fixed in place (see <see cref="fixBitmapBin"/>)</param>
        /// <exception cref="InvalidDataException">the data is not an image, or it is too large</exception>
        private static Image LoadPalette(byte[] data)
        {
            var options = new DecoderOptions
            {
                Configuration = _paletteConfiguration.Value,
                MaxFrames = 1,
            };

            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    // header only: check the dimensions before anything is allocated for the pixels
                    var info = Image.Identify(options, data);
                    if ((long)info.Width * info.Height > MaxPalettePixels)
                        throw new InvalidDataException($"Palette image {info.Width}x{info.Height} is too large (limit {MaxPalettePixels} pixels)");

                    return Image.Load(options, data);
                }
                catch (InvalidDataException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    if (attempt > 0)
                        throw new InvalidDataException("Could not read palette: " + e.Message, e);

                    // Delphi sometimes writes a broken BMP header: fix it and try again
                    fixBitmapBin(data);
                }
            }
        }

        /// <summary>
        /// https://github.com/HarpyWar/nfkmap-viewer/wiki/BMP-картинка-палитры
        /// Fixes the file size and the pixel data offset in a BMP file header (in place)
        /// </summary>
        /// <param name="data"></param>
        private static void fixBitmapBin(byte[] data)
        {
            const int headerSize = 0x26; // BITMAPFILEHEADER + the part of BITMAPINFOHEADER up to biSizeImage
            if (data.Length < headerSize)
                return;

            int dataSize = data.Length;
            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            using (var bw = new BinaryWriter(ms))
            {
                // read bitmap data length
                ms.Seek(0x22, SeekOrigin.Begin);
                var bitmapDataSize = br.ReadInt32();

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
