using LibDeflate.Imports;
using System;
using System.Runtime.InteropServices;

namespace LibDeflate
{
    public sealed class GzipCompressor : Compressor
    {
        public GzipCompressor(int compressionLevel) : base(compressionLevel)
        {
        }

        protected override nuint Compress(ReadOnlySpan<byte> input, Span<byte> output)
            => Compression.libdeflate_gzip_compress(compressor, MemoryMarshal.GetReference(input), (nuint)input.Length, ref MemoryMarshal.GetReference(output), (nuint)output.Length);
    }
}
