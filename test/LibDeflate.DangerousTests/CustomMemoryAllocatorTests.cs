﻿using LibDeflate.Imports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

namespace LibDeflate.Tests;

public class CustomMemoryAllocatorTests
{
    private static int mallocCount = 0;
    private static int freeCount = 0;

    //This is not thread-safe, so we disable parallel tests in xunit.runner.json
    private static void VerifyAndResetCount()
    {
        (mallocCount, freeCount) = (0, 0);

        Assert.Equal(0, mallocCount);
        Assert.Equal(0, freeCount);
    }

    //[UnmanagedCallersOnly]
    private static nint malloc(nuint len)
    {
        mallocCount++;
        return Marshal.AllocHGlobal((nint)len);
    }

    //[UnmanagedCallersOnly]
    private static void free(nint alloc)
    {
        freeCount++;
        Marshal.FreeHGlobal(alloc);
    }

    [Fact]
    public void UseGlobalCustomAllocatorsTest()
    {
        VerifyAndResetCount();

        CustomMemoryAllocator.libdeflate_set_memory_allocator(malloc, free);

        //test compressor
        {
            //allocate something
            var compressor = Compression.libdeflate_alloc_compressor(0);
            Assert.Equal(1, mallocCount);

            //free something
            Compression.libdeflate_free_compressor(compressor);
            Assert.Equal(1, freeCount);
        }

        //test decompressor
        {
            var decompressor = Decompression.libdeflate_alloc_decompressor();
            Assert.Equal(2, mallocCount);

            Decompression.libdeflate_free_decompressor(decompressor);
            Assert.Equal(2, freeCount);
        }
    }

    [Fact]
    public void UsePerCompressorCustomAllocatorsTest()
    {
        int startingGlobalMallocs = mallocCount;
        int startingGlobalFrees = freeCount;

        int localMallocs = 0;
        int localFrees = 0;
        var options = new libdeflate_options((nuint len) =>
        {
            localMallocs++;
            return Marshal.AllocHGlobal((nint)len);
        }, (nint alloc) =>
        {
            localFrees++;
            Marshal.FreeHGlobal(alloc);
        });

        //test compressor
        {
            var compressor = Compression.libdeflate_alloc_compressor_ex(0, options);
            Assert.Equal(1, localMallocs);
            Assert.Equal(startingGlobalMallocs, mallocCount);

            Compression.libdeflate_free_compressor(compressor);
            Assert.Equal(1, localFrees);
            Assert.Equal(startingGlobalFrees, freeCount);
        }

        //test decompressor
        {
            var decompressor = Decompression.libdeflate_alloc_decompressor_ex(options);
            Assert.Equal(2, localMallocs);
            Assert.Equal(startingGlobalMallocs, mallocCount);

            Decompression.libdeflate_free_decompressor(decompressor);
            Assert.Equal(2, localFrees);
            Assert.Equal(startingGlobalFrees, freeCount);
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void* malloc_unsafe(nuint len)
    {
        mallocCount++;
        return (void*)Marshal.AllocHGlobal((nint)len);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static unsafe void free_unsafe(void* alloc)
    {
        freeCount++;
        Marshal.FreeHGlobal((nint)alloc);
    }

    [Fact]
    public unsafe void UseCustomAllocatorsUnsafeTest()
    {
        VerifyAndResetCount();

        CustomMemoryAllocator.libdeflate_set_memory_allocator_unsafe(&malloc_unsafe, &free_unsafe);

        //allocate something
        var compressor = Compression.libdeflate_alloc_compressor(0);
        Assert.Equal(1, mallocCount);

        //free something
        Compression.libdeflate_free_compressor(compressor);
        Assert.Equal(1, freeCount);
    }
}
