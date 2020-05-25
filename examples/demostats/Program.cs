using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using nfklib;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: demostats.exe demo.ndm");
                Environment.Exit(0);
            }
            var fileName = args[0];

            var ndm = new nfklib.NDemo.NFKDemo();
            try
            {
                var demo = ndm.Read(fileName);
                if (demo != null)
                {
                    Console.WriteLine("Map: {0} ({1}x{2})", demo.Map.Header.MapName, demo.Map.Header.MapSizeX, demo.Map.Header.MapSizeY);
                    Console.WriteLine("Demo duration: {0} seconds", demo.Duration);
                    Console.WriteLine("Demo actions: {0}", demo.DemoUnits.Count);
                    Console.WriteLine();
                    foreach (var p in demo.Players)
                    {
                        Console.WriteLine("---------------------------------------------");
                        Console.WriteLine("Stats for {0} ({1})", Helper.GetRealNick(p.netname), p.modelname);
                        Console.WriteLine();
                        foreach (var s in demo.PlayerStats)
                        {
                            if (p.DXID == s.DXID)
                            {
                                Console.WriteLine("kills: {0}, deaths: {1}, suicides: {2}, frags: {3}", s.stat_kills, s.stat_deaths, s.stat_suicide, s.frags);
                                Console.WriteLine();
                                Console.WriteLine("Accuracy info:");
                                Console.WriteLine("  Gaunlet:\t{0}", s.gaun_hits);
                                Console.WriteLine("  Machine:\t{0}/{1}\t{2:0}%", s.mach_hits, s.mach_fire, s.mach_hits == 0 ? 0 : s.mach_hits / (s.mach_fire * 0.01));
                                Console.WriteLine("  Shotgun:\t{0}/{1}\t{2:0}%", s.shot_hits, s.shot_fire, s.shot_hits == 0 ? 0 : s.shot_hits / (s.shot_hits * 0.01));
                                Console.WriteLine("  Grenade:\t{0}/{1}\t{2:0}%", s.gren_hits, s.gren_fire, s.gren_hits == 0 ? 0 : s.gren_hits / (s.gren_hits * 0.01));
                                Console.WriteLine("  Rocket:\t{0}/{1}\t{2:0}%", s.rocket_hits, s.rocket_fire, s.rocket_hits == 0 ? 0 : s.rocket_hits / (s.rocket_hits * 0.01));
                                Console.WriteLine("  Shaft:\t{0}/{1}\t{2:0}%", s.shaft_hits, s.shaft_fire, s.shaft_hits == 0 ? 0 : s.shaft_hits / (s.shaft_hits * 0.01));
                                Console.WriteLine("  Rail:\t\t{0}/{1}\t{2:0}%", s.rail_hits, s.rail_fire, s.rail_hits == 0 ? 0 : s.rail_hits / (s.rail_hits * 0.01));
                                Console.WriteLine("  Plazma:\t{0}/{1}\t{2:0}%", s.plasma_hits, s.plasma_fire, s.plasma_hits == 0 ? 0 : s.plasma_hits / (s.plasma_hits * 0.01));
                                Console.WriteLine();
                                Console.WriteLine("dmggiven\t{0}", s.stat_dmggiven);
                                Console.WriteLine("dmgreceived\t{0}", s.stat_dmgrecvd);
                                Console.WriteLine();
                                Console.WriteLine("Rewards:");
                                Console.WriteLine("  Impressive: {0}", s.bonus_impressive);
                                Console.WriteLine("  Excellent: {0}", s.bonus_excellent);
                                Console.WriteLine("  Humiliation: {0}", s.bonus_humiliation);
                                Console.WriteLine();
                            }
                        }
                    }

                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }

        }
    }
}
