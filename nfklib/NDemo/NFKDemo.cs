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
using Encoder = System.Drawing.Imaging.Encoder;

namespace nfklib.NDemo
{
    public class NFKDemo
    {
        const string DemoHeader = "NFKDEMO";

        private byte[] demoBytes = new byte[] { };
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
            using (var br = new BinaryReader(stream, Encoding.ASCII))
            {
                // check if header is bad
                var header = br.ReadBytes(DemoHeader.Length);
                if (Encoding.ASCII.GetString(header) != DemoHeader)
                    throw new Exception("Bad demo file");

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
                    var pos = ms.Position; // remember
                    int datasize = (int)(ms.Length - ms.Position) - 1;
                    demoBytes = new byte[datasize];
                    ms.Read(demoBytes, 0, datasize);
#if DEBUG
                    File.WriteAllBytes("rawdemo.ndm", demoBytes);
#endif
                    ms.Seek(pos, SeekOrigin.Begin); // restore
                    demo = ReadDemo(mbr);
                }
            }
            return demo;
        }


        /// <summary>
        /// Read demo info from raw demo chunk (after nmap)
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public DemoItem ReadDemo(BinaryReader br)
        {
            #region research
            //int validCounter = 5;
            //long lastErrorPos = 0;
            #endregion

            var bs = br.BaseStream;

            // 1) GET PLAYERS
            while (br.BaseStream.Length > bs.Position)
            {
                #region research
                //validCounter++;
                //lastErrorPos = bs.Position;
                #endregion

                var d = new DemoUnitItem();
                //read data!
                d.DData = bs.ReadStruct<TDData>();
#if DEBUG
                //Console.WriteLine("{0}: {1} {2} {3}", bs.Position, d.DData.gametic, d.DData.gametime, d.DData.type0);
#endif

                switch (d.DData.type0)
                {
                    case DemoUnit.DDEMO_FIREROCKET:
                        d.DemoUnit = bs.ReadStruct<TDMissileV2>();
                        break;
                    case DemoUnit.DDEMO_PLAYERPOSV3:
                        d.DemoUnit = bs.ReadStruct<TDPlayerUpdateV3>();
                        break;
                    case DemoUnit.DDEMO_FIREGRENV2:
                        d.DemoUnit = bs.ReadStruct<TDGrenadeFireV2>();
                        break;
                    case DemoUnit.DDEMO_TIMESET:
                        d.DemoUnit = bs.ReadStruct<TDImmediateTimeSet>();
                        break;
                    case DemoUnit.DDEMO_CREATEPLAYER: 
                        // not used (old version)
                        d.DemoUnit = bs.ReadStruct<TDSpawnPlayer>();
                        break;
                    case DemoUnit.DDEMO_CREATEPLAYERV2:
                        d.DemoUnit = bs.ReadStruct<TDSpawnPlayerV2>();
                        var du_ = (TDSpawnPlayerV2)d.DemoUnit;
                        du_.netname = Helper.Windows1251ToUtf8(Helper.GetDelphiString(du_.netname));
                        du_.modelname = Helper.GetDelphiString(du_.modelname);
                        // push a player
                        demo.Players.Add(du_);
                        d.DemoUnit = du_;
                        break;
                    case DemoUnit.DDEMO_KILLOBJECT:
                        d.DemoUnit = bs.ReadStruct<TDDXIDKill>();
                        break;
                    case DemoUnit.DDEMO_FIREBFG:
                        d.DemoUnit = bs.ReadStruct<TDMissileV2>();
                        break;
                    case DemoUnit.DDEMO_FIREPLASMA:
                        d.DemoUnit = bs.ReadStruct<TDMissile>();
                        break;
                    case DemoUnit.DDEMO_FIREPLASMAV2:
                        d.DemoUnit = bs.ReadStruct<TDMissileV2>();
                        break;
                    case DemoUnit.DDEMO_FIREGREN:
                    case DemoUnit.DDEMO_FIRERAIL:
                    case DemoUnit.DDEMO_FIRESHAFT:
                    case DemoUnit.DDEMO_FIRESHOTGUN:
                    case DemoUnit.DDEMO_FIREMACH:
                        d.DemoUnit = bs.ReadStruct<TDVectorMissile>();
                        break;
                    case DemoUnit.DDEMO_ITEMDISSAPEAR:
                    case DemoUnit.DDEMO_ITEMAPEAR:
                        d.DemoUnit = bs.ReadStruct<TDItemDissapear>();
                        break;
                    case DemoUnit.DDEMO_DAMAGEPLAYER:
                        d.DemoUnit = bs.ReadStruct<TDDamagePlayer>();
                        break;
                    case DemoUnit.DDEMO_HAUPDATE:
                        d.DemoUnit = bs.ReadStruct<TDPlayerHAUpdate>();
                        break;
                    case DemoUnit.DDEMO_JUMPSOUND:
                        d.DemoUnit = bs.ReadStruct<TDPlayerJump>();
                        break;
                    case DemoUnit.DDEMO_FLASH:
                        d.DemoUnit = bs.ReadStruct<TDRespawnFlash>();
                        break;
                    case DemoUnit.DDEMO_GAMEEND:
                        d.DemoUnit = bs.ReadStruct<TDGameEnd>();
                        demo.Duration = d.DData.gametime;
                        break;
                    case DemoUnit.DDEMO_RESPAWNSOUND:
                        d.DemoUnit = bs.ReadStruct<TDRespawnSound>();
                        break;
                    case DemoUnit.DDEMO_LAVASOUND:
                        d.DemoUnit = bs.ReadStruct<TDLavaSound>();
                        break;
                    case DemoUnit.DDEMO_POWERUPSOUND:
                        d.DemoUnit = bs.ReadStruct<TDPowerUpSound>();
                        break;
                    case DemoUnit.DDEMO_JUMPPADSOUND:
                        d.DemoUnit = bs.ReadStruct<TDJumppadSound>();
                        break;
                    case DemoUnit.DDEMO_EARNPOWERUP:
                        d.DemoUnit = bs.ReadStruct<TDEarnPowerup>();
                        break;
                    case DemoUnit.DDEMO_FLIGHTSOUND:
                        d.DemoUnit = bs.ReadStruct<TDFlightSound>();
                        break;
                    case DemoUnit.DDEMO_NOAMMOSOUND:
                        d.DemoUnit = bs.ReadStruct<TDNoAmmoSound>();
                        break;
                    case DemoUnit.DDEMO_EARNREWARD:
                        d.DemoUnit = bs.ReadStruct<TDEarnReward>();
                        break;
                    case DemoUnit.DDEMO_READYPRESS:
                        d.DemoUnit = bs.ReadStruct<TDReadyPress>();
                        break;
                    case DemoUnit.DDEMO_STATS3:
                        d.DemoUnit = bs.ReadStruct<TDStats3>();
                        // stats of player at the end of the game
                        demo.PlayerStats.Add((TDStats3)d.DemoUnit);
                        break;
                    case DemoUnit.DDEMO_GAMESTATE:
                        d.DemoUnit = bs.ReadStruct<TDGameState>();
                        break;
                    case DemoUnit.DDEMO_TRIXARENAEND:
                        d.DemoUnit = bs.ReadStruct<TDTrixArenaEnd>();
                        break;
                    case DemoUnit.DDEMO_OBJCHANGESTATE:
                        d.DemoUnit = bs.ReadStruct<TDObjChangeState>();
                        break;
                    case DemoUnit.DDEMO_CORPSESPAWN:
                        d.DemoUnit = bs.ReadStruct<TDCorpseSpawn>();
                        break;
                    case DemoUnit.DDEMO_GRENADESYNC:
                        d.DemoUnit = bs.ReadStruct<TDGrenadeSync>();
                        break;
                    case DemoUnit.DDEMO_GAUNTLETSTATE:
                        d.DemoUnit = bs.ReadStruct<TDGauntletState>();
                        break;
                    case DemoUnit.DDEMO_BUBBLE:
                        d.DemoUnit = bs.ReadStruct<TDBubble>();
                        break;

                    // multiplayer addons
                    case DemoUnit.DDEMO_MPSTATE:
                        d.DemoUnit = bs.ReadStruct<TDMultiplayer>();
                        break;
                    case DemoUnit.DDEMO_NETRAIL:
                        d.DemoUnit = bs.ReadStruct<TDNetRail>();
                        break;
                    case DemoUnit.DDEMO_NETPARTICLE:
                        d.DemoUnit = bs.ReadStruct<TDNetShotParticle>();
                        break;
                    case DemoUnit.DDEMO_NETTIMEUPDATE:
                        d.DemoUnit = bs.ReadStruct<TDNETTimeUpdate>();
                        break;
                    case DemoUnit.DDEMO_NETSVMATCHSTART:
                        d.DemoUnit = bs.ReadStruct<TDNETSV_MatchStart>();
                        break;
                    case DemoUnit.DDEMO_DROPPLAYER:
                        d.DemoUnit = bs.ReadStruct<TDNETKickDropPlayer>();
                        break;
                    case DemoUnit.DDEMO_SPECTATORDISCONNECT:
                        d.DemoUnit = bs.ReadStruct<TDNETSpectator>();
                        break;
                    case DemoUnit.DDEMO_SPECTATORCONNECT:
                        d.DemoUnit = bs.ReadStruct<TDNETSpectator>();
                        var du = (TDNETSpectator) d.DemoUnit;
                        du.netname = Helper.Windows1251ToUtf8(Helper.GetDelphiString(du.netname));
                        d.DemoUnit = du;
                        break;
                    case DemoUnit.DDEMO_GENERICSOUNDDATA:
                        d.DemoUnit = bs.ReadStruct<TDNETSoundData>();
                        break;
                    case DemoUnit.DDEMO_GENERICSOUNDSTATDATA:
                        d.DemoUnit = bs.ReadStruct<TDNETSoundStatData>();
                        break;
                    case DemoUnit.DDEMO_CHATMESSAGE:
                        var chatMessage = new TDNETCHATMessageText();
                        chatMessage.TDNETCHATMessage = bs.ReadStruct<TDNETCHATMessage>();
                        // read message text
                        var bytes = br.ReadBytes(chatMessage.TDNETCHATMessage.messagelenght);
                        chatMessage.MessageText = Encoding.GetEncoding(1251).GetString(bytes);
                        d.DemoUnit = chatMessage;
                        break;
                    case DemoUnit.DDEMO_PLAYERRENAME:
                        d.DemoUnit = bs.ReadStruct<TDNETNameModelChange>();
                        var du__ = (TDNETNameModelChange)d.DemoUnit;
                        du__.newstr = Helper.GetDelphiString(du__.newstr);
                        d.DemoUnit = du__;
                        break;
                    case DemoUnit.DDEMO_PLAYERMODELCHANGE:
                        d.DemoUnit = bs.ReadStruct<TDNETNameModelChange>();
                        var du___ = (TDNETNameModelChange)d.DemoUnit;
                        du___.newstr = Helper.GetDelphiString(du___.newstr);
                        d.DemoUnit = du___;
                        break;
                    case DemoUnit.DDEMO_TEAMSELECT:
                        d.DemoUnit = bs.ReadStruct<TDNETTeamSelect>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagTaken>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagCapture>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP:
                        d.DemoUnit = bs.ReadStruct<TDCTF_DropFlag>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROPGAMESTATE:
                        d.DemoUnit = bs.ReadStruct<TDCTF_DropFlag>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_APPLY:
                        d.DemoUnit = bs.ReadStruct<TDCTF_DropFlagApply>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGPICKUP:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagPickUp>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGRETURN:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagReturnFlag>();
                        break;
                    case DemoUnit.DDEMO_CTF_GAMESTATE:
                        d.DemoUnit = bs.ReadStruct<TDCTF_GameState>();
                        break;
                    case DemoUnit.DDEMO_CTF_GAMESTATESCORE:
                        d.DemoUnit = bs.ReadStruct<TDCTF_GameStateScore>();
                        break;
                    case DemoUnit.DDEMO_CTF_FLAGCARRIER:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagCarrier>();
                        break;
                    case DemoUnit.DDEMO_DOM_CAPTURE:
                        d.DemoUnit = bs.ReadStruct<TDDOM_Capture>();
                        break;
                    case DemoUnit.DDEMO_DOM_CAPTUREGAMESTATE:
                        d.DemoUnit = bs.ReadStruct<TDDOM_Capture>();
                        break;
                    case DemoUnit.DDEMO_DOM_SCORECHANGED:
                        d.DemoUnit = bs.ReadStruct<TDDOM_ScoreChanges>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP:
                        d.DemoUnit = bs.ReadStruct<TDWPN_DropWeapon>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROPGAMESTATE:
                        d.DemoUnit = bs.ReadStruct<TDWPN_DropWeapon>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_WEAPONDROP_APPLY:
                        d.DemoUnit = bs.ReadStruct<TDCTF_DropFlagApply>();
                        break;
                    case DemoUnit.DDEMO_WPN_EVENT_PICKUP:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagPickUp>();
                        break;
                    case DemoUnit.DDEMO_NEW_SHAFTBEGIN:
                        d.DemoUnit = bs.ReadStruct<TD_049t4_ShaftBegin>();
                        break;
                    case DemoUnit.DDEMO_NEW_SHAFTEND:
                        d.DemoUnit = bs.ReadStruct<TD_049t4_ShaftEnd>();
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROP:
                        d.DemoUnit = bs.ReadStruct<TDPOWERUP_DropPowerup>();
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_POWERUPDROPGAMESTATE:
                        d.DemoUnit = bs.ReadStruct<TDPOWERUP_DropPowerup>();
                        break;
                    case DemoUnit.DDEMO_POWERUP_EVENT_PICKUP:
                        d.DemoUnit = bs.ReadStruct<TDCTF_FlagPickUp>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGTAKEN_RED:
                        d.DemoUnit = bs.ReadStruct<TD_UNKNOWN1>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGCAPTURE_RED:
                        d.DemoUnit = bs.ReadStruct<TD_UNKNOWN2>();
                        break;
                    case DemoUnit.DDEMO_CTF_EVENT_FLAGDROP_RED:
                        d.DemoUnit = bs.ReadStruct<TD_UNKNOWN3>();
                        break;

                    // FIXME: unused?
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

                demo.DemoUnits.Add(d);

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
                        // convert netname and modename back to delphi string
                        du.netname = Helper.SetDelphiString(Helper.Utf8ToWindows1251(du.netname), 31);
                        du.modelname = Helper.SetDelphiString(du.modelname, 31);
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
                        // convert netname back to delphi string
                        du1.netname = Helper.SetDelphiString(Helper.Utf8ToWindows1251(du1.netname), 31);
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
                        var bytes = Encoding.GetEncoding(1251).GetBytes(text);
                        bw.Write(bytes);
                        break;
                    case DemoUnit.DDEMO_PLAYERRENAME:
                    case DemoUnit.DDEMO_PLAYERMODELCHANGE:
                        var du2 = (TDNETNameModelChange)d.DemoUnit;
                        // convert netname back to delphi string
                        du2.newstr = Helper.SetDelphiString(du2.newstr, 31);
                        bw.Write(StreamExtensions.ToByteArray(du2));
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
                        throw new Exception("Unknown demo event, tell to the developer about it");
                        break;
                }

            }
            // HING: we can just write initial raw bytes instead of serialize all demounits
            //bw.Write(demoBytes, 0, demoBytes.Length);
        }

        public void Write(Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.ASCII))
            {
                var header = Encoding.ASCII.GetBytes(DemoHeader);
                bw.Write(header, 0, header.Length);

                // separator 0x2D
                bw.BaseStream.WriteByte(0x2D);

                // gzipped data
                using (var ms = new MemoryStream())
                {
                    using (var databw = new BinaryWriter(ms))
                    {
#if DEBUG
                        Map.Write("demomap.mapa");
#endif
                        Map.Write(databw, true);
                        WriteDemo(databw);
                    }
                    ms.Flush();
                    var gzbytes = ms.GetBuffer();
                    var gzdata = Helper.BZCompress(gzbytes);
                    bw.Write(gzdata, 0, gzdata.Length);
                }

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
