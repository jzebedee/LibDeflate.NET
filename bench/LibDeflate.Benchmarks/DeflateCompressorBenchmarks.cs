using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LibDeflate.Benchmarks;

public class DeflateCompressorBenchmarks
{
    private static Random GetRepeatableRandom() => new(63 * 13 * 17 * 13);
    private static byte[] GetRandomBuffer(int length)
    {
        var rand = GetRepeatableRandom();
        var input = new byte[length];
        rand.NextBytes(input);
        return input;
    }

    public static IEnumerable<object[]> Inputs => from buffer in Buffers
                                                  from level in Levels
                                                  select new object[] { buffer, level };

    private static IEnumerable<byte[]> Buffers
    {
        get
        {
            yield return GetRandomBuffer(0);
            yield return GetRandomBuffer(1 << 0);
            yield return GetRandomBuffer(1 << 4);
            yield return GetRandomBuffer(1 << 8);
            yield return GetRandomBuffer(1 << 16);
            //yield return GetRandomBuffer(int.MaxValue);
        }
    }

    private static IEnumerable<int> Levels
    {
        get
        {
            yield return 0;
            yield return 1;
            yield return 6;
            yield return 9;
        }
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(Inputs))]
    public void DeflateSIO(byte[] input, int level)
    {
        var compressionLevel = level switch
        {
            0 => CompressionLevel.NoCompression,
            1 => CompressionLevel.Fastest,
            6 => CompressionLevel.Optimal,
            9 => CompressionLevel.SmallestSize
        };

        using var outputMs = new MemoryStream(input.Length);
        using var deflateStream = new DeflateStream(outputMs, compressionLevel);
        deflateStream.Write(input);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Inputs))]
    public void DeflateLibdeflate(byte[] input, int level)
    {
        using var compressor = new DeflateCompressor(level);
        using var owner = compressor.Compress(input);
    }
}
