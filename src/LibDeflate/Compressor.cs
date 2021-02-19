using LibDeflate.Imports;
using Microsoft.Toolkit.HighPerformance.Buffers;
using System;

namespace LibDeflate
{
    using static Compression;

    public abstract class Compressor : IDisposable
    {
        protected readonly IntPtr compressor;

        private bool disposedValue;

        protected Compressor(int compressionLevel)
        {
            if (compressionLevel < 0 || compressionLevel > 12)
                throw new ArgumentOutOfRangeException(nameof(compressionLevel));

            var compressor = libdeflate_alloc_compressor(compressionLevel);
            if (compressor == IntPtr.Zero)
                throw new InvalidOperationException("Failed to allocate compressor");

            this.compressor = compressor;
        }
        ~Compressor() => DisposeCore();

        protected abstract nuint CompressCore(ReadOnlySpan<byte> input, Span<byte> output);

        public MemoryOwner<byte>? Compress(ReadOnlySpan<byte> input)
        {
            var output = MemoryOwner<byte>.Allocate(input.Length);
            try
            {
                nuint bytesWritten = CompressCore(input, output.Span);
                if (bytesWritten == UIntPtr.Zero)
                {
                    output.Dispose();
                    return null;
                }

                return output.Slice(0, (int)bytesWritten);
            }
            catch
            {
                output?.Dispose();
                throw;
            }
        }
        public int Compress(ReadOnlySpan<byte> input, Span<byte> output) => (int)CompressCore(input, output);

        private void DisposeCore()
        {
            if (!disposedValue)
            {
                libdeflate_free_compressor(compressor);
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            DisposeCore();
            GC.SuppressFinalize(this);
        }
    }
}
