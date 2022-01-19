using SixLabors.ZlibStream;
using System;
using System.IO;
using System.IO.Compression;

namespace LibDeflate.Tests;

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

        switch (mode)
        {
            case CompressionMode.Compress:
                using (var flateStream = new DeflateStream(outputMs, mode, true))
                {
                    flateStream.Write(input);
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
                using (var zlibStream = new ZlibOutputStream(outputMs, SixLabors.ZlibStream.CompressionLevel.DefaultCompression))
                {
                    zlibStream.Write(input);
                    zlibStream.Flush();
                }
                break;
            case CompressionMode.Decompress:
                using (var inputMs = CopySpanToMemoryStream(input))
                using (var zlibStream = new ZlibInputStream(inputMs))
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
                    flateStream.Write(input);
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
