using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nfklib;
using nfklib.NDemo;
using nfklib.NMap;

namespace ndm_pal_replace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("The program replaces map or palette in NFK demo file.");
            Console.WriteLine("(c) 2020 HarpyWar <harpywar@gmail.com>\n");

            if (args.Length < 3 || args.Length > 5)
            {
                Console.WriteLine("Usage: ndm_pal_replace.exe [olddemo] [newdemo] [map|palette] {transparent_color} {numlights}");
                Console.WriteLine("Examples:");
                Console.WriteLine("\tndm_pal_replace.exe olddemo.ndm newdemo.ndm map.mapa");
                Console.WriteLine("\tndm_pal_replace.exe olddemo.ndm newdemo.ndm palette.bmp");
                Console.WriteLine("\tndm_pal_replace.exe olddemo.ndm newdemo.ndm palette.png 0xffff00 6");
                Console.WriteLine("\nPress any key to exit...");
                Console.Read();
                Environment.Exit(0);
            }
            var oldDemoFile = args[0];
            var newDemoFile = args[1];
            var palOrMapFile = args[2];
            short numlights;
            int transparent;


            if (!File.Exists(oldDemoFile))
            {
                Console.WriteLine("Demo file not exists");
                Environment.Exit(2);
            }
            if (!File.Exists(palOrMapFile))
            {
                Console.WriteLine("Map/Palette file not exists");
                Environment.Exit(2);
            }

            var ndm = new NFKDemo();
            try
            {
                Console.WriteLine("Reading demo " + oldDemoFile + "...");
                ndm.Read(oldDemoFile);

                // replace palette
                var palBytes = File.ReadAllBytes(palOrMapFile);
                
                if (Path.GetExtension(palOrMapFile) == ".mapa")
                {
                    Console.WriteLine("Reading map " + palOrMapFile + "...");
                    ndm.Map = new NFKMap();
                    ndm.Map.Read(palOrMapFile);
                }
                else
                {
                    Console.WriteLine("Reading palette " + palOrMapFile + "...");
                    ndm.Map.map.Palette = new Bitmap(Bitmap.FromFile(palOrMapFile));
                }

                if (args.Length >= 4)
                {
                    try
                    {
                        transparent = Convert.ToInt32(args[3], 16);
                        ndm.Map.map.PaletteEntry.Reserved5 = transparent;
                        ndm.Map.map.PaletteEntry.Reserved6 = 1;
                    }
                    catch { }
                }
                if (args.Length >= 5)
                {
                    if (short.TryParse(args[4], out numlights))
                    {
                        ndm.Map.map.Header.numlights = numlights;
                    }
                }
                ndm.Write(newDemoFile);
                Console.WriteLine(newDemoFile + " successfully saved!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(2);
            }
        }
    }
}
