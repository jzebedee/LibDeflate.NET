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

        private static ReadOnlySpan<byte> BclDeflate(ReadOnlySpan<byte> input)
        {
            var ms = new MemoryStream();
            using (var deflateStream = new DeflateStream(ms, CompressionLevel.NoCompression, true))
            {
                deflateStream.Write(input);
                deflateStream.Flush();

                return new ReadOnlySpan<byte>(ms.GetBuffer(), 0, (int)ms.Length);
            }
        }

        private static ReadOnlySpan<byte> BclInflate(ReadOnlySpan<byte> input)
        {
            using var ms = new MemoryStream();
            ms.Write(input);
            ms.Seek(0, SeekOrigin.Begin);

            using(var inflateStream = new DeflateStream(ms, CompressionMode.Decompress, true))
            {
                var buf = new byte[512];
                var bytesRead = inflateStream.Read(buf);

                return new ReadOnlySpan<byte>(buf, 0, bytesRead);
            }
        }

        [Theory]
        [MemberData(nameof(CompressionLevels))]
        public void AllocCompressorTest(int compressionLevel)
        {
            var compressor = Imports.Compression.libdeflate_alloc_compressor(compressionLevel);
            Assert.NotEqual(compressor, IntPtr.Zero);
        }

        [Fact]
        public void DeflateCompressTest()
        {
            var compressor = Imports.Compression.libdeflate_alloc_compressor(0);

            const string expected = "Hello world!";
            ReadOnlySpan<byte> testBytes = Encoding.UTF8.GetBytes(expected);
            Span<byte> outputBuffer = stackalloc byte[512];
            var numBytesCompressed = Imports.Compression.libdeflate_deflate_compress(compressor, MemoryMarshal.GetReference(testBytes), (UIntPtr)testBytes.Length, ref MemoryMarshal.GetReference(outputBuffer), (UIntPtr)outputBuffer.Length);

            var compressedBuffer = outputBuffer.Slice(0, (int)numBytesCompressed);
            var actual = Encoding.UTF8.GetString(BclInflate(compressedBuffer));
            Assert.Equal(expected, actual);
        }
    }
}
