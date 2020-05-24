using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace nfklib.NMap
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct TMapObj
    {
        public byte active; // boolean
        public byte unknown0; // unknown
        public short x, y, length, dir, wait; // word
        public short targetname, target, orient, nowanim, special; // word
        public byte objtype; // byte
        public byte unknown1; // unknown
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct THeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] ID; // char[4]
        public byte Version; // byte
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        public string MapName; // byte header + string[70]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        public string Author; // byte header + string[70]
        public byte MapSizeX, MapSizeY, BG, GAMETYPE, numobj;  // byte
        public short numlights; // word
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct TMapEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] EntryType; // byte header + string[3]
        public int DataSize; // longint
        public byte Reserved1; // byte
        public short Reserved2; // word
        public int Reserved3; // integer
        public int Reserved4; // longint
        public int Reserved5; // cardinal
        public byte Reserved6; // boolean
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    [Serializable]
    public struct TLocationText
    {
        public byte enabled; // boolean
        public byte x; public byte y; // byte
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
        public string text; // string[64]
    }
}
