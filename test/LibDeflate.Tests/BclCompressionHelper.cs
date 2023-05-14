using System;
using System.IO;
using System.IO.Compression;

namespace LibDeflate.Tests;

internal static class BclCompressionHelper
{
    internal static MemoryStream CopySpanToMemoryStream(ReadOnlySpan<byte> input)
    {
        var ms = new MemoryStream();
#if NET6_0_OR_GREATER
        ms.Write(input);
#else
        ms.Write(input.ToArray(), 0, input.Length);
#endif
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    internal static ReadOnlyMemory<byte> FlateToBuffer(ReadOnlySpan<byte> input, CompressionMode mode)
    {
        var outputMs = new MemoryStream();

        switch (mode)
        {
            case CompressionMode.Compress:
                using (var flateStream = new DeflateStream(outputMs, mode, true))
                {
#if NET6_0_OR_GREATER
                    flateStream.Write(input);
#else
                    flateStream.Write(input.ToArray(), 0, input.Length);
#endif
                    flateStream.Flush();
                }
                break;
            case CompressionMode.Decompress:
                using (var inputMs = CopySpanToMemoryStream(input))
                using (var flateStream = new DeflateStream(inputMs, mode))
                {
                    flateStream.CopyTo(outputMs);
                }
                break;
        }

        return new ReadOnlyMemory<byte>(outputMs.GetBuffer(), 0, (int)outputMs.Length);
    }

    internal static ReadOnlyMemory<byte> ZlibToBuffer(ReadOnlySpan<byte> input, CompressionMode mode)
    {
        var outputMs = new MemoryStream();

        switch (mode)
        {
            case CompressionMode.Compress:
                using (var zlibStream = new Ionic.Zlib.ZlibStream(outputMs, Ionic.Zlib.CompressionMode.Compress))
                {
#if NET6_0_OR_GREATER
                    zlibStream.Write(input);
#else
                    zlibStream.Write(input.ToArray(), 0, input.Length);
#endif
                    zlibStream.Flush();
                }
                break;
            case CompressionMode.Decompress:
                using (var inputMs = CopySpanToMemoryStream(input))
                using (var zlibStream = new Ionic.Zlib.ZlibStream(inputMs, Ionic.Zlib.CompressionMode.Decompress))
                {
                    zlibStream.CopyTo(outputMs);
                }
                break;
        }

        return new ReadOnlyMemory<byte>(outputMs.GetBuffer(), 0, (int)outputMs.Length);
    }


    internal static ReadOnlyMemory<byte> GzipToBuffer(ReadOnlySpan<byte> input, CompressionMode mode)
    {
        var outputMs = new MemoryStream();

        switch (mode)
        {
            case CompressionMode.Compress:
                using (var flateStream = new GZipStream(outputMs, mode, true))
                {
#if NET6_0_OR_GREATER
                    flateStream.Write(input);
#else
                    flateStream.Write(input.ToArray(), 0, input.Length);
#endif
                    flateStream.Flush();
                }
                break;
            case CompressionMode.Decompress:
                using (var inputMs = CopySpanToMemoryStream(input))
                using (var flateStream = new GZipStream(inputMs, mode))
                {
                    flateStream.CopyTo(outputMs);
                }
                break;
        }

        return new ReadOnlyMemory<byte>(outputMs.GetBuffer(), 0, (int)outputMs.Length);
    }
}
