using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace nfklib
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Read a packed (Delphi record) structure from the stream
        /// </summary>
        /// <exception cref="InvalidDataException">the stream ends before the whole structure is read</exception>
        public static T ReadStruct<T>(this Stream stream) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            try
            {
                stream.ReadExactly(buffer, 0, size);
            }
            catch (EndOfStreamException e)
            {
                throw new InvalidDataException($"Unexpected end of data while reading {typeof(T).Name}", e);
            }

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] ToByteArray<T>(T msg) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var result = new byte[size];
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(msg, buffer, false);
                Marshal.Copy(buffer, result, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return result;
        }
    }

    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, IImageEncoder format)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
