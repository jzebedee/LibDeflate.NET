using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace LibDeflate.Tests
{
    public class CompressionTests
    {
        public static IEnumerable<object[]> CompressionLevels => Enumerable.Range(0, 13).Select(i => new object[] { i });

        private static MemoryStream CopySpanToMemoryStream(ReadOnlySpan<byte> input)
        {
            var ms = new MemoryStream();
            ms.Write(input);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private static ReadOnlySpan<byte> BclInflate(ReadOnlySpan<byte> input)
        {
            using var ms = CopySpanToMemoryStream(input);
            using (var inflateStream = new DeflateStream(ms, CompressionMode.Decompress, true))
            {
                var buf = new byte[512];
                var bytesRead = inflateStream.Read(buf);

                return new ReadOnlySpan<byte>(buf, 0, bytesRead);
            }
        }

        private static ReadOnlySpan<byte> ZlibInflate(ReadOnlySpan<byte> input)
        {
            const ushort ZlibMagicNoCompression = 0x0178;
            const ushort ZlibMagicDefaultCompression = 0x9C78;
            const ushort ZlibMagicMaximumCompression = 0xDA78;

            using var ms = CopySpanToMemoryStream(input);

            //currently no zlibstream support in the BCL, so hack it
            ushort magic = default;
            Span<byte> magicSpan = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref magic, 1));
            if (ms.Read(magicSpan) == sizeof(ushort) && magic switch
            {
                ZlibMagicNoCompression => true,
                ZlibMagicDefaultCompression => true,
                ZlibMagicMaximumCompression => true,
                _ => false
            })
            {
                using var inflateStream = new DeflateStream(ms, CompressionMode.Decompress, true);
                var buf = new byte[512];
                var bytesRead = inflateStream.Read(buf);

                return new ReadOnlySpan<byte>(buf, 0, bytesRead);
            }

            throw new InvalidOperationException("Could not read zlib header magic");
        }

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
                var actual = Encoding.UTF8.GetString(BclInflate(compressedBuffer));
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
                var actual = Encoding.UTF8.GetString(ZlibInflate(compressedBuffer));
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
    }
}
