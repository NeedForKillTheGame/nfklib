using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace nfklib
{
    public class DemoUnit
    {
        // DDEMO
        public const byte DDEMO_VERSION = 0;     // here is a version of the demo engine... reading from demofile
        public const byte DDEMO_FIREROCKET = 1;
        public const byte DDEMO_PLAYERPOS = 2; // unused (old version)
        public const byte DDEMO_TIMESET = 3;
        public const byte DDEMO_CREATEPLAYER = 4;
        public const byte DDEMO_KILLOBJECT = 5;
        public const byte DDEMO_FIREBFG = 6;
        public const byte DDEMO_FIREPLASMA = 7;
        public const byte DDEMO_FIREGREN = 8;
        public const byte DDEMO_FIRERAIL = 9;
        public const byte DDEMO_FIRESHAFT = 10;
        public const byte DDEMO_FIRESHOTGUN = 11;
        public const byte DDEMO_FIREMACH = 12;
        public const byte DDEMO_ITEMDISSAPEAR = 13;
        public const byte DDEMO_ITEMAPEAR = 14;
        public const byte DDEMO_DAMAGEPLAYER = 15;
        public const byte DDEMO_HAUPDATE = 16;
        public const byte DDEMO_FLASH = 17;
        public const byte DDEMO_JUMPSOUND = 18;
        public const byte DDEMO_GAMEEND = 19;
        public const byte DDEMO_RESPAWNSOUND = 20;
        public const byte DDEMO_JUMPPADSOUND = 21;
        public const byte DDEMO_LAVASOUND = 22;
        public const byte DDEMO_POWERUPSOUND = 23;
        public const byte DDEMO_EARNPOWERUP = 24;
        public const byte DDEMO_READYPRESS = 25;
        public const byte DDEMO_FLIGHTSOUND = 26;
        public const byte DDEMO_EARNREWARD = 27;
        public const byte DDEMO_STATS = 28; // unused (old version)
        public const byte DDEMO_GAMESTATE = 29;
        public const byte DDEMO_TRIXARENAEND = 30;
        public const byte DDEMO_OBJCHANGESTATE = 31;
        public const byte DDEMO_CORPSESPAWN = 32;
        public const byte DDEMO_GRENADESYNC = 33;
        public const byte DDEMO_STATS2 = 34; // unused (old version)
        public const byte DDEMO_PLAYERPOSV2 = 35; // unused (old version)
        public const byte DDEMO_FIREGRENV2 = 36;
        public const byte DDEMO_NOAMMOSOUND = 37;
        public const byte DDEMO_GAUNTLETSTATE = 38;
        public const byte DDEMO_STATS3 = 39;
        public const byte DDEMO_FIREPLASMAV2 = 40;
        public const byte DDEMO_PLAYERPOSV3 = 41;
        public const byte DDEMO_BUBBLE = 42;
        //multiplayer
        public const byte DDEMO_MPSTATE = 43;
        public const byte DDEMO_NETRAIL = 44; //clients.
        public const byte DDEMO_NETPARTICLE = 45; //clients
        public const byte DDEMO_NETTIMEUPDATE = 46; //only clients.
        public const byte DDEMO_NETSVMATCHSTART = 47; //only clients.
        public const byte DDEMO_DROPPLAYER = 48;
        public const byte DDEMO_CREATEPLAYERV2 = 49;
        public const byte DDEMO_SPECTATORCONNECT = 50;
        public const byte DDEMO_SPECTATORDISCONNECT = 51;
        public const byte DDEMO_CHATMESSAGE = 52;
        public const byte DDEMO_PLAYERRENAME = 53;
        public const byte DDEMO_PLAYERMODELCHANGE = 54;
        public const byte DDEMO_GENERICSOUNDDATA = 55;
        public const byte DDEMO_GENERICSOUNDSTATDATA = 56;
        public const byte DDEMO_TEAMSELECT = 57;
        public const byte DDEMO_CTF_EVENT_FLAGTAKEN = 58;
        public const byte DDEMO_CTF_EVENT_FLAGCAPTURE = 59;
        public const byte DDEMO_CTF_EVENT_FLAGDROP = 60;
        public const byte DDEMO_CTF_EVENT_FLAGPICKUP = 61;
        public const byte DDEMO_CTF_EVENT_FLAGDROP_APPLY = 62;
        public const byte DDEMO_CTF_EVENT_FLAGRETURN = 63;
        public const byte DDEMO_CTF_GAMESTATE = 64;
        public const byte DDEMO_CTF_EVENT_FLAGDROPGAMESTATE = 65;
        public const byte DDEMO_CTF_GAMESTATESCORE = 66;
        public const byte DDEMO_CTF_FLAGCARRIER = 67;
        public const byte DDEMO_DOM_CAPTURE = 68;
        public const byte DDEMO_DOM_SCORECHANGED = 69;
        public const byte DDEMO_WPN_EVENT_WEAPONDROP = 70;
        public const byte DDEMO_WPN_EVENT_PICKUP = 71;
        public const byte DDEMO_WPN_EVENT_WEAPONDROP_APPLY = 72;
        public const byte DDEMO_WPN_EVENT_WEAPONDROPGAMESTATE = 73;
        public const byte DDEMO_DOM_CAPTUREGAMESTATE = 74;
        public const byte DDEMO_NEW_SHAFTBEGIN = 75;
        public const byte DDEMO_NEW_SHAFTEND = 76;
        public const byte DDEMO_POWERUP_EVENT_POWERUPDROP = 77;
        public const byte DDEMO_POWERUP_EVENT_PICKUP = 78;
        public const byte DDEMO_POWERUP_EVENT_POWERUPDROPGAMESTATE = 79;

        // conn: additional demo ctf events
        public const byte DDEMO_CTF_EVENT_FLAGTAKEN_RED      = 80;
        public const byte DDEMO_CTF_EVENT_FLAGCAPTURE_RED    = 81;
        public const byte DDEMO_CTF_EVENT_FLAGDROP_RED       = 82;
        // FIXME: (HarpyWar) unused in demo? (I could not find a demo among thousands with these events)
        public const byte DDEMO_CTF_EVENT_FLAGPICKUP_RED     = 83;
        public const byte DDEMO_CTF_EVENT_FLAGDROP_APPLY_RED = 84;
        public const byte DDEMO_CTF_EVENT_FLAGRETURN_RED     = 85;

        /*
        public TDMissile DMissile;
        public TDMissileV2 DMissileV2;
        public TDVectorMissile DVectorMissile;
        public TDGrenadeSync DGrenadeSync;
        public TDGameState DGameState;
        public TDCorpseSpawn DCorpseSpawn;
        public TDReadyPress DReadyPress;
        public TDEarnPowerup DEarnPowerup;
        public TDEarnReward DEarnReward;
        public TDJumppadSound DJumppadSound;
        public TDRespawnSound DRespawnSound;
        public TDLavaSound DLavaSound;
        public TDPowerUpSound DPowerUpSound;
        public TDFlightSound DFlightSound;
        public TDData DData;
        public TDPlayerUpdateV3 DPlayerUpdateV3;

        public TDImmediateTimeSet DImmediateTimeSet;
        public TDGrenadeFireV2 DGrenadeFireV2;
        public TDNoAmmoSound DNoAmmoSound;
        public TDSpawnPlayer DSpawnPlayer;
        public TDSpawnPlayerV2 DSpawnPlayerV2;
        public TDDXIDKill DDXIDKill;
        public TDBubble DBubble;
        public TDGauntletState DGauntletState;
        public TDMultiplayer DMultiplayer;
        public TDItemDissapear DItemDissapear;
        public TDDamagePlayer DDamagePlayer;
        public TDPlayerJump DPlayerJump;
        public TDRespawnFlash DRespawnFlash;
        public TDGameEnd DGameEnd;

        public TDStats3 DStats3;
        public TDTrixArenaEnd DTrixArenaEnd;
        public TDNetShotParticle DNetShotParticle;
        public TDPlayerHAUpdate DPlayerHAUpdate;
        public TDObjChangeState DObjChangeState;

        // MULTIPLAYER
        public TDNetRail DNetRail;
        public TDNETTimeUpdate DNETTimeUpdate;
        public TDNETSV_MatchStart DNETSV_MatchStart;
        public TDNETKickDropPlayer DNETKickDropPlayer;
        public TDNETSpectator DNETSpectator;
        public TDNETCHATMessage DNETCHATMessage;
        public TDNETSoundData DNETSoundData;
        public TDNETSoundStatData DNETSoundStatData;
        public TDNETNameModelChange DNETNameModelChange;
        public TDNETTeamSelect DNETTeamSelect;

        // ctf (demo)
        public TDCTF_DropFlag DCTF_DropFlag;
        public TDCTF_FlagTaken DCTF_FlagTaken;
        public TDCTF_FlagCapture DCTF_FlagCapture;
        public TDCTF_DropFlagApply DCTF_DropFlagApply;
        public TDCTF_FlagPickUp DCTF_FlagPickUp;
        public TDCTF_FlagReturnFlag DCTF_FlagReturnFlag;
        public TDCTF_GameState DCTF_GameState;
        public TDCTF_GameStateScore DCTF_GameStateScore;
        public TDCTF_FlagCarrier DCTF_FlagCarrier;

        // dom
        public TDDOM_Capture DDOM_Capture;
        public TDDOM_ScoreChanges DDOM_ScoreChanges;

        public TDWPN_DropWeapon DWPN_DropWeapon;
        public TDPOWERUP_DropPowerup DPOWERUP_DropPowerup;

        public TD_049t4_ShaftBegin D_049t4_ShaftBegin;
        public TD_049t4_ShaftEnd D_049t4_ShaftEnd;
        */
    }

    // [Delphi Types]
    // Single = 4 bytes (floating)
    // Word = 2 bytes
    // Smallint = 2 bytes

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDData
    {
        public byte gametic;
        public ushort gametime; // word
        public byte type0;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDDamagePlayer
    {
        public ushort DXID, ATTDXID; // word {x,y}
        public byte attwpn, armor;
        public short health; // smallint
        public byte ext;
        public ushort stat_dmggiven, stat_dmgrecvd;  // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDMissile
    {
        public ushort DXID, x, y, spawnerDxid; // word
        public float inertiax, inertiay; // single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDMissileV2
    {
        public ushort DXID, spawnerDxid; // word
        public float inertiax, x, y, inertiay; // single
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDPlayerJump
    {
        public ushort dxid;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDVectorMissile
    {
        public ushort DXID, x, y, cx, cy, spawnerDxid; // word
        public float inertiax, inertiay, angle; // single
        public byte dir;
        //public byte unknown0; // FIXME: new version addition?
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDPlayerRename
    {
        public ushort DXID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string NewName; // string[30]
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDGrenadeFireV2
    {
        public ushort DXID, spawnerDxid; // word
        public float x, y, cx, cy, inertiax, inertiay, angle; // single
        public byte dir;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDGrenadeSync
    {
        public ushort DXID, x, y; // word
        public float inertiax, inertiay; // single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDBubble
    {
        public ushort DXID;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDPlayerUpdateV3
    {
        public ushort DXID; // word
        public float x, y, inertiax, inertiay; // single
        public ushort PUV3; // word
        public byte PUV3B;
        public byte wpnang, currammo;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDPlayerHAUpdate
    {
        public ushort DXID; // word
        public short health; //smallint
        public byte armor;
        public short frags; // smallint
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDItemDissapear
    {
        public byte x, y, i;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDDXIDKill
    {
        public ushort DXID, x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDImmediateTimeSet
    {
        public byte newgametic;
        public ushort newgametime; // word
        public ushort warmup; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDSpawnPlayer
    {
        public ushort DXID, x, y; // word
        public byte dir, frame, dead;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string modelname, netname; // string[30]
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDSpawnPlayerV2
    {
        public ushort DXID, x, y; // word
        public byte dir, dead;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string modelname, netname; // string[30]
        public byte team;
        public byte reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDGauntletState
    {
        public ushort DXID; // word
        public byte State;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDRespawnFlash
    {
        public ushort x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDJumppadSound
    {
        public ushort x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDRespawnSound
    {
        public ushort x, y; // word
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDFlightSound
    {
        public ushort x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDLavaSound
    {
        public ushort x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDPowerUpSound
    {
        public ushort x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDGameEnd
    {          //DDEMO_GAMEEND
        public byte EndType;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDRegenWork
    {
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDFlightWork
    {
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDEarnPowerup
    {
        public ushort DXID; // word
        public byte type1;
        public byte time;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDEarnReward
    {
        public ushort DXID; // word
        public byte type1;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNoAmmoSound
    {
        public ushort x, y; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDStats
    {
        public ushort DXID, stat_kills; // word
        public int stat_dmggiven;
        public int stat_dmgrecvd;
        public ushort mach_hits; // word
        public ushort shot_hits; // word
        public ushort gren_hits; // word
        public ushort rocket_hits; // word
        public ushort shaft_hits; // word
        public ushort plasma_hits; // word
        public ushort rail_hits; // word
        public ushort bfg_hits; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDStats2
    {
        public ushort DXID, stat_kills; // word
        public ushort stat_suicide, stat_deaths; // word
        public int stat_dmggiven, frags;
        public int stat_dmgrecvd;
        public ushort mach_hits; // word
        public ushort shot_hits; // word
        public ushort gren_hits; // word
        public ushort rocket_hits; // word
        public ushort shaft_hits; // word
        public ushort plasma_hits; // word
        public ushort rail_hits; // word
        public ushort bfg_hits; // word
        public ushort mach_fire, shot_fire, gren_fire, rocket_fire, shaft_fire, plasma_fire, rail_fire, bfg_fire; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDStats3
    {
        public ushort DXID, stat_kills; // word
        public ushort stat_suicide, stat_deaths; // word
        public int stat_dmggiven, frags;
        public int stat_dmgrecvd;
        public ushort bonus_impressive, bonus_excellent, bonus_humiliation; // word
        public ushort gaun_hits; // word
        public ushort mach_hits; // word
        public ushort shot_hits; // word
        public ushort gren_hits; // word
        public ushort rocket_hits; // word
        public ushort shaft_hits; // word
        public ushort plasma_hits; // word
        public ushort rail_hits; // word
        public ushort bfg_hits; // word
        public ushort mach_fire, shot_fire, gren_fire, rocket_fire, shaft_fire, plasma_fire, rail_fire, bfg_fire; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDTrixArenaEnd
    {
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDGameState
    {
        public byte type1;   //1=5min,2=1min,3=sudden
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDReadyPress
    {
        public ushort newmatch_statsin; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDObjChangeState
    {
        public byte objindex;
        public byte state;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCorpseSpawn
    {
        public ushort DXID;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDMultiplayer
    {
        public byte y;
        public ushort pov; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNetRail
    {
        public ushort x, y, x1, y1, endx, endy; // word
        public byte color;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNetShotParticle
    {
        public ushort x, y, x1, y1; // word
        public byte index;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETTimeUpdate
    {
        public ushort Min; // word
        public byte WARMUP; // bool
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETSV_MatchStart
    {
        public byte spacer;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETKickDropPlayer
    {
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETSpectator
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string netname; // string[30]
        public byte action; //bool
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETCHATMessage
    {
        public ushort DXID; // word
        public byte messagelenght;
    }
    /// <summary>
    /// Not used in demo, it used to store a message text
    /// </summary>
    public struct TDNETCHATMessageText
    {
        public TDNETCHATMessage TDNETCHATMessage;
        public string MessageText;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETSoundData
    {
        public ushort DXID; // word
        public byte SoundType;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETSoundStatData
    {
        public byte SoundType;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETNameModelChange
    {
        public ushort DXID; // word
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        public string newstr; //string[30]
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDNETTeamSelect
    {
        public ushort DXID; // word
        public byte team;
    }

    // ctf (demo).
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagTaken
    {
        public ushort DXID; // word
        public byte x, y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagDrop
    { //drop from player..
        public ushort DXID, DropDXID; // word
        public float inertiax, inertiay; // single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagCapture
    { //Capture....
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagDroped
    { // dropped to ground, reupdate coordz.
        public ushort DXID; // word
        public float x, y; // single (4 bytes)
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_DropFlag
    { // drop from player.
        public ushort DXID; // word
        public ushort DropperDXID; // word
        public float X, Y; // single
        public float Inertiax, Inertiay; // single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDWPN_DropWeapon
    { // drop from player.
        public ushort DXID; // word
        public ushort DropperDXID; // word
        public byte WeaponID;
        public float X, Y; // single
        public float Inertiax, Inertiay; // single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDPOWERUP_DropPowerup
    { // drop from player.
        public ushort DXID; // word
        public ushort DropperDXID; // word
        public byte dir, imageindex;
        public float X, Y; // single
        public float Inertiax, Inertiay; // single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_DropFlagApply
    { // drop from player. coorrect flag poz
        public ushort DXID; // word
        public float X, Y;// single
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagPickUp
    { // pickup flag.
        public ushort FlagDXID, PlayerDXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagReturnFlag
    { // return flag.
        public ushort FlagDXID; // word
        public byte team;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_GameState
    {
        public byte RedFlagAtBase, BlueFlagAtBase; // bool
        public ushort RedScore, BlueScore; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_GameStateScore
    {
        public ushort RedScore, BlueScore; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDCTF_FlagCarrier
    {
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDDOM_ScoreChanges
    {
        public ushort RedScore, BlueScore; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TDDOM_Capture
    {
        public byte x, y, team;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TD_049t4_ShaftBegin
    {
        public byte AMMO;
        public ushort DXID; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TD_049t4_ShaftEnd
    {
        public ushort DXID; // word
    }

    /* Example: c006aa00165a01 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TD_UNKNOWN1
    {
        public int Unkn1; // dword
        public ushort Unkn2; // word
        public byte Unkn3;
    }

    /* Example: 02 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TD_UNKNOWN2
    {
        public byte Unkn;
    }

    /* Example: e32a0000 */
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TD_UNKNOWN3
    {
        public ushort Unkn1; // word
        public ushort Unkn2; // word
    }
}