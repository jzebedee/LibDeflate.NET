using System;
using System.Runtime.InteropServices;

namespace LibDeflate.Checksums;

using static Imports.Checksums;

public struct Adler32
{
    private bool _initialized;
    //because we have to supply 1 as the initial value
    //we must always retrieve this through Hash
    private uint _currentAdler;
    public uint Hash
    {
        get
        {
            if (!_initialized)
            {
                _currentAdler = 1;
                _initialized = true;
            }
            return _currentAdler;
        }
    }

    public void Append(ReadOnlySpan<byte> input)
        => _currentAdler = AppendCore(Hash, input);

    public uint Compute(ReadOnlySpan<byte> input)
        => _currentAdler = AppendCore(1, input);

    private static uint AppendCore(uint adler, ReadOnlySpan<byte> input)
        => libdeflate_adler32(adler, MemoryMarshal.GetReference(input), (nuint)input.Length);
}
