using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using nfklib;
using nfklib.NMap;

namespace nfklib.NDemo
{
    public class NFKDemo
    {
        const string DemoHeader = "NFKDEMO";

        public DemoItem demo;
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
            using (var br = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
            {
                // check if header is bad
                var header = br.ReadBytes(DemoHeader.Length);
                if (Encoding.ASCII.GetString(header) != DemoHeader || stream.Length - stream.Position < 1)
                    throw new InvalidDataException("Bad demo file");

                // separator 0x2D
                br.ReadByte();
                // bzip2 compressed map + demo units
                var bzdata = br.ReadBytes((int)(stream.Length - stream.Position));

                data = Helper.BZDecompress(bzdata);
            }
            using (var ms = new MemoryStream(data, writable: false))
            using (var mbr = new BinaryReader(ms))
            {
                demo.Map = Map.Read(mbr);
                ReadDemo(mbr);
            }
            return demo;
        }


        /// <summary>
        /// Read demo units from raw demo chunk (after nmap)
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public DemoItem ReadDemo(BinaryReader br)
        {
            var bs = br.BaseStream;

            while (bs.Length > bs.Position)
            {
                var data = bs.ReadStruct<TDData>();
                var unit = ReadDemoUnit(data.type0, br);

                switch (unit)
                {
                    case TDSpawnPlayerV2 player:
                        // push a player
                        demo.Players.Add(player);
                        break;
                    case TDStats3 stats:
                        // stats of player at the end of the game
                        demo.PlayerStats.Add(stats);
                        break;
                    case TDGameEnd:
                        demo.Duration = data.gametime;
                        break;
                }

                demo.DemoUnits.Add(new DemoUnitItem(data, unit));
            }

            return demo;
        }

        /// <summary>
        /// Read a single demo unit of the given type (the TDData header is already consumed)
        /// </summary>
        private static object ReadDemoUnit(byte type, BinaryReader br)
        {
            var bs = br.BaseStream;
            switch (type)
            {
                case DemoUnit.DDEMO_FIREROCKET:
                    return bs.ReadStruct<TDMissileV2>();
                case DemoUnit.DDEMO_PLAYERPOSV3:
                    return bs.ReadStruct<TDPlayerUpdateV3>();
                case DemoUnit.DDEMO_FIREGRENV2:
                    return bs.ReadStruct<TDGrenadeFireV2>();
                case DemoUnit.DDEMO_TIMESET:
                    return bs.ReadStruct<TDImmediateTimeSet>();
                case DemoUnit.DDEMO_CREATEPLAYER:
                    // not used (old version)
                    return bs.ReadStruct<TDSpawnPlayer>();
                case DemoUnit.DDEMO_CREATEPLAYERV2:
                    return bs.ReadStruct<TDSpawnPlayerV2>();
                case DemoUnit.DDEMO_KILLOBJECT:
                    return bs.ReadStruct<TDDXIDKill>();
                case DemoUnit.DDEMO_FIREBFG:
                    return bs.ReadStruct<TDMissileV2>();
                case DemoUnit.DDEMO_FIREPLASMA:
                    return bs.ReadStruct<TDMissile>();
                case DemoUnit.DDEMO_FIREPLASMAV2:
                    return bs.ReadStruct<TDMissileV2>();
                case DemoUnit.DDEMO_FIREGREN:
                case DemoUnit.DDEMO_FIRERAIL:
                case DemoUnit.DDEMO_FIRESHAFT:
                case DemoUnit.DDEMO_FIRESHOTGUN:
                case DemoUnit.DDEMO_FIREMACH:
                    return bs.ReadStruct<TDVectorMissile>();
                case DemoUnit.DDEMO_ITEMDISSAPEAR:
                case DemoUnit.DDEMO_ITEMAPEAR:
                    return bs.ReadStruct<TDItemDissapear>();
                case DemoUnit.DDEMO_DAMAGEPLAYER:
                    return bs.ReadStruct<TDDamagePlayer>();
                case DemoUnit.DDEMO_HAUPDATE:
                    return bs.ReadStruct<TDPlayerHAUpdate>();
                case DemoUnit.DDEMO_JUMPSOUND:
                    return bs.ReadStruct<TDPlayerJump>();
                case DemoUnit.DDEMO_FLASH:
                    return bs.ReadStruct<TDRespawnFlash>();
                case DemoUnit.DDEMO_GAMEEND:
                    return bs.ReadStruct<TDGameEnd>();
                case DemoUnit.DDEMO_RESPAWNSOUND:
                    return bs.ReadStruct<TDRespawnSound>();
                case DemoUnit.DDEMO_LAVASOUND:
                    return bs.ReadStruct<TDLavaSound>();
                case DemoUnit.DDEMO_POWERUPSOUND:
                    return bs.ReadStruct<TDPowerUpSound>();
                case DemoUnit.DDEMO_JUMPPADSOUND:
                    return bs.ReadStruct<TDJumppadSound>();
                case DemoUnit.DDEMO_EARNPOWERUP:
                    return bs.ReadStruct<TDEarnPowerup>();
                case DemoUnit.DDEMO_FLIGHTSOUND:
                    return bs.ReadStruct<TDFlightSound>();
                case DemoUnit.DDEMO_NOAMMOSOUND:
                    return bs.ReadStruct<TDNoAmmoSound>();
                case DemoUnit.DDEMO_EARNREWARD:
                    return bs.ReadStruct<TDEarnReward>();
                case DemoUnit.DDEMO_READYPRESS:
                    return bs.ReadStruct<TDReadyPress>();
                case DemoUnit.DDEMO_STATS3:
                    return bs.ReadStruct<TDStats3>();
                case DemoUnit.DDEMO_GAMESTATE:
                    return bs.ReadStruct<TDGameState>();
                case DemoUnit.DDEMO_TRIXARENAEND:
                    return bs.ReadStruct<TDTrixArenaEnd>();
                case DemoUnit.DDEMO_OBJCHANGESTATE:
                    return bs.ReadStruct<TDObjChangeState>();
                case DemoUnit.DDEMO_CORPSESPAWN:
                    return bs.ReadStruct<TDCorpseSpawn>();
                case DemoUnit.DDEMO_GRENADESYNC:
                    return bs.ReadStruct<TDGrenadeSync>();
                case DemoUnit.DDEMO_GAUNTLETSTATE:
                    return bs.ReadStruct<TDGauntletState>();
                case DemoUnit.DDEMO_BUBBLE:
                    return bs.ReadStruct<TDBubble>();

                // multiplayer addons
                case DemoUnit.DDEMO_MPSTATE:
                    return bs.ReadStruct<TDMultiplayer>();
                case DemoUnit.DDEMO_NETRAIL:
                    return bs.ReadStruct<TDNetRail>();
                case DemoUnit.DDEMO_NETPARTICLE:
                    return bs.ReadStruct<TDNetShotParticle>();
                case DemoUnit.DDEMO_NETTIMEUPDATE:
                    return bs.ReadStruct<TDNETTimeUpdate>();
                case DemoUnit.DDEMO_NETSVMATCHSTART:
                    return bs.ReadStruct<TDNETSV_MatchStart>();
                case DemoUnit.DDEMO_DROPPLAYER:
                    return bs.ReadStruct<TDNETKickDropPlayer>();
                case DemoUnit.DDEMO_SPECTATORDISCONNECT:
                    return bs.ReadStruct<TDNETSpectator>();
                case DemoUnit.DDEMO_SPECTATORCONNECT:
                    return bs.ReadStruct<TDNETSpectator>();
                case DemoUnit.DDEMO_GENERICSOUNDDATA:
                    return bs.ReadStruct<TDNETSoundData>();
                case DemoUnit.DDEMO_GENERICSOUNDSTATDATA:
                    return bs.ReadStruct<TDNETSoundStatData>();
                case DemoUnit.DDEMO_CHATMESSAGE:
                {
                    var chatMessage = new TDNETCHATMessageText();
                    chatMessage.TDNETCHATMessage = bs.ReadStruct<TDNETCHATMessage>();
                    // read message text
                    var bytes = br.ReadBytes(chatMessage.TDNETCHATMessage.messagelenght);
                    chatMessage.MessageText = Helper.DelphiEncoding.GetString(bytes);
                    return chatMessage;
                }
                case DemoUnit.DDEMO_PLAYERRENAME:
                    return bs.ReadStruct<TDNETNameModelChange>();
                case DemoUnit.DDEMO_PLAYERMODELCHANGE:
                    return bs.ReadStruct<TDNETNameModelChange>();
                case DemoUnit.DDEMO_TEAMSELECT:
                    return bs.ReadStruct<TDNETTeamSelect>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN:
                    return bs.ReadStruct<TDCTF_FlagTaken>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE:
                    return bs.ReadStruct<TDCTF_FlagCapture>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP:
                    return bs.ReadStruct<TDCTF_DropFlag>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGDROPGAMESTATE:
                    return bs.ReadStruct<TDCTF_DropFlag>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY:
                    return bs.ReadStruct<TDCTF_DropFlagApply>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP:
                    return bs.ReadStruct<TDCTF_FlagPickUp>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN:
                    return bs.ReadStruct<TDCTF_FlagReturnFlag>();
                case DemoUnit.DDEMO_CTF_GAMESTATE:
                    return bs.ReadStruct<TDCTF_GameState>();
                case DemoUnit.DDEMO_CTF_GAMESTATESCORE:
                    return bs.ReadStruct<TDCTF_GameStateScore>();
                case DemoUnit.DDEMO_CTF_FLAGCARRIER:
                    return bs.ReadStruct<TDCTF_FlagCarrier>();
                case DemoUnit.DDEMO_DOM_CAPTURE:
                    return bs.ReadStruct<TDDOM_Capture>();
                case DemoUnit.DDEMO_DOM_CAPTUREGAMESTATE:
                    return bs.ReadStruct<TDDOM_Capture>();
                case DemoUnit.DDEMO_DOM_SCORECHANGED:
                    return bs.ReadStruct<TDDOM_ScoreChanges>();
                case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP:
                    return bs.ReadStruct<TDWPN_DropWeapon>();
                case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROPGAMESTATE:
                    return bs.ReadStruct<TDWPN_DropWeapon>();
                case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP_APPLY:
                    return bs.ReadStruct<TDCTF_DropFlagApply>();
                case DemoUnit.DDEMO_WPN_EVENT_PICKUP:
                    return bs.ReadStruct<TDCTF_FlagPickUp>();
                case DemoUnit.DDEMO_NEW_SHAFTBEGIN:
                    return bs.ReadStruct<TD_049t4_ShaftBegin>();
                case DemoUnit.DDEMO_NEW_SHAFTEND:
                    return bs.ReadStruct<TD_049t4_ShaftEnd>();
                case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROP:
                    return bs.ReadStruct<TDPOWERUP_DropPowerup>();
                case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROPGAMESTATE:
                    return bs.ReadStruct<TDPOWERUP_DropPowerup>();
                case DemoUnit.DDEMO_POWERUP_EVENT_PICKUP:
                    return bs.ReadStruct<TDCTF_FlagPickUp>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN_RED:
                    return bs.ReadStruct<TD_UNKNOWN1>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE_RED:
                    return bs.ReadStruct<TD_UNKNOWN2>();
                case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_RED:
                    return bs.ReadStruct<TD_UNKNOWN3>();
                // FIXME: unused?
                case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP_RED:
                case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY_RED:
                case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN_RED:
                default:
                    throw new InvalidDataException("Unknown event. Please, send this demo to https://github.com/NeedForKillTheGame/nfklib/issues");
            }
        }


        public void WriteDemo(BinaryWriter bw)
        {
            var bs = bw.BaseStream;
            foreach (var d in demo.DemoUnits)
            {
                bw.Write(StreamExtensions.ToByteArray<TDData>(d.DData));

                switch (d.DData.type0)
                {
                    case DemoUnit.DDEMO_FIREROCKET:
                        bw.Write(StreamExtensions.ToByteArray((TDMissileV2)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_PLAYERPOSV3:
                        bw.Write(StreamExtensions.ToByteArray((TDPlayerUpdateV3)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FIREGRENV2:
                        bw.Write(StreamExtensions.ToByteArray((TDGrenadeFireV2)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_TIMESET:
                        bw.Write(StreamExtensions.ToByteArray((TDImmediateTimeSet)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CREATEPLAYER:
                        // not used (old version)
                        bw.Write(StreamExtensions.ToByteArray((TDSpawnPlayer)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CREATEPLAYERV2:
                        var du = (TDSpawnPlayerV2)d.DemoUnit;
                        bw.Write(StreamExtensions.ToByteArray(du));
                        break;
                    case DemoUnit.DDEMO_KILLOBJECT:
                        bw.Write(StreamExtensions.ToByteArray((TDDXIDKill)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FIREBFG:
                        bw.Write(StreamExtensions.ToByteArray((TDMissileV2)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FIREPLASMA:
                        bw.Write(StreamExtensions.ToByteArray((TDMissile)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FIREPLASMAV2:
                        bw.Write(StreamExtensions.ToByteArray((TDMissileV2)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FIREGREN:
                    case DemoUnit.DDEMO_FIRERAIL:
                    case DemoUnit.DDEMO_FIRESHAFT:
                    case DemoUnit.DDEMO_FIRESHOTGUN:
                    case DemoUnit.DDEMO_FIREMACH:
                        bw.Write(StreamExtensions.ToByteArray((TDVectorMissile)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_ITEMDISSAPEAR:
                    case DemoUnit.DDEMO_ITEMAPEAR:
                        bw.Write(StreamExtensions.ToByteArray((TDItemDissapear)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_DAMAGEPLAYER:
                        bw.Write(StreamExtensions.ToByteArray((TDDamagePlayer)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_HAUPDATE:
                        bw.Write(StreamExtensions.ToByteArray((TDPlayerHAUpdate)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_JUMPSOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDPlayerJump)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FLASH:
                        bw.Write(StreamExtensions.ToByteArray((TDRespawnFlash)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_GAMEEND:
                        bw.Write(StreamExtensions.ToByteArray((TDGameEnd)d.DemoUnit));
                        demo.Duration = d.DData.gametime;
                        break;
                    case DemoUnit.DDEMO_RESPAWNSOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDRespawnSound)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_LAVASOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDLavaSound)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_POWERUPSOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDPowerUpSound)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_JUMPPADSOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDJumppadSound)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_EARNPOWERUP:
                        bw.Write(StreamExtensions.ToByteArray((TDEarnPowerup)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_FLIGHTSOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDFlightSound)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NOAMMOSOUND:
                        bw.Write(StreamExtensions.ToByteArray((TDNoAmmoSound)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_EARNREWARD:
                        bw.Write(StreamExtensions.ToByteArray((TDEarnReward)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_READYPRESS:
                        bw.Write(StreamExtensions.ToByteArray((TDReadyPress)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_STATS3:
                        bw.Write(StreamExtensions.ToByteArray((TDStats3)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_GAMESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDGameState)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_TRIXARENAEND:
                        bw.Write(StreamExtensions.ToByteArray((TDTrixArenaEnd)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_OBJCHANGESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDObjChangeState)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CORPSESPAWN:
                        bw.Write(StreamExtensions.ToByteArray((TDCorpseSpawn)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_GRENADESYNC:
                        bw.Write(StreamExtensions.ToByteArray((TDGrenadeSync)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_GAUNTLETSTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDGauntletState)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_BUBBLE:
                        bw.Write(StreamExtensions.ToByteArray((TDBubble)d.DemoUnit));
                        break;

                    // multiplayer addons
                    case DemoUnit.DDEMO_MPSTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDMultiplayer)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NETRAIL:
                        bw.Write(StreamExtensions.ToByteArray((TDNetRail)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NETPARTICLE:
                        bw.Write(StreamExtensions.ToByteArray((TDNetShotParticle)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NETTIMEUPDATE:
                        bw.Write(StreamExtensions.ToByteArray((TDNETTimeUpdate)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NETSVMATCHSTART:
                        bw.Write(StreamExtensions.ToByteArray((TDNETSV_MatchStart)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_DROPPLAYER:
                        bw.Write(StreamExtensions.ToByteArray((TDNETKickDropPlayer)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_SPECTATORDISCONNECT:
                        bw.Write(StreamExtensions.ToByteArray((TDNETSpectator)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_SPECTATORCONNECT:
                        var du1 = (TDNETSpectator)d.DemoUnit;
                        bw.Write(StreamExtensions.ToByteArray(du1));
                        break;
                    case DemoUnit.DDEMO_GENERICSOUNDDATA:
                        bw.Write(StreamExtensions.ToByteArray((TDNETSoundData)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_GENERICSOUNDSTATDATA:
                        bw.Write(StreamExtensions.ToByteArray((TDNETSoundStatData)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CHATMESSAGE:
                        // write text header
                        bw.Write(StreamExtensions.ToByteArray(((TDNETCHATMessageText)d.DemoUnit).TDNETCHATMessage));
                        // convert from string to bytes and write
                        var text = ((TDNETCHATMessageText)d.DemoUnit).MessageText;
                        var bytes = Helper.DelphiEncoding.GetBytes(text);
                        bw.Write(bytes);
                        break;
                    case DemoUnit.DDEMO_PLAYERRENAME:
                    case DemoUnit.DDEMO_PLAYERMODELCHANGE:
                        bw.Write(StreamExtensions.ToByteArray((TDNETNameModelChange)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_TEAMSELECT:
                        bw.Write(StreamExtensions.ToByteArray((TDNETTeamSelect)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagTaken)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagCapture)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_DropFlag)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROPGAMESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_DropFlag)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_DropFlagApply)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagPickUp)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagReturnFlag)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_GAMESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_GameState)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_GAMESTATESCORE:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_GameStateScore)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_FLAGCARRIER:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagCarrier)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_DOM_CAPTURE:
                        bw.Write(StreamExtensions.ToByteArray((TDDOM_Capture)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_DOM_CAPTUREGAMESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDDOM_Capture)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_DOM_SCORECHANGED:
                        bw.Write(StreamExtensions.ToByteArray((TDDOM_ScoreChanges)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP:
                        bw.Write(StreamExtensions.ToByteArray((TDWPN_DropWeapon)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROPGAMESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDWPN_DropWeapon)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP_APPLY:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_DropFlagApply)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_PICKUP:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagPickUp)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NEW_SHAFTBEGIN:
                        bw.Write(StreamExtensions.ToByteArray((TD_049t4_ShaftBegin)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_NEW_SHAFTEND:
                        bw.Write(StreamExtensions.ToByteArray((TD_049t4_ShaftEnd)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROP:
                        bw.Write(StreamExtensions.ToByteArray((TDPOWERUP_DropPowerup)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROPGAMESTATE:
                        bw.Write(StreamExtensions.ToByteArray((TDPOWERUP_DropPowerup)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_PICKUP:
                        bw.Write(StreamExtensions.ToByteArray((TDCTF_FlagPickUp)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN_RED:
                        bw.Write(StreamExtensions.ToByteArray((TD_UNKNOWN1)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE_RED:
                        bw.Write(StreamExtensions.ToByteArray((TD_UNKNOWN2)d.DemoUnit));
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_RED:
                        bw.Write(StreamExtensions.ToByteArray((TD_UNKNOWN3)d.DemoUnit));
                        break;

                    // FIXME: unused?
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP_RED:
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY_RED:
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN_RED:

                    default:
                        throw new InvalidDataException("Unknown demo event, tell to the developer about it");
                }

            }
        }

        public void Write(Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true))
            {
                var header = Encoding.ASCII.GetBytes(DemoHeader);
                bw.Write(header, 0, header.Length);

                // separator 0x2D
                bw.BaseStream.WriteByte(0x2D);

                // bzip2 compressed map + demo units
                byte[] raw;
                using (var ms = new MemoryStream())
                {
                    using (var databw = new BinaryWriter(ms))
                    {
                        Map.Write(databw, true);
                        WriteDemo(databw);
                    }
                    raw = ms.ToArray();
                }
                var bzdata = Helper.BZCompress(raw);
                bw.Write(bzdata, 0, bzdata.Length);
            }
        }

        public void Write(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                Write(fs);
            }
        }



    }




}
