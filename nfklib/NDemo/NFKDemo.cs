using SharpCompress.Compressor.BZip2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using nfklib;
using nfklib.NMap;

namespace nfklib.NDemo
{
    public class NFKDemo
    {
        const string DemoHeader = "NFKDEMO";

        public bool Error = false;
        private DemoItem demo;

        public NFKMap Map;

        public NFKDemo()
        {
            demo = new DemoItem();
            Map = new NFKMap();
        }

        public DemoItem Read(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Open))
            {
                return Read(fs);
            }
        }

        public DemoItem Read(Stream stream)
        {
            byte[] data;
            using (var br = new BinaryReader(stream, Encoding.ASCII))
            {
                // check if header is bad
                var header = br.ReadBytes(DemoHeader.Length);
                if (Encoding.Default.GetString(header) != DemoHeader)
                    return null;

                // separator 0x2D
                br.ReadByte();
                // gzip compressed data
                var gzdata = br.ReadBytes((int)stream.Length - 8);

                data = Helper.BZDecompress(gzdata);
            }
#if DEBUG
            File.WriteAllBytes("rawdemoandmap.ndm", data);
#endif
            using (var ms = new MemoryStream(data))
            {
                using (var mbr = new BinaryReader(ms))
                {
                    demo.Map = Map.Read(mbr);
#if DEBUG
                    var pos = ms.Position; // remember
                    int datasize = (int)(ms.Length - ms.Position) - 1;
                    byte[] data2 = new byte[datasize];
                    ms.Read(data2, 0, datasize);
                    File.WriteAllBytes("rawdemo.ndm", data2);
                    ms.Seek(pos, SeekOrigin.Begin); // restore
#endif
                    demo = Read(mbr);
                }
            }
            return demo;
        }




        /// <summary>
        /// Read demo info from a raw demo chunk (after nmap)
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public DemoItem Read(BinaryReader br)
        {
            #region research
            //int validCounter = 5;
            //long lastErrorPos = 0;
            #endregion

            var bs = br.BaseStream;
            var d = new DemoUnit();

            // 1) GET PLAYERS
            while (br.BaseStream.Length > bs.Position)
            {
                #region research
                //validCounter++;
                //lastErrorPos = bs.Position;
                #endregion

                //read data!
                d.DData = bs.ReadStruct<TDData>();
#if DEBUG
                Console.WriteLine("{0}: {1} {2} {3}", bs.Position, d.DData.gametic, d.DData.gametime, d.DData.type0);
#endif
                switch (d.DData.type0)
                {
                    case DemoUnit.DDEMO_FIREROCKET:
                        d.DMissileV2 = bs.ReadStruct<TDMissileV2>();
                        break;
                    case DemoUnit.DDEMO_PLAYERPOSV3:
                        d.DPlayerUpdateV3 = bs.ReadStruct<TDPlayerUpdateV3>();
                        break;
                    case DemoUnit.DDEMO_FIREGRENV2:
                        d.DGrenadeFireV2 = bs.ReadStruct<TDGrenadeFireV2>();
                        break;
                    case DemoUnit.DDEMO_TIMESET:
                        d.DImmediateTimeSet = bs.ReadStruct<TDImmediateTimeSet>();
                        break;
                    case DemoUnit.DDEMO_CREATEPLAYER:
                        d.DSpawnPlayer = bs.ReadStruct<TDSpawnPlayer>();
                        break;
                    case DemoUnit.DDEMO_CREATEPLAYERV2:
                        d.DSpawnPlayerV2 = bs.ReadStruct<TDSpawnPlayerV2>();
                        // add a player
                        demo.Players.Add(d.DSpawnPlayerV2);
                        break;
                    case DemoUnit.DDEMO_KILLOBJECT:
                        d.DDXIDKill = bs.ReadStruct<TDDXIDKill>();
                        break;
                    case DemoUnit.DDEMO_FIREBFG:
                        d.DMissileV2 = bs.ReadStruct<TDMissileV2>();
                        break;
                    case DemoUnit.DDEMO_FIREPLASMA:
                        d.DMissile = bs.ReadStruct<TDMissile>();
                        break;
                    case DemoUnit.DDEMO_FIREPLASMAV2:
                        d.DMissileV2 = bs.ReadStruct<TDMissileV2>();
                        break;
                    case DemoUnit.DDEMO_FIREGREN:
                    case DemoUnit.DDEMO_FIRERAIL:
                    case DemoUnit.DDEMO_FIRESHAFT:
                    case DemoUnit.DDEMO_FIRESHOTGUN:
                    case DemoUnit.DDEMO_FIREMACH:
                        d.DVectorMissile = bs.ReadStruct<TDVectorMissile>();
                        break;
                    case DemoUnit.DDEMO_ITEMDISSAPEAR:
                    case DemoUnit.DDEMO_ITEMAPEAR:
                        d.DItemDissapear = bs.ReadStruct<TDItemDissapear>();
                        break;
                    case DemoUnit.DDEMO_DAMAGEPLAYER:
                        d.DDamagePlayer = bs.ReadStruct<TDDamagePlayer>();
                        break;
                    case DemoUnit.DDEMO_HAUPDATE:
                        d.DPlayerHAUpdate = bs.ReadStruct<TDPlayerHAUpdate>();
                        break;
                    case DemoUnit.DDEMO_JUMPSOUND:
                        d.DPlayerJump = bs.ReadStruct<TDPlayerJump>();
                        break;
                    case DemoUnit.DDEMO_FLASH:
                        d.DRespawnFlash = bs.ReadStruct<TDRespawnFlash>();
                        break;
                    case DemoUnit.DDEMO_GAMEEND:
                        d.DGameEnd = bs.ReadStruct<TDGameEnd>();
                        demo.Duration = d.DData.gametime;
                        break;
                    case DemoUnit.DDEMO_RESPAWNSOUND:
                        d.DRespawnSound = bs.ReadStruct<TDRespawnSound>();
                        break;
                    case DemoUnit.DDEMO_LAVASOUND:
                        d.DLavaSound = bs.ReadStruct<TDLavaSound>();
                        break;
                    case DemoUnit.DDEMO_POWERUPSOUND:
                        d.DPowerUpSound = bs.ReadStruct<TDPowerUpSound>();
                        break;
                    case DemoUnit.DDEMO_JUMPPADSOUND:
                        d.DJumppadSound = bs.ReadStruct<TDJumppadSound>();
                        break;
                    case DemoUnit.DDEMO_EARNPOWERUP:
                        d.DEarnPowerup = bs.ReadStruct<TDEarnPowerup>();
                        break;
                    case DemoUnit.DDEMO_FLIGHTSOUND:
                        d.DFlightSound = bs.ReadStruct<TDFlightSound>();
                        break;
                    case DemoUnit.DDEMO_NOAMMOSOUND:
                        d.DNoAmmoSound = bs.ReadStruct<TDNoAmmoSound>();
                        break;
                    case DemoUnit.DDEMO_EARNREWARD:
                        d.DEarnReward = bs.ReadStruct<TDEarnReward>();
                        break;
                    case DemoUnit.DDEMO_READYPRESS:
                        d.DReadyPress = bs.ReadStruct<TDReadyPress>();
                        break;
                    case DemoUnit.DDEMO_STATS3:
                        d.DStats3 = bs.ReadStruct<TDStats3>();
                        // stats of player at the end of the game
                        demo.PlayerStats.Add(d.DStats3);
                        break;
                    case DemoUnit.DDEMO_GAMESTATE:
                        d.DGameState = bs.ReadStruct<TDGameState>();
                        break;
                    case DemoUnit.DDEMO_TRIXARENAEND:
                        d.DTrixArenaEnd = bs.ReadStruct<TDTrixArenaEnd>();
                        break;
                    case DemoUnit.DDEMO_OBJCHANGESTATE:
                        d.DObjChangeState = bs.ReadStruct<TDObjChangeState>();
                        break;
                    case DemoUnit.DDEMO_CORPSESPAWN:
                        d.DCorpseSpawn = bs.ReadStruct<TDCorpseSpawn>();
                        break;
                    case DemoUnit.DDEMO_GRENADESYNC:
                        d.DGrenadeSync = bs.ReadStruct<TDGrenadeSync>();
                        break;
                    case DemoUnit.DDEMO_GAUNTLETSTATE:
                        d.DGauntletState = bs.ReadStruct<TDGauntletState>();
                        break;
                    case DemoUnit.DDEMO_BUBBLE:
                        d.DBubble = bs.ReadStruct<TDBubble>();
                        break;

                    // multiplayer addons
                    case DemoUnit.DDEMO_MPSTATE:
                        d.DMultiplayer = bs.ReadStruct<TDMultiplayer>();
                        break;
                    case DemoUnit.DDEMO_NETRAIL:
                        d.DNetRail = bs.ReadStruct<TDNetRail>();
                        break;
                    case DemoUnit.DDEMO_NETPARTICLE:
                        d.DNetShotParticle = bs.ReadStruct<TDNetShotParticle>();
                        break;
                    case DemoUnit.DDEMO_NETTIMEUPDATE:
                        d.DNETTimeUpdate = bs.ReadStruct<TDNETTimeUpdate>();
                        break;
                    case DemoUnit.DDEMO_NETSVMATCHSTART:
                        d.DNETSV_MatchStart = bs.ReadStruct<TDNETSV_MatchStart>();
                        break;
                    case DemoUnit.DDEMO_DROPPLAYER:
                        d.DNETKickDropPlayer = bs.ReadStruct<TDNETKickDropPlayer>();
                        break;
                    case DemoUnit.DDEMO_SPECTATORDISCONNECT:
                        d.DNETSpectator = bs.ReadStruct<TDNETSpectator>();
                        break;
                    case DemoUnit.DDEMO_SPECTATORCONNECT:
                        d.DNETSpectator = bs.ReadStruct<TDNETSpectator>();
                        break;
                    case DemoUnit.DDEMO_GENERICSOUNDDATA:
                        d.DNETSoundData = bs.ReadStruct<TDNETSoundData>();
                        break;
                    case DemoUnit.DDEMO_GENERICSOUNDSTATDATA:
                        d.DNETSoundStatData = bs.ReadStruct<TDNETSoundStatData>();
                        break;
                    case DemoUnit.DDEMO_CHATMESSAGE:
                        d.DNETCHATMessage = bs.ReadStruct<TDNETCHATMessage>();
                        // read message text
                        var msgBytes = br.ReadBytes(d.DNETCHATMessage.messagelenght);
                        break;
                    case DemoUnit.DDEMO_PLAYERRENAME:
                        d.DNETNameModelChange = bs.ReadStruct<TDNETNameModelChange>();
                        break;
                    case DemoUnit.DDEMO_PLAYERMODELCHANGE:
                        d.DNETNameModelChange = bs.ReadStruct<TDNETNameModelChange>();
                        break;
                    case DemoUnit.DDEMO_TEAMSELECT:
                        d.DNETTeamSelect = bs.ReadStruct<TDNETTeamSelect>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN:
                        d.DCTF_FlagTaken = bs.ReadStruct<TDCTF_FlagTaken>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE:
                        d.DCTF_FlagCapture = bs.ReadStruct<TDCTF_FlagCapture>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP:
                        d.DCTF_DropFlag = bs.ReadStruct<TDCTF_DropFlag>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROPGAMESTATE:
                        d.DCTF_DropFlag = bs.ReadStruct<TDCTF_DropFlag>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY:
                        d.DCTF_DropFlagApply = bs.ReadStruct<TDCTF_DropFlagApply>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP:
                        d.DCTF_FlagPickUp = bs.ReadStruct<TDCTF_FlagPickUp>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN:
                        d.DCTF_FlagReturnFlag = bs.ReadStruct<TDCTF_FlagReturnFlag>();
                        break;
                    case DemoUnit.DDEMO_CTF_GAMESTATE:
                        d.DCTF_GameState = bs.ReadStruct<TDCTF_GameState>();
                        break;
                    case DemoUnit.DDEMO_CTF_GAMESTATESCORE:
                        d.DCTF_GameStateScore = bs.ReadStruct<TDCTF_GameStateScore>();
                        break;
                    case DemoUnit.DDEMO_CTF_FLAGCARRIER:
                        d.DCTF_FlagCarrier = bs.ReadStruct<TDCTF_FlagCarrier>();
                        break;
                    case DemoUnit.DDEMO_DOM_CAPTURE:
                        d.DDOM_Capture = bs.ReadStruct<TDDOM_Capture>();
                        break;
                    case DemoUnit.DDEMO_DOM_CAPTUREGAMESTATE:
                        d.DDOM_Capture = bs.ReadStruct<TDDOM_Capture>();
                        break;
                    case DemoUnit.DDEMO_DOM_SCORECHANGED:
                        d.DDOM_ScoreChanges = bs.ReadStruct<TDDOM_ScoreChanges>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP:
                        d.DWPN_DropWeapon = bs.ReadStruct<TDWPN_DropWeapon>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROPGAMESTATE:
                        d.DWPN_DropWeapon = bs.ReadStruct<TDWPN_DropWeapon>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP_APPLY:
                        d.DCTF_DropFlagApply = bs.ReadStruct<TDCTF_DropFlagApply>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_PICKUP:
                        d.DCTF_FlagPickUp = bs.ReadStruct<TDCTF_FlagPickUp>();
                        break;
                    case DemoUnit.DDEMO_NEW_SHAFTBEGIN:
                        d.D_049t4_ShaftBegin = bs.ReadStruct<TD_049t4_ShaftBegin>();
                        break;
                    case DemoUnit.DDEMO_NEW_SHAFTEND:
                        d.D_049t4_ShaftEnd = bs.ReadStruct<TD_049t4_ShaftEnd>();
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROP:
                        d.DPOWERUP_DropPowerup = bs.ReadStruct<TDPOWERUP_DropPowerup>();
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROPGAMESTATE:
                        d.DPOWERUP_DropPowerup = bs.ReadStruct<TDPOWERUP_DropPowerup>();
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_PICKUP:
                        d.DCTF_FlagPickUp = bs.ReadStruct<TDCTF_FlagPickUp>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN_RED:
                        bs.ReadStruct<TD_UNKNOWN1>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE_RED:
                        bs.ReadStruct<TD_UNKNOWN2>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_RED:
                        bs.ReadStruct<TD_UNKNOWN3>();
                        break;

                    // unused?
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP_RED:
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY_RED:
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN_RED:

                    default:
                        throw new Exception("Unknown event. Please, send this demo to https://github.com/NeedForKillTheGame/nfklib/issues");

                        #region research
                        //Console.WriteLine("Pos: " + bs.Position);
                        //validCounter = 0;
                        #endregion
                        break;
                }

                #region research
                //if (error && validCounter > 5)
                //    error = false;
                //else if (error && validCounter > 0)
                //    bs.Seek(lastErrorPos+1, SeekOrigin.Begin);
                //else if (error && validCounter == 0)
                //    bs.Seek(-3, SeekOrigin.Current);
                #endregion
            }

            return demo;
        }





    }




}
