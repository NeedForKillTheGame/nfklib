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
            var fileName = @"D:\Games\NFK\basenfk\demos\54734_HarpyWar_&_S^2ElekTroN_vs_mne_vezet_ya_igrau_&_megarchi_on_ctf1_CTF_20-59-04.ndm";
            //var fileName = "ctf2.mapa";
            var ndm = new nfklib.NDemo.NFKDemo();
            var demo = ndm.Read(fileName);

            if (demo != null)
            {
                Console.WriteLine("Map size: {0}x{1}", demo.Map.Header.MapSizeX, demo.Map.Header.MapSizeY);
                Console.WriteLine("Players: {0}, Stats: {1}", demo.Players.Count, demo.PlayerStats.Count);
            }
        }
    }
}
