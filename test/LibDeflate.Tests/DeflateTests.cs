using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LibDeflate.Tests
{
    public class DeflateTests
    {
        [Fact]
        public void DeflateDecompressProvidedBufferTest()
        {
            Span<byte> input = new byte[0x79000];
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

            using var decompressor = new DeflateDecompressor();
            var status = decompressor.Decompress(deflatedInput, inflatedOutput, out var bytesWritten);

            var outSpan = new ReadOnlySpan<byte>(inflatedOutput, 0, (int)bytesWritten);
            Assert.True(input.SequenceEqual(outSpan));
        }

        [Fact]
        public void DeflateDecompressOversizedInputTest()
        {
            Span<byte> input = new byte[0x40000];
            var rand = new Random();
            rand.NextBytes(input);

            using var ms = new MemoryStream();
            {
                using var ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionLevel.Optimal, true);
                ds.Write(input);
                ds.Flush();
            }

            var expectedReadLength = ms.Length;
            Span<byte> appendedGarbage = new byte[0x400];
            rand.NextBytes(appendedGarbage);
            ms.Write(appendedGarbage);

            var deflatedInput = ms.GetBuffer()[..(int)ms.Length];
            var inflatedOutput = new byte[input.Length];

            using var decompressor = new DeflateDecompressor();
            var status = decompressor.Decompress(deflatedInput, inflatedOutput, out var bytesWritten, out var bytesRead);

            var outSpan = new ReadOnlySpan<byte>(inflatedOutput, 0, (int)bytesWritten);
            Assert.True(input.SequenceEqual(outSpan));
            Assert.Equal(expectedReadLength, bytesRead);
        }
    }
}
