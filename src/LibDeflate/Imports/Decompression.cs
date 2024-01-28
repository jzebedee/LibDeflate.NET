using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibDeflate.Imports;

using libdeflate_decompressor = nint;
using size_t = nuint;

internal static partial class Decompression
{
    ///<summary>
    /// Result of a call to libdeflate_deflate_decompress(),
    /// libdeflate_zlib_decompress(), or libdeflate_gzip_decompress().
    ///</summary>
    public enum libdeflate_result
    {
        ///<summary>
        /// Decompression was successful.
        ///</summary>
        LIBDEFLATE_SUCCESS = 0,

        ///<summary>
        /// Decompressed failed because the compressed data was invalid, corrupt,
        /// or otherwise unsupported.
        ///</summary>
        LIBDEFLATE_BAD_DATA = 1,

        ///<summary>
        /// A NULL 'actual_out_nbytes_ret' was provided, but the data would have
        /// decompressed to fewer than 'out_nbytes_avail' bytes.
        ///</summary>
        LIBDEFLATE_SHORT_OUTPUT = 2,

        ///<summary>
        /// The data would have decompressed to more than 'out_nbytes_avail' bytes.
        ///</summary>
        LIBDEFLATE_INSUFFICIENT_SPACE = 3,
    }

    ///<summary>
    /// libdeflate_alloc_decompressor() allocates a new decompressor that can be used
    /// for DEFLATE, zlib, and gzip decompression.  The return value is a pointer to
    /// the new decompressor, or NULL if out of memory.
    ///
    /// This function takes no parameters, and the returned decompressor is valid for
    /// decompressing data that was compressed at any compression level and with any
    /// sliding window size.
    ///
    /// A single decompressor is not safe to use by multiple threads concurrently.
    /// However, different threads may use different decompressors concurrently.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_decompressor libdeflate_alloc_decompressor();

    /// <summary>
    /// Like <see cref="libdeflate_alloc_decompressor"/> but allows specifying advanced options per-decompressor.
    /// </summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_decompressor libdeflate_alloc_decompressor_ex(in libdeflate_options options);

    ///<summary>
    /// libdeflate_deflate_decompress() decompresses the DEFLATE-compressed stream
    /// from the buffer 'in' with compressed size up to 'in_nbytes' bytes.  The
    /// uncompressed data is written to 'out', a buffer with size 'out_nbytes_avail'
    /// bytes.  If decompression succeeds, then 0 (LIBDEFLATE_SUCCESS) is returned.
    /// Otherwise, a nonzero result code such as LIBDEFLATE_BAD_DATA is returned.  If
    /// a nonzero result code is returned, then the contents of the output buffer are
    /// undefined.
    ///
    /// Decompression stops at the end of the DEFLATE stream (as indicated by the
    /// BFINAL flag), even if it is actually shorter than 'in_nbytes' bytes.
    ///
    /// libdeflate_deflate_decompress() can be used in cases where the actual
    /// uncompressed size is known (recommended) or unknown (not recommended):
    ///
    ///   - If the actual uncompressed size is known, then pass the actual
    ///     uncompressed size as 'out_nbytes_avail' and pass NULL for
    ///     'actual_out_nbytes_ret'.  This makes libdeflate_deflate_decompress() fail
    ///     with LIBDEFLATE_SHORT_OUTPUT if the data decompressed to fewer than the
    ///     specified number of bytes.
    ///
    ///   - If the actual uncompressed size is unknown, then provide a non-NULL
    ///     'actual_out_nbytes_ret' and provide a buffer with some size
    ///     'out_nbytes_avail' that you think is large enough to hold all the
    ///     uncompressed data.  In this case, if the data decompresses to less than
    ///     or equal to 'out_nbytes_avail' bytes, then
    ///     libdeflate_deflate_decompress() will write the actual uncompressed size
    ///     to *actual_out_nbytes_ret and return 0 (LIBDEFLATE_SUCCESS).  Otherwise,
    ///     it will return LIBDEFLATE_INSUFFICIENT_SPACE if the provided buffer was
    ///     not large enough but no other problems were encountered, or another
    ///     nonzero result code if decompression failed for another reason.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_result libdeflate_deflate_decompress(libdeflate_decompressor decompressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail, out size_t actual_out_nbytes_ret);

    ///<summary>
    /// Like libdeflate_deflate_decompress(), but adds the 'actual_in_nbytes_ret'
    /// argument.  If decompression succeeds and 'actual_in_nbytes_ret' is not NULL,
    /// then the actual compressed size of the DEFLATE stream (aligned to the next
    /// byte boundary) is written to *actual_in_nbytes_ret.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_result libdeflate_deflate_decompress_ex(libdeflate_decompressor decompressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail, out size_t actual_in_nbytes_ret, out size_t actual_out_nbytes_ret);

    ///<summary>
    /// Like libdeflate_deflate_decompress(), but assumes the zlib wrapper format
    /// instead of raw DEFLATE.
    ///
    /// Decompression will stop at the end of the zlib stream, even if it is shorter
    /// than 'in_nbytes'.  If you need to know exactly where the zlib stream ended,
    /// use libdeflate_zlib_decompress_ex().
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_result libdeflate_zlib_decompress(libdeflate_decompressor decompressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail, out size_t actual_out_nbytes_ret);

    ///<summary>
    /// Like libdeflate_deflate_decompress(), but assumes the zlib wrapper format
    /// instead of raw DEFLATE.
    ///
    /// Decompression will stop at the end of the zlib stream, even if it is shorter
    /// than 'in_nbytes'.  If you need to know exactly where the zlib stream ended,
    /// use libdeflate_zlib_decompress_ex().
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_result libdeflate_zlib_decompress_ex(libdeflate_decompressor decompressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail, out size_t actual_in_nbytes_ret, out size_t actual_out_nbytes_ret);

    ///<summary>
    /// Like libdeflate_deflate_decompress(), but assumes the gzip wrapper format
    /// instead of raw DEFLATE.
    ///
    /// If multiple gzip-compressed members are concatenated, then only the first
    /// will be decompressed.  Use libdeflate_gzip_decompress_ex() if you need
    /// multi-member support.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_result libdeflate_gzip_decompress(libdeflate_decompressor decompressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail, out size_t actual_out_nbytes_ret);

    ///<summary>
    /// Like libdeflate_gzip_decompress(), but adds the 'actual_in_nbytes_ret'
    /// argument.  If 'actual_in_nbytes_ret' is not NULL and the decompression
    /// succeeds (indicating that the first gzip-compressed member in the input
    /// buffer was decompressed), then the actual number of input bytes consumed is
    /// written to *actual_in_nbytes_ret.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial libdeflate_result libdeflate_gzip_decompress_ex(libdeflate_decompressor decompressor, in byte @in, size_t in_nbytes, ref byte @out, size_t out_nbytes_avail, out size_t actual_in_nbytes_ret, out size_t actual_out_nbytes_ret);

    ///<summary>
    /// libdeflate_free_decompressor() frees a decompressor that was allocated with
    /// libdeflate_alloc_decompressor().  If a NULL pointer is passed in, no action
    /// is taken.
    ///</summary>
    [LibraryImport(Constants.DllName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void libdeflate_free_decompressor(libdeflate_decompressor compressor);
}