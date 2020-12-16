using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nfklib.NDemo
{
    public class Constants
    {

		// team
		public const int C_TEAMBLUE = 0;
		public const int C_TEAMRED = 1;
		public const int C_TEAMNON = 2;

		 // weapon ID
		public const int C_WPN_GAUNTLET = 0;
		public const int C_WPN_MACHINE = 1;
		public const int C_WPN_SHOTGUN = 2;
		public const int C_WPN_GRENADE = 3;
		public const int C_WPN_ROCKET = 4;
		public const int C_WPN_SHAFT = 5;
		public const int C_WPN_RAIL = 6;
		public const int C_WPN_PLASMA = 7;
		public const int C_WPN_BFG = 8;


		// model direction & current animation status.
		public const int DIR_LW = 0;  // walkin left
		public const int DIR_RW = 1;  // walkin right
		public const int DIR_LS = 2;  // standin left
		public const int DIR_RS = 3;  // standin right

		public const int GAMETYPE_FFA = 0;
		public const int GAMETYPE_1V1 = 1;
		public const int GAMETYPE_TEAM = 2;
		public const int GAMETYPE_CTF = 3;
		public const int GAMETYPE_RAILARENA = 4;
		public const int GAMETYPE_TRIXARENA = 5;
		public const int GAMETYPE_PRACTICE = 6;
		public const int GAMETYPE_DOMINATION = 7;
		public static uint[] COLORARRAY =
		{
			0xFFFFFFF,
			0xFF000080,
			0xFF008000,
			0xFF800000,
			0xFF800080,
			0xFF808000,
			0xFF808080,
			0xFFC0C0C0,
			0xFF0000FF,
			0xFF00FF00,
			0xFF00FFFF,
			0xFFFF0000,
			0xFFFF00FF,
			0xFFFFFF00,
			0xFFC0C0C0,
			0xFF808080,
			0xFF000000
		};

		// // demo player state
		public const int PUV3_DIR0 = 1;
		public const int PUV3_DIR1 = 2;
		public const int PUV3_DIR2 = 8;
		public const int PUV3_DIR3 = 16;
		public const int PUV3_DEAD0 = 32;
		public const int PUV3_DEAD1 = 64;
		public const int PUV3_DEAD2 = 128;

		/// <summary>
		/// C_WPN_GAUNTLET
		/// </summary>
		public const int PUV3_WPN0 = 256;
		/// <summary>
		/// C_WPN_MACHINE
		/// </summary>
		public const int PUV3_WPN1 = 512;
		/// <summary>
		/// C_WPN_SHOTGUN
		/// </summary>
		public const int PUV3_WPN2 = 1024;
		/// <summary>
		/// C_WPN_GRENADE
		/// </summary>
		public const int PUV3_WPN3 = 2048;
		/// <summary>
		/// C_WPN_ROCKET
		/// </summary>
		public const int PUV3_WPN4 = 4096;
		/// <summary>
		/// C_WPN_SHAFT
		/// </summary>
		public const int PUV3_WPN5 = 8192;
		/// <summary>
		/// C_WPN_RAIL
		/// </summary>
		public const int PUV3_WPN6 = 16384;
		/// <summary>
		/// C_WPN_PLASMA
		/// </summary>
		public const int PUV3_WPN7 = 32768;
		/// <summary>
		/// C_WPN_BFG
		/// </summary>
		public const int PUV3B_WPN8 = 1;

		public const int PUV3B_CROUCH = 2;
		public const int PUV3B_BALLOON =  8;


	
    }
}
