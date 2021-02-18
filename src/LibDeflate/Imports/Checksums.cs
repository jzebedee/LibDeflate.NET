using System;
using System.Runtime.InteropServices;

namespace LibDeflate.Imports
{
    using size_t = System.UIntPtr;

    internal static class Checksums
    {
        ///<summary>
        /// libdeflate_adler32() updates a running Adler-32 checksum with 'len' bytes of
        /// data and returns the updated checksum.  When starting a new checksum, the
        /// required initial value for 'adler' is 1.  This value is also returned when
        /// 'buffer' is specified as NULL.
        ///</summary>
        [DllImport(Constants.DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 libdeflate_adler32(UInt32 adler, in byte buffer, size_t len);

        ///<summary>
        /// libdeflate_crc32() updates a running CRC-32 checksum with 'len' bytes of data
        /// and returns the updated checksum.  When starting a new checksum, the required
        /// initial value for 'crc' is 0.  This value is also returned when 'buffer' is
        /// specified as NULL.
        ///</summary>
        [DllImport(Constants.DllName, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 libdeflate_crc32(UInt32 crc, in byte buffer, size_t len);
    }
}
