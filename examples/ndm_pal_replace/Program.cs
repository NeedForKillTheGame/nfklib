using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nfklib;
using nfklib.NDemo;

namespace ndm_pal_replace
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 5)
            {
                Console.WriteLine("The program replaces palette in NFK demo file.");
                Console.WriteLine("(c) 2020 HarpyWar <harpywar@gmail.com>\n");
                Console.WriteLine("Usage: ndm_pal_replace.exe [olddemo] [newdemo] [palette{bmp|png}] {transparent_color} {numlights}");
                Console.WriteLine("Examples:");
                Console.WriteLine("\tndm_pal_replace.exe olddemo.ndm newdemo.ndm palette.bmp");
                Console.WriteLine("\tndm_pal_replace.exe olddemo.ndm newdemo.ndm palette.png 0xffff00 6");
                Environment.Exit(0);
            }
            var oldDemoFile = args[0];
            var newDemoFile = args[1];
            var palFile = args[2];
            short numlights;
            int transparent;


            if (!File.Exists(oldDemoFile))
            {
                Console.WriteLine("Demo file not exists");
                Environment.Exit(2);
            }
            if (!File.Exists(palFile))
            {
                Console.WriteLine("Palette file not exists");
                Environment.Exit(2);
            }

            var ndm = new NFKDemo();
            try
            {
                ndm.Read(oldDemoFile);

                // replace palette
                var palBytes = File.ReadAllBytes(palFile);
                ndm.Map.map.Palette = new Bitmap(Bitmap.FromFile(palFile));

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
            }
            catch (Exception e)
            {
                Console.WriteLine("Palette file not exists");
                Environment.Exit(2);
            }
        }
    }
}
