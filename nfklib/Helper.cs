using SharpCompress.Compressor.BZip2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace nfklib
{
    public static class Helper
    {

        /// <summary>
        /// Clear nickname from special symbols
        /// </summary>
        /// <param name="nickname">^3H^#arpy^5War -> HarpyWar</param>
        /// <returns></returns>
        public static string GetRealNick(string nickname)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nickname.Length; i++)
            {
                if (nickname[i] == '^')
                    i++;
                else
                    sb.Append(nickname[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Cut Delphi string from rubbish
        /// </summary>
        /// <param name="str">
        /// Encoding.Default.GetString(new byte[]{3, (byte)'h', (byte)'e', (byte)'l', (byte)'l', (byte)'o'})
        /// ->
        /// "hel"
        /// </param>
        /// <returns></returns>
        public static string GetDelphiString(string str)
        {
            if (str.Length == 0)
                return str;

            byte len = (byte)str[0];
            if (len <= str.Length - 1)
                str = str.Substring(1, len);
            return str;
        }
        public static string SetDelphiString(string str, int maxSize)
        {
            var original = str;

            byte[] bytes = new byte[maxSize];
            Array.Copy(new byte[] {  (byte)str.Length }, bytes, 1);
            Array.Copy(Encoding.Default.GetBytes(str), 0, bytes, 1, str.Length);

            return Encoding.Default.GetString(bytes);
        }

        /// <summary>
        /// Decompress BZip2 data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] BZDecompress(byte[] data)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (BZip2Stream stream = new BZip2Stream(new MemoryStream(data), SharpCompress.Compressor.CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }

        /// <summary>
        /// Compress data using BZip2
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] BZCompress(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BZip2Stream zip = new BZip2Stream(stream, SharpCompress.Compressor.CompressionMode.Compress))
                {
                    zip.Write(data, 0, data.Length);
                    return stream.ToArray();
                }
            }
        }

        public static string Windows1251ToUtf8(string str)
        {
            // FIXME: on my windows 10 webapi return utf8 nicknames, but windows1251 on dedicated server 2008 r2
            //        anyway we don't need to convert this, just return the original string
            return str; 
            //byte[] bytes = Encoding.UTF8.GetBytes(str);
            //return Encoding.GetEncoding(1251).GetString(bytes);
        }
    }
}
