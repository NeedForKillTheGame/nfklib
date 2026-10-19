using SharpCompress.Compressors.BZip2;
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
        /// Code page of all strings stored inside NFK files (Delphi, Windows-1251).
        /// Encoding.Default is UTF-8 on .NET Core, so it must not be used for them.
        /// </summary>
        public static Encoding DelphiEncoding => _delphiEncoding.Value;
        private static readonly Lazy<Encoding> _delphiEncoding = new(() =>
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(1251);
        });


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
            if (str.Length > maxSize - 1)
                str = str.Substring(0, maxSize - 1);

            byte[] bytes = new byte[maxSize];
            Array.Copy(new byte[] {  (byte)str.Length }, bytes, 1);
            Array.Copy(DelphiEncoding.GetBytes(str), 0, bytes, 1, str.Length);

            return DelphiEncoding.GetString(bytes);
        }

        /// <summary>
        /// Default upper bound for <see cref="BZDecompress(byte[], long)"/>: a bzip2 bomb of a few KB can expand
        /// to gigabytes, real demos and map palettes expand to a few MB at most.
        /// </summary>
        public const long DefaultMaxDecompressedSize = 256L * 1024 * 1024;

        /// <summary>
        /// Decompress BZip2 data
        /// </summary>
        /// <param name="data">bzip2 stream</param>
        /// <param name="maxSize">maximum allowed size of the decompressed data</param>
        /// <exception cref="InvalidDataException">the data is not a valid bzip2 stream, or the decompressed data would exceed <paramref name="maxSize"/></exception>
        public static byte[] BZDecompress(byte[] data, long maxSize = DefaultMaxDecompressedSize)
        {
            try
            {
                using (var stream = new BZip2Stream(new MemoryStream(data), SharpCompress.Compressors.CompressionMode.Decompress, false))
                using (var memory = new MemoryStream())
                {
                    var buffer = new byte[64 * 1024];
                    int count;
                    while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        if (memory.Length + count > maxSize)
                            throw new InvalidDataException($"Decompressed data exceeds the limit of {maxSize} bytes");
                        memory.Write(buffer, 0, count);
                    }
                    return memory.ToArray();
                }
            }
            catch (Exception e) when (e is not InvalidDataException)
            {
                throw new InvalidDataException("Bad bzip2 data: " + e.Message, e);
            }
        }

        /// <summary>
        /// Compress data using BZip2
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] BZCompress(byte[] data)
        {
            using (var stream = new MemoryStream())
            {
                using (var zip = new BZip2Stream(stream, SharpCompress.Compressors.CompressionMode.Compress, false))
                {
                    zip.Write(data, 0, data.Length);
                }
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Kept for compatibility: strings are already decoded with <see cref="DelphiEncoding"/>.
        /// </summary>
        public static string Windows1251ToUtf8(string str) => str;

        /// <summary>
        /// Kept for compatibility: strings are encoded with <see cref="DelphiEncoding"/> on write.
        /// </summary>
        public static string Utf8ToWindows1251(string str) => str;
    }
}
