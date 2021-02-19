using LibDeflate.Imports;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace LibDeflate.Tests
{
    public class CustomMemoryAllocatorTests
    {
        private static int mallocCount = 0;
        private static int freeCount = 0;

        private static nint malloc(nuint len)
        {
            mallocCount++;
            return Marshal.AllocHGlobal((nint)len);
        }

        private static void free(nint alloc)
        {
            freeCount++;
            Marshal.FreeHGlobal(alloc);
        }

        [Fact]
        public void UseCustomAllocatorsTest()
        {
            CustomMemoryAllocator.libdeflate_set_memory_allocator(malloc, free);

            //allocate something
            var compressor = Compression.libdeflate_alloc_compressor(0);
            Assert.Equal(1, mallocCount);

            //free something
            Compression.libdeflate_free_compressor(compressor);
            Assert.Equal(1, freeCount);
        }
    }
}
