using LibDeflate.Imports;
using System;
using System.Buffers;

namespace LibDeflate
{
    public abstract class Compressor : IDisposable
    {
        protected readonly IntPtr compressor;

        private bool disposedValue;

        protected Compressor(int compressionLevel)
        {
            if (compressionLevel < 0 || compressionLevel > 12)
                throw new ArgumentOutOfRangeException(nameof(compressionLevel));

            var compressor = Compression.libdeflate_alloc_compressor(compressionLevel);
            if (compressor == IntPtr.Zero)
                throw new InvalidOperationException("Failed to allocate compressor");

            this.compressor = compressor;
        }
        ~Compressor() => DisposeCore();

        protected abstract nuint Compress(ReadOnlySpan<byte> input, Span<byte> output);

        public bool TryCompress(ReadOnlySpan<byte> input, out IMemoryOwner<byte> output, out uint bytesWritten)
        {
            var outputOwner = MemoryPool<byte>.Shared.Rent(input.Length);
            var outputBuffer = outputOwner.Memory.Span;

            nuint outputWritten = Compress(input, outputBuffer);
            if (outputWritten == UIntPtr.Zero)
            {
                outputOwner.Dispose();
                output = null;
                bytesWritten = 0;
                return false;
            }

            output = outputOwner;
            bytesWritten = (uint)outputWritten;
            return true;
        }

        private void DisposeCore()
        {
            if (!disposedValue)
            {
                Compression.libdeflate_free_compressor(compressor);
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
