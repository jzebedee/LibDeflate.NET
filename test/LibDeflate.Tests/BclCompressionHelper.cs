using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace LibDeflate.Tests
{
    internal static class BclCompressionHelper
    {
        internal static MemoryStream CopySpanToMemoryStream(ReadOnlySpan<byte> input)
        {
            var ms = new MemoryStream();
            ms.Write(input);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        internal static ReadOnlyMemory<byte> FlateToBuffer(ReadOnlySpan<byte> input, CompressionMode mode)
        {
            var outputMs = new MemoryStream();

            using var inputMs = CopySpanToMemoryStream(input);
            using var inflateStream = new DeflateStream(inputMs, mode);
            inflateStream.CopyTo(outputMs);

            return new ReadOnlyMemory<byte>(outputMs.GetBuffer(), 0, (int)outputMs.Length);
        }

        internal static ReadOnlyMemory<byte> ZlibToBuffer(ReadOnlySpan<byte> input, CompressionMode mode)
        {
            const ushort ZlibMagicNoCompression = 0x0178;
            const ushort ZlibMagicDefaultCompression = 0x9C78;
            const ushort ZlibMagicMaximumCompression = 0xDA78;

            using var inputMs = CopySpanToMemoryStream(input);

            //currently no zlibstream support in the BCL, so hack it
            ushort magic = default;
            Span<byte> magicSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref magic, 1));
            if (inputMs.Read(magicSpan) != sizeof(ushort) || !(magic switch
            {
                ZlibMagicNoCompression => true,
                ZlibMagicDefaultCompression => true,
                ZlibMagicMaximumCompression => true,
                _ => false
            }))
            {
                throw new InvalidOperationException("Could not read zlib header magic");
            }

            using var inflateStream = new DeflateStream(inputMs, mode, true);

            var outputMs = new MemoryStream();
            inflateStream.CopyTo(outputMs);

            return new ReadOnlyMemory<byte>(outputMs.GetBuffer(), 0, (int)outputMs.Length);
        }

        internal static ReadOnlyMemory<byte> GzipToBuffer(ReadOnlySpan<byte> input, CompressionMode mode)
        {
            var outputMs = new MemoryStream();

            using var inputMs = CopySpanToMemoryStream(input);
            using var inflateStream = new GZipStream(inputMs, mode);
            inflateStream.CopyTo(outputMs);

            return new ReadOnlyMemory<byte>(outputMs.GetBuffer(), 0, (int)outputMs.Length);
        }
    }
}
