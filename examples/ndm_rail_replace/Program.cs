using nfklib;
using nfklib.NDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ndm_rail_replace
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("The program replaces rail color in NFK demo file.");
            Console.WriteLine("(c) 2020 HarpyWar <harpywar@gmail.com>\n");

            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine("Usage: ndm_rail_replace.exe [olddemo] [newdemo] [newrailcolor(1-8)] {playerid(1-8)}\nIf no playerid specified then color replaces for all players");
                Console.WriteLine("Examples:");
                Console.WriteLine("\tndm_rail_replace.exe olddemo.ndm newdemo.ndm 5");
                Console.WriteLine("\tndm_rail_replace.exe olddemo.ndm newdemo.ndm 5 1");
                Console.WriteLine("\nRail color list: {0}", getColorList());
                Console.WriteLine("\nPress any key to exit...");
                Console.Read();
                Environment.Exit(0);
            }
            var oldDemoFile = args[0];
            var newDemoFile = args[1];
            var palOrMapFile = args[2];
            byte railColor = 1;
            ushort? playerDXID = null;


            if (!File.Exists(oldDemoFile))
            {
                Console.WriteLine("Demo file not exists");
                Environment.Exit(2);
            }

            if (args.Length >= 3)
            {
                try
                {
                    railColor = Convert.ToByte(args[2], 10);
                    if (railColor < 1 || railColor > 8)
                    {
                        
                        throw new Exception("[ERROR] rail color must be between 1 and 8" + getColorList()); // \n1 = red\n2 = green\n3 = yellow\n4 = blue\n5 = teal\n6 = pink\n7 = white\n8 = black\n
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }
            }

            var ndm = new NFKDemo();
            try
            {
                Console.WriteLine("Reading demo " + oldDemoFile + "...");
                ndm.Read(oldDemoFile);

                if (args.Length >= 4)
                {
                    try
                    {
                        var playerId = Convert.ToByte(args[3], 10);
                        var playerCount = ndm.demo.Players.Count;
                        if (playerId < 1 || playerId > playerCount)
                            throw new Exception(string.Format("[ERROR] player id must be between 1 and {0} (max players), or empty - to change rail color for all players", playerCount));
                        playerDXID = ndm.demo.Players[playerId - 1].DXID;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Environment.Exit(1);
                    }
                }

                Console.WriteLine("Players");
                // print players
                for (var i = 0; i < ndm.demo.Players.Count; i++)
                    Console.WriteLine("\t[{0}] {1}", i + 1, ndm.demo.Players[i].netname);
                Console.WriteLine();

                bool foundRail = false;
                for (var i = 0; i < ndm.demo.DemoUnits.Count; i++)
                {
                    var type = ndm.demo.DemoUnits[i].DData.type0;

                    if (type == DemoUnit.DDEMO_FIRERAIL)
                    {
                        var unit = (TDVectorMissile)ndm.demo.DemoUnits[i].DemoUnit;
                        if (unit.dir == railColor)
                            continue;

                        // filter by player if given
                        if (playerDXID != null)
                            if (unit.spawnerDxid != playerDXID)
                                continue;

                        var pname = ndm.demo.Players.Where(t => t.DXID == unit.spawnerDxid).FirstOrDefault().netname;
                        var min = ndm.demo.DemoUnits[i].DData.gametime / 60;
                        var sec = ndm.demo.DemoUnits[i].DData.gametime - (ndm.demo.DemoUnits[i].DData.gametime / 60) * 60;
                        Console.WriteLine("[({3}:{4}] Replace rail from '{0}' to '{1}' ({2})", 
                            getRailColorString(unit.dir), 
                            getRailColorString(railColor), 
                            pname,
                            min.ToString().PadLeft(2, '0'),
                            sec.ToString().PadLeft(2, '0'));

                        unit.dir = railColor;
                        ndm.demo.DemoUnits[i].DemoUnit = unit;
                        foundRail = true;
                    }
                }
                if (!foundRail)
                {
                    Console.WriteLine("Can not find replaceable rail shoots in the demo");
                    Environment.Exit(0);
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

        private static string getColorList()
        {
            var colorList = new StringBuilder();
            foreach (RailColor c in Enum.GetValues(typeof(RailColor)))
                colorList.AppendFormat("\n\t{0} = {1}", (int)c, c.ToString());
            return colorList.ToString();
        }

        static string getRailColorString(byte color)
        {
            return ((RailColor)color).ToString();
        }
        enum RailColor
        {
            red = 1,
            green = 2,
            yellow = 3,
            blue = 4,
            teal = 5,
            pink = 6,
            white = 7,
            black = 8
        }
    }
}
