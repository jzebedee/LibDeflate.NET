using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace LibDeflate.Tests;
using static Helpers;

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

    private static byte[] GetRandomBuffer(int length)
    {
        var rand = GetRepeatableRandom();
        var input = new byte[length];
        rand.NextBytes(input);
        return input;
    }

    private static byte[] GetOversizedInputBuffer(ReadOnlySpan<byte> input, out int expectedReadLength)
    {
        var rand = GetRepeatableRandom();
        var oversizedMs = new MemoryStream();
        oversizedMs.Write(input);

        expectedReadLength = (int)oversizedMs.Length;
        Span<byte> appendedGarbage = new byte[0x40];
        rand.NextBytes(appendedGarbage);
        oversizedMs.Write(appendedGarbage);

        return oversizedMs.GetBuffer()[..(int)oversizedMs.Length];
    }

    [Theory]
    [MemberData(nameof(Decompressors))]
    public void DisposedDecompressorThrows(Decompressor decompressor, ReadOnlyMemory<byte> input, BclDeflater bclDeflater)
    {
        //compress with BCL
        var bclDeflated = bclDeflater(input);

        using (decompressor)
        {
            //make sure we don't throw when not disposed
            Decompress();
        }

        Assert.Throws<ObjectDisposedException>(Decompress);

        void Decompress()
        {
            ReadOnlySpan<byte> inSpan = bclDeflated.Span;
            decompressor.Decompress(inSpan, input.Length, out var owner);
            owner.Dispose();

            Span<byte> outSpan = new byte[input.Length];
            decompressor.Decompress(inSpan, outSpan, out int bytesWritten);

            decompressor.Decompress(inSpan, input.Length, out owner, out int bytesRead);
            owner.Dispose();

            decompressor.Decompress(inSpan, outSpan, out bytesWritten, out bytesRead);
        }
    }

    [Theory]
    [MemberData(nameof(Decompressors))]
    public void DecompressOwnedBufferTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
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
            using (outputOwner)
            {
                Assert.Equal(OperationStatus.Done, status);
                Assert.NotNull(outputOwner);

                //ensure inflated results match input
                var output = outputOwner.Memory.Span;
                Assert.True(output.SequenceEqual(inputMemory.Span));
            }
        }
    }

    [Theory]
    [MemberData(nameof(Decompressors))]
    public void DecompressProvidedBufferTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
    {
        using (decompressor)
        {
            //compress with BCL
            var bclDeflated = bclDeflater(inputMemory);

            Span<byte> outputSpan = new byte[inputMemory.Length + 0x1000];

            //decompress result with our lib
            var status = decompressor.Decompress(bclDeflated.Span, outputSpan, out int bytesWritten);
            Assert.Equal(OperationStatus.Done, status);
            Assert.True(bytesWritten > 0);

            //ensure inflated results match input
            Assert.True(outputSpan[..bytesWritten].SequenceEqual(inputMemory.Span));
        }
    }

    [Theory]
    [MemberData(nameof(Decompressors))]
    public void DecompressOwnedShortBufferTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
    {
        using (decompressor)
        {
            //compress with BCL
            var bclDeflated = bclDeflater(inputMemory);

            //decompress result with our lib
            var status = decompressor.Decompress(bclDeflated.Span, inputMemory.Length - 1, out var outputOwner);
            Assert.NotEqual(OperationStatus.Done, status);
            Assert.Null(outputOwner);
        }
    }

    [Theory]
    [MemberData(nameof(Decompressors))]
    public void DecompressOversizedInputTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
    {
        var bclDeflated = bclDeflater(inputMemory);
        var deflatedInput = GetOversizedInputBuffer(bclDeflated.Span, out var expectedReadLength);
        var inflatedOutput = new byte[inputMemory.Length];

        using (decompressor)
        {
            var status = decompressor.Decompress(deflatedInput, inflatedOutput, out var bytesWritten, out int bytesRead);
            Assert.Equal(OperationStatus.Done, status);

            var outSpan = new ReadOnlySpan<byte>(inflatedOutput, 0, bytesWritten);
            Assert.True(inputMemory.Span.SequenceEqual(outSpan));
            Assert.Equal(expectedReadLength, bytesRead);
        }
    }

    [Theory]
    [MemberData(nameof(Decompressors))]
    public void DecompressOversizedInputUnknownSizeTest(Decompressor decompressor, ReadOnlyMemory<byte> inputMemory, BclDeflater bclDeflater)
    {
        var bclDeflated = bclDeflater(inputMemory);
        var deflatedInput = GetOversizedInputBuffer(bclDeflated.Span, out var expectedReadLength);

        using (decompressor)
        {
            var status = decompressor.Decompress(deflatedInput, inputMemory.Length, out var outputOwner, out int bytesRead);
            using (outputOwner)
            {
                Assert.Equal(OperationStatus.Done, status);
                Assert.NotNull(outputOwner);

                var output = outputOwner.Memory.Span;
                Assert.True(inputMemory.Span.SequenceEqual(output));
                Assert.Equal(expectedReadLength, bytesRead);
            }
        }
    }
}
