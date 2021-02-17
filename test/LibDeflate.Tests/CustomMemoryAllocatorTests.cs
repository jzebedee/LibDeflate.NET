using LibDeflate.Imports;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Xunit;

namespace LibDeflate.Tests
{
    public class CustomMemoryAllocatorTests
    {
        [Fact]
        public void UseCustomAllocatorsTest()
        {
            CustomMemoryAllocator.libdeflate_set_memory_allocator(malloc, free);

            bool receivedmalloc = false, receivedfree = false;

            //allocate something
            var compressor = Compression.libdeflate_alloc_compressor(0);
            Assert.True(receivedmalloc);

            //free something
            Compression.libdeflate_free_compressor(compressor);
            Assert.True(receivedfree);

            return;

            IntPtr malloc(UIntPtr len)
            {
                Debug.WriteLine("Received malloc_func request for {0} bytes", (int)len);
                receivedmalloc = true;
                return Marshal.AllocHGlobal((int)len);
            }
            void free(IntPtr alloc)
            {
                Debug.WriteLine("Received free_func request for alloc {0}", alloc);
                receivedfree = true;
                Marshal.FreeHGlobal(alloc);
            }
        }
    }
}
