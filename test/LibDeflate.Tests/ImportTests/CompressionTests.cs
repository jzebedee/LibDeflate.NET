using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace LibDeflate.Tests.ImportTests;

using static BclCompressionHelper;

public class CompressionTests
{
    public static IEnumerable<object[]> CompressionLevels => Enumerable.Range(0, 13).Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(CompressionLevels))]
    public void AllocAndFreeCompressorTest(int compressionLevel)
    {
        var compressor = Imports.Compression.libdeflate_alloc_compressor(compressionLevel);
        try
        {
            Assert.NotEqual(compressor, IntPtr.Zero);
        }
        finally
        {
            Imports.Compression.libdeflate_free_compressor(compressor);
        }
    }

    [Theory]
    [MemberData(nameof(CompressionLevels))]
    public void DeflateCompressTest(int compressionLevel)
    {
        var compressor = Imports.Compression.libdeflate_alloc_compressor(compressionLevel);
        try
        {
            const string expected = "Hello world!";
            ReadOnlySpan<byte> testBytes = Encoding.UTF8.GetBytes(expected);
            Span<byte> outputBuffer = stackalloc byte[512];
            var numBytesCompressed = Imports.Compression.libdeflate_deflate_compress(compressor, MemoryMarshal.GetReference(testBytes), (UIntPtr)testBytes.Length, ref MemoryMarshal.GetReference(outputBuffer), (UIntPtr)outputBuffer.Length);

            var compressedBuffer = outputBuffer.Slice(0, (int)numBytesCompressed);
#if NET6_0_OR_GREATER
            var buf = FlateToBuffer(compressedBuffer, CompressionMode.Decompress).Span;
#else
            var buf = FlateToBuffer(compressedBuffer, CompressionMode.Decompress).ToArray();
#endif
            var actual = Encoding.UTF8.GetString(buf);
            Assert.Equal(expected, actual);
        }
        finally
        {
            Imports.Compression.libdeflate_free_compressor(compressor);
        }
    }

    [Fact]
    public void DeflateCompressBoundTest()
    {
        //this should deflate to a larger size than the input
        const int inBytes = 10;
        var bound = Imports.Compression.libdeflate_deflate_compress_bound(IntPtr.Zero, (UIntPtr)inBytes);
        Assert.True((int)bound > inBytes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(9)]
    public void ZlibCompressTest(int compressionLevel)
    {
        var compressor = Imports.Compression.libdeflate_alloc_compressor(compressionLevel);
        try
        {
            const string expected = "Hello world!";
            ReadOnlySpan<byte> testBytes = Encoding.UTF8.GetBytes(expected);
            Span<byte> outputBuffer = stackalloc byte[512];
            var numBytesCompressed = Imports.Compression.libdeflate_zlib_compress(compressor, MemoryMarshal.GetReference(testBytes), (UIntPtr)testBytes.Length, ref MemoryMarshal.GetReference(outputBuffer), (UIntPtr)outputBuffer.Length);

            var compressedBuffer = outputBuffer.Slice(0, (int)numBytesCompressed);
#if NET6_0_OR_GREATER
            var buf = ZlibToBuffer(compressedBuffer, CompressionMode.Decompress).Span;
#else
            var buf = ZlibToBuffer(compressedBuffer, CompressionMode.Decompress).ToArray();
#endif
            var actual = Encoding.UTF8.GetString(buf);
            Assert.Equal(expected, actual);
        }
        finally
        {
            Imports.Compression.libdeflate_free_compressor(compressor);
        }
    }

    [Fact]
    public void ZlibCompressBoundTest()
    {
        //this should deflate to a larger size than the input
        const int inBytes = 10;
        var bound = Imports.Compression.libdeflate_zlib_compress_bound(IntPtr.Zero, (UIntPtr)inBytes);
        Assert.True((int)bound > inBytes);
    }

    [Theory]
    [MemberData(nameof(CompressionLevels))]
    public void GzipCompressTest(int compressionLevel)
    {
        var compressor = Imports.Compression.libdeflate_alloc_compressor(compressionLevel);
        try
        {
            const string expected = "Hello world!";
            ReadOnlySpan<byte> testBytes = Encoding.UTF8.GetBytes(expected);
            Span<byte> outputBuffer = stackalloc byte[512];
            var numBytesCompressed = Imports.Compression.libdeflate_gzip_compress(compressor, MemoryMarshal.GetReference(testBytes), (UIntPtr)testBytes.Length, ref MemoryMarshal.GetReference(outputBuffer), (UIntPtr)outputBuffer.Length);

            var compressedBuffer = outputBuffer.Slice(0, (int)numBytesCompressed);
#if NET6_0_OR_GREATER
            var buf = GzipToBuffer(compressedBuffer, CompressionMode.Decompress).Span;
#else
            var buf = GzipToBuffer(compressedBuffer, CompressionMode.Decompress).ToArray();
#endif
            var actual = Encoding.UTF8.GetString(buf);
            Assert.Equal(expected, actual);
        }
        finally
        {
            Imports.Compression.libdeflate_free_compressor(compressor);
        }
    }

    [Fact]
    public void GzipCompressBoundTest()
    {
        //this should deflate to a larger size than the input
        const int inBytes = 10;
        var bound = Imports.Compression.libdeflate_gzip_compress_bound(IntPtr.Zero, (UIntPtr)inBytes);
        Assert.True((int)bound > inBytes);
    }
}
