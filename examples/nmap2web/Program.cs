using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nfklib.NMap;
using nfklib;
using System.IO;

namespace nmap2web
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: nmap2web.exe filename.mapa");
                Environment.Exit(0);
            }


            var filename = args[0];
            var nmap = new NFKMap();
            var map = nmap.Read(filename);

            // vertical lines
            var bricks = new string[map.Header.MapSizeY];

            foreach (var x in nmap.map.Bricks)
            {
                var count = 0; // column counter
                foreach (var b in x)
                {
                    if (b > 42)
                        bricks[count] += "0";
                    else if(b == SimpleObject.Respawn())
                        bricks[count] += "R";
                    else
                        bricks[count] += "+";

                    count++;
                }
            }
            // implode lines
            var output = string.Join("%0D%0A", bricks);

            File.WriteAllText(filename + ".webmap", output.ToString());
        }
    }
}
