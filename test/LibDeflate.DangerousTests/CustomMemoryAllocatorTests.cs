using LibDeflate.Imports;
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
    public void UseCustomAllocatorsTest()
    {
        VerifyAndResetCount();

        CustomMemoryAllocator.libdeflate_set_memory_allocator(malloc, free);

        //allocate something
        var compressor = Compression.libdeflate_alloc_compressor(0);
        Assert.Equal(1, mallocCount);

        //free something
        Compression.libdeflate_free_compressor(compressor);
        Assert.Equal(1, freeCount);
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
