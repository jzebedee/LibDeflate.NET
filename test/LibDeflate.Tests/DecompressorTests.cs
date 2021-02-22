using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace LibDeflate.Tests
{
    public class DecompressorTests
    {
        public delegate ReadOnlyMemory<byte> BclDeflater(ReadOnlyMemory<byte> inflatedInput);

        public static IEnumerable<object[]> Decompressors()
        {
            var input = GetRandomBuffer(length: 0x7900);

            yield return new object[] { new DeflateDecompressor(), input, (BclDeflater)Deflate };
            yield return new object[] { new ZlibDecompressor(), input, (BclDeflater)ZlibDeflate };
            yield return new object[] { new GzipDecompressor(), input, (BclDeflater)GzipDeflate };

            static ReadOnlyMemory<byte> Deflate(ReadOnlyMemory<byte> inflatedInput)
                => BclCompressionHelper.FlateToBuffer(inflatedInput.Span, CompressionMode.Compress);

            static ReadOnlyMemory<byte> ZlibDeflate(ReadOnlyMemory<byte> inflatedInput)
                => BclCompressionHelper.ZlibToBuffer(inflatedInput.Span, CompressionMode.Compress);

            static ReadOnlyMemory<byte> GzipDeflate(ReadOnlyMemory<byte> inflatedInput)
                => BclCompressionHelper.GzipToBuffer(inflatedInput.Span, CompressionMode.Compress);
        }

        private static readonly Random _rand = new();
        private static byte[] GetRandomBuffer(int length)
        {
            var input = new byte[length];
            _rand.NextBytes(input);
            return input;
        }


        [Theory]
        [MemberData(nameof(Decompressors))]
        public void DecompressProvidedBufferTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
        {
            using (decompressor)
            {
                //compress with BCL
                var bclDeflated = bclDeflater(inputMemory);
                //sanity-check
                //if (decompressor is ZlibDecompressor)
                //{
                //    var bclRoundtrip = BclCompressionHelper.ZlibToBuffer(bclDeflated.Span, CompressionMode.Decompress).Span;
                //    Assert.True(bclRoundtrip.SequenceEqual(inputMemory.Span));
                //}

                //decompress result with our lib
                var status = decompressor.Decompress(bclDeflated.Span, inputMemory.Length, out var outputOwner);
                Assert.Equal(OperationStatus.Done, status);
                Assert.NotNull(outputOwner);

                //ensure inflated results match input
                Assert.True(outputOwner.Span.SequenceEqual(inputMemory.Span));
            }
        }

        [Theory]
        [MemberData(nameof(Decompressors))]
        public void DecompressOversizedInputTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
        {
            var oversizedMs = new MemoryStream();
            var bclDeflated = bclDeflater(inputMemory);
            oversizedMs.Write(bclDeflated.Span);

            var expectedReadLength = oversizedMs.Length;
            Span<byte> appendedGarbage = new byte[0x40];
            _rand.NextBytes(appendedGarbage);
            oversizedMs.Write(appendedGarbage);

            var deflatedInput = oversizedMs.GetBuffer()[..(int)oversizedMs.Length];
            var inflatedOutput = new byte[inputMemory.Length];

            using (decompressor)
            {
                var status = decompressor.Decompress(deflatedInput, inflatedOutput, out var bytesWritten, out var bytesRead);

                var outSpan = new ReadOnlySpan<byte>(inflatedOutput, 0, bytesWritten);
                Assert.True(inputMemory.Span.SequenceEqual(outSpan));
                Assert.Equal(expectedReadLength, bytesRead);
            }
        }
    }
}
