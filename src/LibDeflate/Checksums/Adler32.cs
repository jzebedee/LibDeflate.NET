using System;
using System.Runtime.InteropServices;

namespace LibDeflate.Checksums;

using static Imports.Checksums;

public struct Adler32
{
    private uint _currentAdler;

    public Adler32() => _currentAdler = 1;

    public readonly uint Hash => _currentAdler;

    public void Append(ReadOnlySpan<byte> input)
        => _currentAdler = AppendCore(_currentAdler, input);

    public uint Compute(ReadOnlySpan<byte> input)
        => _currentAdler = AppendCore(1, input);

    private static uint AppendCore(uint adler, ReadOnlySpan<byte> input)
        => libdeflate_adler32(adler, MemoryMarshal.GetReference(input), (nuint)input.Length);
}
