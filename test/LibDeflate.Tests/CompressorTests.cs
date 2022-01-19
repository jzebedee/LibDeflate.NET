using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace LibDeflate.Tests;
using static Helpers;

public class CompressorTests
{
    public delegate ReadOnlyMemory<byte> BclInflater(ReadOnlyMemory<byte> deflatedInput);

    public static IEnumerable<object[]> Compressors(int compressionLevel)
    {
        var input = GetRandomBuffer(length: 0x7900);
        yield return new object[] { new DeflateCompressor(compressionLevel), input, (BclInflater)Inflate };
        yield return new object[] { new ZlibCompressor(compressionLevel), input, (BclInflater)ZlibInflate };
        yield return new object[] { new GzipCompressor(compressionLevel), input, (BclInflater)GzipInflate };

        static ReadOnlyMemory<byte> Inflate(ReadOnlyMemory<byte> deflatedInput)
            => BclCompressionHelper.FlateToBuffer(deflatedInput.Span, CompressionMode.Decompress);

        static ReadOnlyMemory<byte> ZlibInflate(ReadOnlyMemory<byte> deflatedInput)
            => BclCompressionHelper.ZlibToBuffer(deflatedInput.Span, CompressionMode.Decompress);

        static ReadOnlyMemory<byte> GzipInflate(ReadOnlyMemory<byte> deflatedInput)
            => BclCompressionHelper.GzipToBuffer(deflatedInput.Span, CompressionMode.Decompress);
    }

    public static IEnumerable<object[]> AllCompressors => Levels.SelectMany(Compressors);

    public static IEnumerable<int> Levels => Enumerable.Range(0, 13);

    private static byte[] GetRandomBuffer(int length)
    {
        var rand = GetRepeatableRandom();
        var input = new byte[length];
        rand.NextBytes(input);
        return input;
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void CompressOwnedBufferTest(Compressor compressor, ReadOnlyMemory<byte> input, BclInflater bclInflater)
    {
        using (compressor)
        {
            using var outputOwner = compressor.Compress(input.Span, useUpperBound: true);
            Assert.NotNull(outputOwner);

            var bclInflated = bclInflater(outputOwner.Memory);
            Assert.True(input.Span.SequenceEqual(bclInflated.Span));
        }
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
    public void CompressProvidedBufferTest(Compressor compressor, ReadOnlyMemory<byte> input, BclInflater bclInflater)
    {
        using (compressor)
        {
            var outputSpan = new byte[input.Length + 0x1000];

            int bytesWritten = compressor.Compress(input.Span, outputSpan);
            Assert.True(bytesWritten > 0);

            var bclInflated = bclInflater(outputSpan[..bytesWritten]);
            Assert.True(input.Span.SequenceEqual(bclInflated.Span));
        }
    }

    [Theory]
    [MemberData(nameof(AllCompressors))]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
    public void CompressProvidedShortBufferTest(Compressor compressor, ReadOnlyMemory<byte> input, BclInflater _)
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
    {
        using (compressor)
        {
            Span<byte> outputSpan = stackalloc byte[1];

            int bytesWritten = compressor.Compress(input.Span, outputSpan);
            Assert.True(bytesWritten == 0);
        }
    }
}
