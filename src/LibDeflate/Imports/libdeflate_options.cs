using System;
using System.Runtime.CompilerServices;
namespace LibDeflate.Imports;

using static LibDeflate.Imports.CustomMemoryAllocator;
using size_t = UIntPtr;

/// <summary>
/// Advanced options.  This is the options structure that
/// <see cref="Compression.libdeflate_alloc_compressor_ex"/>
/// and <see cref="Decompression.libdeflate_alloc_decompressor_ex"/>
/// require.  Most users won't need this and should just use the non-"_ex"
/// functions instead.
/// </summary>
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
        if (malloc is null)
        {
            throw new ArgumentNullException(nameof(malloc));
        }

        if (free is null)
        {
            throw new ArgumentNullException(nameof(free));
        }
#endif
        this.sizeof_options = Size;
        this.malloc = malloc;
        this.free = free;
    }

    /// <summary>
    ///  This field must be set to the struct size.  This field exists for
    ///  extensibility, so that fields can be appended to this struct in
    ///  future versions of libdeflate while still supporting old binaries.
    /// </summary>
    public readonly size_t sizeof_options;

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
