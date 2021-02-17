using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibDeflate.Imports
{
    using libdeflate_decompressor = System.IntPtr;
    using size_t = System.UIntPtr;

    public static class Decompression
    {
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
        [DllImport(Constants.DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern libdeflate_decompressor libdeflate_alloc_decompressor();

    }
}
