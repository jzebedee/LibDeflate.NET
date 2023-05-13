using LibDeflate.Buffers;
using LibDeflate.Imports;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace LibDeflate;
using static Decompression;

public abstract class Decompressor : IDisposable
{
    protected readonly IntPtr decompressor;

    private bool disposedValue;

    protected Decompressor()
    {
        var decompressor = libdeflate_alloc_decompressor();
        if (decompressor == IntPtr.Zero)
        {
            ThrowHelper_FailedAllocDecompressor();
        }

        this.decompressor = decompressor;

        [DoesNotReturn]
        static void ThrowHelper_FailedAllocDecompressor() => throw new InvalidOperationException("Failed to allocate decompressor");
    }
    ~Decompressor() => Dispose(disposing: false);

    internal static OperationStatus StatusFromResult(libdeflate_result result)
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        => result switch
        {
            libdeflate_result.LIBDEFLATE_SUCCESS => OperationStatus.Done,
            libdeflate_result.LIBDEFLATE_BAD_DATA => OperationStatus.InvalidData,
            libdeflate_result.LIBDEFLATE_SHORT_OUTPUT => OperationStatus.NeedMoreData,
            libdeflate_result.LIBDEFLATE_INSUFFICIENT_SPACE => OperationStatus.DestinationTooSmall,
        };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

    protected abstract OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, nuint uncompressedSize);
    protected abstract OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, out nuint bytesWritten);
    protected abstract OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, nuint uncompressedSize, out nuint bytesRead);
    protected abstract OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, out nuint bytesWritten, out nuint bytesRead);

    public OperationStatus Decompress(ReadOnlySpan<byte> input, int uncompressedSize, out IMemoryOwner<byte>? outputOwner, out int bytesRead)
    {
        DisposedGuard();
        var output = MemoryOwner<byte>.Allocate(uncompressedSize);
        try
        {
            var status = DecompressCore(input, output.Span, uncompressedSize: (nuint)uncompressedSize, bytesRead: out nuint in_nbytes);
            switch (status)
            {
                case OperationStatus.Done:
                    outputOwner = output;
                    bytesRead = (int)in_nbytes;
                    return status;
                case OperationStatus.NeedMoreData:
                case OperationStatus.DestinationTooSmall:
                case OperationStatus.InvalidData:
                default:
                    output.Dispose();
                    outputOwner = null;
                    bytesRead = default;
                    return status;
            }
        }
        catch
        {
            output.Dispose();
            throw;
        }
    }

    public OperationStatus Decompress(ReadOnlySpan<byte> input, int uncompressedSize, out IMemoryOwner<byte>? outputOwner)
    {
        DisposedGuard();
        var output = MemoryOwner<byte>.Allocate(uncompressedSize);
        try
        {
            var status = DecompressCore(input, output.Span, uncompressedSize: (nuint)uncompressedSize);
            switch (status)
            {
                case OperationStatus.Done:
                    outputOwner = output;
                    return status;
                case OperationStatus.NeedMoreData:
                case OperationStatus.DestinationTooSmall:
                case OperationStatus.InvalidData:
                default:
                    output.Dispose();
                    outputOwner = null;
                    return status;
            }
        }
        catch
        {
            output.Dispose();
            throw;
        }
    }

    public OperationStatus Decompress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesWritten, out int bytesRead)
    {
        DisposedGuard();
        var status = DecompressCore(input, output, out nuint out_nbytes, out nuint in_nbytes);
        switch (status)
        {
            case OperationStatus.Done:
                bytesWritten = (int)out_nbytes;
                bytesRead = (int)in_nbytes;
                return status;
            case OperationStatus.NeedMoreData:
            case OperationStatus.DestinationTooSmall:
            case OperationStatus.InvalidData:
            default:
                bytesWritten = default;
                bytesRead = default;
                return status;
        }
    }

    public OperationStatus Decompress(ReadOnlySpan<byte> input, Span<byte> output, out int bytesWritten)
    {
        DisposedGuard();
        var status = DecompressCore(input, output, out nuint out_nbytes);
        switch (status)
        {
            case OperationStatus.Done:
                bytesWritten = (int)out_nbytes;
                return status;
            case OperationStatus.NeedMoreData:
            case OperationStatus.DestinationTooSmall:
            case OperationStatus.InvalidData:
            default:
                bytesWritten = default;
                return status;
        }
    }

    private void DisposedGuard()
    {
        if (disposedValue)
        {
            ThrowHelperObjectDisposed();
        }

        [DoesNotReturn]
        static void ThrowHelperObjectDisposed() => throw new ObjectDisposedException(nameof(Decompressor));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            //no managed state to dispose
            //if (disposing)
            //{
            //}

            libdeflate_free_decompressor(decompressor);
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
