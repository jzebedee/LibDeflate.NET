using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibDeflate.Imports;

using libdeflate_compressor = System.IntPtr;
using size_t = System.UIntPtr;

internal static partial class Compression
{
    ///<summary>
    /// libdeflate_alloc_compressor() allocates a new compressor that supports
    /// DEFLATE, zlib, and gzip compression.  'compression_level' is the compression
    /// level on a zlib-like scale but with a higher maximum value (1 = fastest, 6 =
    /// medium/default, 9 = slow, 12 = slowest).  Level 0 is also supported and means
    /// "no compression", specifically "create a valid stream, but only emit
    /// uncompressed blocks" (this will expand the data slightly).
    ///
    /// The return value is a pointer to the new compressor, or NULL if out of memory
    /// or if the compression level is invalid (i.e. outside the range [0, 12]).
    ///
    /// Note: for compression, the sliding window size is defined at compilation time
    /// to 32768, the largest size permissible in the DEFLATE format.  It cannot be
    /// changed at runtime.
    ///
    /// A single compressor is not safe to use by multiple threads concurrently.
    /// However, different threads may use different compressors concurrently.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial libdeflate_compressor libdeflate_alloc_compressor(int compression_level);

    ///<summary>
    /// libdeflate_deflate_compress() performs raw DEFLATE compression on a buffer of
    /// data.  The function attempts to compress 'in_nbytes' bytes of data located at
    /// 'in' and write the results to 'out', which has space for 'out_nbytes_avail'
    /// bytes.  The return value is the compressed size in bytes, or 0 if the data
    /// could not be compressed to 'out_nbytes_avail' bytes or fewer.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial size_t libdeflate_deflate_compress(libdeflate_compressor compressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail);


    ///<summary>
    /// libdeflate_deflate_compress_bound() returns a worst-case upper bound on the
    /// number of bytes of compressed data that may be produced by compressing any
    /// buffer of length less than or equal to 'in_nbytes' using
    /// libdeflate_deflate_compress() with the specified compressor.  Mathematically,
    /// this bound will necessarily be a number greater than or equal to 'in_nbytes'.
    /// It may be an overestimate of the true upper bound.  The return value is
    /// guaranteed to be the same for all invocations with the same compressor and
    /// same 'in_nbytes'.
    ///
    /// As a special case, 'compressor' may be NULL.  This causes the bound to be
    /// taken across *any* libdeflate_compressor that could ever be allocated with
    /// this build of the library, with any options.
    ///
    /// Note that this function is not necessary in many applications.  With
    /// block-based compression, it is usually preferable to separately store the
    /// uncompressed size of each block and to store any blocks that did not compress
    /// to less than their original size uncompressed.  In that scenario, there is no
    /// need to know the worst-case compressed size, since the maximum number of
    /// bytes of compressed data that may be used would always be one less than the
    /// input length.  You can just pass a buffer of that size to
    /// libdeflate_deflate_compress() and store the data uncompressed if
    /// libdeflate_deflate_compress() returns 0, indicating that the compressed data
    /// did not fit into the provided output buffer.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial size_t libdeflate_deflate_compress_bound(libdeflate_compressor compressor, size_t in_nbytes);

    ///<summary>
    /// Like libdeflate_deflate_compress(), but stores the data in the zlib wrapper
    /// format.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial size_t libdeflate_zlib_compress(libdeflate_compressor compressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail);

    ///<summary>
    /// Like libdeflate_deflate_compress_bound(), but assumes the data will be
    /// compressed with libdeflate_zlib_compress() rather than with
    /// libdeflate_deflate_compress().
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial size_t libdeflate_zlib_compress_bound(libdeflate_compressor compressor, size_t in_nbytes);

    ///<summary>
    /// Like libdeflate_deflate_compress(), but stores the data in the gzip wrapper
    /// format.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial size_t libdeflate_gzip_compress(libdeflate_compressor compressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail);

    ///<summary>
    /// Like libdeflate_deflate_compress_bound(), but assumes the data will be
    /// compressed with libdeflate_gzip_compress() rather than with
    /// libdeflate_deflate_compress().
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial size_t libdeflate_gzip_compress_bound(libdeflate_compressor compressor, size_t in_nbytes);

    ///<summary>
    /// libdeflate_free_compressor() frees a compressor that was allocated with
    /// libdeflate_alloc_compressor().  If a NULL pointer is passed in, no action is
    /// taken.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = new[] { typeof(CallConvCdecl) })]
    public static partial void libdeflate_free_compressor(libdeflate_compressor compressor);
}
