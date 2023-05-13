using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace LibDeflate.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class DeflateCompressorBenchmarks
{
    public static IEnumerable<object[]> Inputs => from key in TestFiles.Keys
                                                  from level in Levels
                                                  select new object[] { key, level };

    private static Dictionary<string, byte[]> TestFiles { get; } = Directory.EnumerateFiles(@"texts/")
                .Where(fn => !Path.GetExtension(fn).Equals(".gz", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(fn => Path.GetFileNameWithoutExtension(fn), File.ReadAllBytes);

    private static IEnumerable<int> Levels
    {
        get
        {
            //yield return 0;
            yield return 1;
            //yield return 6;
            //yield return 9;
        }
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(Inputs))]
    public void DeflateSIO(string testFile, int level)
    {
        var compressionLevel = level switch
        {
            0 => CompressionLevel.NoCompression,
            1 => CompressionLevel.Fastest,
            6 => CompressionLevel.Optimal,
            9 => CompressionLevel.SmallestSize
        };

        var input = TestFiles[testFile];
        using var outputMs = new MemoryStream(input.Length);
        using var deflateStream = new DeflateStream(outputMs, compressionLevel);
        deflateStream.Write(input);
    }

    [Benchmark]
    [ArgumentsSource(nameof(Inputs))]
    public void DeflateLibdeflate(string testFile, int level)
    {
        var input = TestFiles[testFile];
        using var compressor = new DeflateCompressor(level);
        using var owner = compressor.Compress(input);
    }
}
