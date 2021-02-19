using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace LibDeflate.Tests
{
    public class DecompressorTests
    {
        public static IEnumerable<object[]> Decompressors
        {
            get
            {
                yield return new[] { new DeflateDecompressor() };
                //yield return new[] { new ZlibDecompressor() };
                //yield return new[] { new GzipDecompressor() };
            }
        }

        [Theory]
        [MemberData(nameof(Decompressors))]
        public void DecompressProvidedBufferTest(Decompressor decompressor)
        {
            Span<byte> input = new byte[0x7900];
            var rand = new Random();
            rand.NextBytes(input);

            using var ms = new MemoryStream();
            {
                using var ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true);
                ds.Write(input);
                ds.Flush();
            }

            var deflatedInput = ms.GetBuffer()[..(int)ms.Length];
            var inflatedOutput = new byte[input.Length];

            using (decompressor)
            {
                var status = decompressor.Decompress(deflatedInput, inflatedOutput, out var bytesWritten);

                var outSpan = new ReadOnlySpan<byte>(inflatedOutput, 0, bytesWritten);
                Assert.True(input.SequenceEqual(outSpan));
            }
        }

        [Theory]
        [MemberData(nameof(Decompressors))]
        public void DecompressOversizedInputTest(Decompressor decompressor)
        {
            Span<byte> input = new byte[0x4000];
            var rand = new Random();
            rand.NextBytes(input);

            using var ms = new MemoryStream();
            {
                using var ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true);
                ds.Write(input);
                ds.Flush();
            }

            var expectedReadLength = ms.Length;
            Span<byte> appendedGarbage = new byte[0x40];
            rand.NextBytes(appendedGarbage);
            ms.Write(appendedGarbage);

            var deflatedInput = ms.GetBuffer()[..(int)ms.Length];
            var inflatedOutput = new byte[input.Length];

            using (decompressor)
            {
                var status = decompressor.Decompress(deflatedInput, inflatedOutput, out var bytesWritten, out var bytesRead);

                var outSpan = new ReadOnlySpan<byte>(inflatedOutput, 0, (int)bytesWritten);
                Assert.True(input.SequenceEqual(outSpan));
                Assert.Equal(expectedReadLength, bytesRead);
            }
        }
    }
}
