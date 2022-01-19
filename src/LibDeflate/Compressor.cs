using LibDeflate.Buffers;
using LibDeflate.Imports;
using System;
using System.Buffers;

namespace LibDeflate;

using static Compression;

public abstract class Compressor : IDisposable
{
    protected readonly IntPtr compressor;

    private bool disposedValue;

    protected Compressor(int compressionLevel)
    {
        if (compressionLevel < 0 || compressionLevel > 12)
        {
            ThrowHelperBadCompressionLevel();
        }

        var compressor = libdeflate_alloc_compressor(compressionLevel);
        if (compressor == IntPtr.Zero)
        {
            ThrowHelperFailedAllocCompressor();
        }

        this.compressor = compressor;

        static void ThrowHelperBadCompressionLevel() => throw new ArgumentOutOfRangeException(nameof(compressionLevel));

        static void ThrowHelperFailedAllocCompressor() => throw new InvalidOperationException("Failed to allocate compressor");
    }
    ~Compressor() => Dispose(disposing: false);

    protected abstract nuint CompressCore(ReadOnlySpan<byte> input, Span<byte> output);

    protected abstract nuint GetBoundCore(nuint inputLength);

    public IMemoryOwner<byte>? Compress(ReadOnlySpan<byte> input, bool useUpperBound = false)
    {
        DisposedGuard();
        var output = MemoryOwner<byte>.Allocate(useUpperBound ? GetBound(input.Length) : input.Length);
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
    public int Compress(ReadOnlySpan<byte> input, Span<byte> output)
    {
        DisposedGuard();
        return (int)CompressCore(input, output);
    }

    public int GetBound(int inputLength)
    {
        DisposedGuard();
        return (int)GetBoundCore((nuint)inputLength);
    }

    private void DisposedGuard()
    {
        if(disposedValue)
        {
            ThrowHelperObjectDisposed();
        }

        static void ThrowHelperObjectDisposed() => throw new ObjectDisposedException(nameof(Compressor));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            //no managed state to dispose
            //if (disposing)
            //{
            //}

            libdeflate_free_compressor(compressor);
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
