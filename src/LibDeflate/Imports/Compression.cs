using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibDeflate.Imports
{
    public struct libdeflate_compressor { }

    public static class Compression
    {
        /*
         * libdeflate_alloc_compressor() allocates a new compressor that supports
         * DEFLATE, zlib, and gzip compression.  'compression_level' is the compression
         * level on a zlib-like scale but with a higher maximum value (1 = fastest, 6 =
         * medium/default, 9 = slow, 12 = slowest).  Level 0 is also supported and means
         * "no compression", specifically "create a valid stream, but only emit
         * uncompressed blocks" (this will expand the data slightly).
         *
         * The return value is a pointer to the new compressor, or NULL if out of memory
         * or if the compression level is invalid (i.e. outside the range [0, 12]).
         *
         * Note: for compression, the sliding window size is defined at compilation time
         * to 32768, the largest size permissible in the DEFLATE format.  It cannot be
         * changed at runtime.
         *
         * A single compressor is not safe to use by multiple threads concurrently.
         * However, different threads may use different compressors concurrently.
         */
        [DllImport(Constants.DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern ref libdeflate_compressor libdeflate_alloc_compressor(int compression_level);

        /*
         * libdeflate_deflate_compress() performs raw DEFLATE compression on a buffer of
         * data.  The function attempts to compress 'in_nbytes' bytes of data located at
         * 'in' and write the results to 'out', which has space for 'out_nbytes_avail'
         * bytes.  The return value is the compressed size in bytes, or 0 if the data
         * could not be compressed to 'out_nbytes_avail' bytes or fewer.
         */
        [DllImport(Constants.DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern libdeflate_compressor libdeflate_deflate_compress(ref libdeflate_compressor compressor, IntPtr @in, UIntPtr in_nbytes, IntPtr @out, UIntPtr out_nbytes_avail);
    }
}
