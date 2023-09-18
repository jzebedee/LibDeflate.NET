using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace LibDeflate.Imports;

using size_t = UIntPtr;

internal static class CustomMemoryAllocator
{
    //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr malloc_func(size_t size);
    //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void free_func(IntPtr alloc);

    ///<summary>
    /// Install a custom memory allocator which libdeflate will use for all memory
    /// allocations.  'malloc_func' is a function that must behave like malloc(), and
    /// 'free_func' is a function that must behave like free().
    ///
    /// There must not be any libdeflate_compressor or libdeflate_decompressor
    /// structures in existence when calling this function.
    ///</summary>
    [DllImport(Constants.DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void libdeflate_set_memory_allocator(malloc_func malloc, free_func free);

    internal readonly struct libdeflate_options
    {
        private static readonly size_t Size = (nuint)(nint)Unsafe.SizeOf<libdeflate_options>();

        public libdeflate_options(malloc_func malloc, free_func free)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(malloc);
            ArgumentNullException.ThrowIfNull(free);
#else
            //TODO: add throwhelpers
            if(malloc is null)
            {
                throw new ArgumentNullException(nameof(malloc));
            }

            if(free is null)
            {
                throw new ArgumentNullException(nameof(free));
            }
#endif

            this.malloc = malloc;
            this.free = free;
        }

        /// <summary>
        ///  This field must be set to the struct size.  This field exists for
        ///  extensibility, so that fields can be appended to this struct in
        ///  future versions of libdeflate while still supporting old binaries.
        /// </summary>
        public readonly size_t sizeof_options = Size;
        /// <summary>
	    /// An optional custom memory allocator to use for this (de)compressor.
	    /// 'malloc_func' must be a function that behaves like malloc().
	    /// </summary>
        /// <remarks>
	    /// This is useful in cases where a process might have multiple users of
	    /// libdeflate who want to use different memory allocators.  For example,
	    /// a library might want to use libdeflate with a custom memory allocator
	    /// without interfering with user code that might use libdeflate too.
	    ///
	    /// This takes priority over the "global" memory allocator (which by
	    /// default is malloc() and free(), but can be changed by
	    /// libdeflate_set_memory_allocator()).  Moreover, libdeflate will never
	    /// call the "global" memory allocator if a per-(de)compressor custom
	    /// allocator is always given.
        /// </remarks>
        public readonly malloc_func malloc;
        /// <summary>
	    /// An optional custom memory deallocator to use for this (de)compressor.
	    /// 'free_func' must be a function that behaves like free().
	    /// </summary>
        /// <remarks>
	    /// This is useful in cases where a process might have multiple users of
	    /// libdeflate who want to use different memory allocators.  For example,
	    /// a library might want to use libdeflate with a custom memory allocator
	    /// without interfering with user code that might use libdeflate too.
	    ///
	    /// This takes priority over the "global" memory allocator (which by
	    /// default is malloc() and free(), but can be changed by
	    /// libdeflate_set_memory_allocator()).  Moreover, libdeflate will never
	    /// call the "global" memory allocator if a per-(de)compressor custom
	    /// allocator is always given.
        /// </remarks>
        public readonly free_func free;
    }
}
