using LibDeflate.Imports;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibDeflate;

using static Decompression;

public sealed class GzipDecompressor : Decompressor
{
    public GzipDecompressor() : base()
    {
    }

    protected override OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, nuint uncompressedSize)
        => StatusFromResult(libdeflate_gzip_decompress(decompressor, MemoryMarshal.GetReference(input),
            (nuint)input.Length, ref MemoryMarshal.GetReference(output), uncompressedSize, out Unsafe.NullRef<UIntPtr>()));

    protected override OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, out nuint bytesWritten)
        => StatusFromResult(libdeflate_gzip_decompress(decompressor, MemoryMarshal.GetReference(input),
            (nuint)input.Length, ref MemoryMarshal.GetReference(output), (nuint)output.Length, out bytesWritten));

    protected override OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, nuint uncompressedSize, out nuint bytesRead)
        => StatusFromResult(libdeflate_gzip_decompress_ex(decompressor, MemoryMarshal.GetReference(input),
            (nuint)input.Length, ref MemoryMarshal.GetReference(output), uncompressedSize, out bytesRead, out Unsafe.NullRef<UIntPtr>()));

    protected override OperationStatus DecompressCore(ReadOnlySpan<byte> input, Span<byte> output, out nuint bytesWritten, out nuint bytesRead)
        => StatusFromResult(libdeflate_gzip_decompress_ex(decompressor, MemoryMarshal.GetReference(input),
            (nuint)input.Length, ref MemoryMarshal.GetReference(output), (nuint)output.Length, out bytesRead, out bytesWritten));
}
