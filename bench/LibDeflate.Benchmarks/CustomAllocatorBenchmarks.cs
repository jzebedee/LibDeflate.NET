using BenchmarkDotNet.Attributes;
using LibDeflate.Imports;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibDeflate.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class CustomAllocatorBenchmarks
{
    [GlobalSetup(Target = nameof(CompressorAllocCustom))]
    public void SetCustomAllocator()
    {
        Console.WriteLine("Custom Allocator: set");
        CustomMemoryAllocator.libdeflate_set_memory_allocator(malloc, free);

        static nint malloc(nuint len) => Marshal.AllocHGlobal((nint)len);

        static void free(nint alloc) => Marshal.FreeHGlobal(alloc);
    }

    [GlobalSetup(Target = nameof(CompressorAllocCustomUnsafe))]
    public unsafe void SetCustomAllocatorUnsafe()
    {
        Console.WriteLine("Custom Unsafe Allocator: set");
        CustomMemoryAllocator.libdeflate_set_memory_allocator_unsafe(&malloc_unsafe, &free_unsafe);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe void* malloc_unsafe(nuint len) => NativeMemory.Alloc(len);

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        static unsafe void free_unsafe(void* alloc) => NativeMemory.Free(alloc);
    }

    [Benchmark(Baseline = true)]
    public void CompressorAlloc()
    {
        var compressor = Compression.libdeflate_alloc_compressor(0);
        Compression.libdeflate_free_compressor(compressor);
    }

    [Benchmark]
    public void CompressorAllocCustom()
    {
        var compressor = Compression.libdeflate_alloc_compressor(0);
        Compression.libdeflate_free_compressor(compressor);
    }

    [Benchmark]
    public void CompressorAllocCustomUnsafe()
    {
        var compressor = Compression.libdeflate_alloc_compressor(0);
        Compression.libdeflate_free_compressor(compressor);
    }
}
