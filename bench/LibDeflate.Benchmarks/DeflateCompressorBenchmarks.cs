using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LibDeflate.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class DeflateCompressorBenchmarks
{
    private static string AssetBase
    {
        get
        {
            var cwd = Directory.GetCurrentDirectory();

            string assetsDir;
            while (!Directory.Exists(assetsDir = Path.Join(cwd, "assets")))
            {
                cwd = Path.GetDirectoryName(cwd);
            }

            return assetsDir;
        }
    }

    private static IEnumerable<int> Levels
    {
        get
        {
            yield return 0;
            yield return 1;
            yield return -1;
            yield return 9;
        }
    }

    [GlobalSetup]
    public static void PrepareTestAssets()
    {
        var assetsFolder = Path.Join(AssetBase, "UncompressedTestFiles");
        var testFiles = new Dictionary<string, byte[]>();
        foreach (var file in Directory.EnumerateFiles(assetsFolder, null, SearchOption.AllDirectories))
        {
            var key = Path.GetRelativePath(assetsFolder, file);
            testFiles.Add(key, File.ReadAllBytes(file));
        }

        TestFiles = testFiles;
    }

    public static Dictionary<string, byte[]> TestFiles { get; set; }

    public static IEnumerable<object[]> GetInputs() => from key in TestFiles.Keys
                                                       from level in Levels
                                                       select new object[] { key, level };

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static CompressionLevel ToLevelEnum(int level) => level switch
    {
        0 => CompressionLevel.NoCompression,
        1 => CompressionLevel.Fastest,
        -1 => CompressionLevel.Optimal,
        9 => CompressionLevel.SmallestSize
    };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(GetInputs))]
    public void DeflateSIO(string testFile, int level)
    {
        var input = TestFiles[testFile];
        using var outputMs = new MemoryStream(input.Length);
        using var deflateStream = new DeflateStream(outputMs, ToLevelEnum(level));
        deflateStream.Write(input);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetInputs))]
    public void DeflateLibdeflate_MemoryOwner(string testFile, int level)
    {
        var input = TestFiles[testFile];
        using var compressor = new DeflateCompressor(level);
        using var owner = compressor.Compress(input);
    }

    [Benchmark]
    [ArgumentsSource(nameof(GetInputs))]
    public void DeflateLibdeflate_Buffer(string testFile, int level)
    {
        var input = TestFiles[testFile];
        using var compressor = new DeflateCompressor(level);
        var output = new byte[input.Length];
        var bytesWritten = compressor.Compress(input, output);
    }
}
