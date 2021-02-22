# LibDeflate.NET [![nuget stable](https://img.shields.io/nuget/v/LibDeflate.NET.svg?style=flat)](https://www.nuget.org/packages/LibDeflate.NET)

LibDeflate.NET is a managed wrapper around [libdeflate](https://github.com/ebiggers/libdeflate), a native library for fast, whole-buffer DEFLATE-based compression and decompression.

Native binaries of libdeflate are packaged in [LibDeflate.Native](https://github.com/jzebedee/LibDeflate.Native).

## Usage

### Compression

#### Creating a `Compressor`

Create a compressor for the DEFLATE ([RFC 1951](https://www.ietf.org/rfc/rfc1951.txt)) format.
```c#
using LibDeflate;

using Compressor compressor = new DeflateCompressor(compressionLevel: 9);
```

Create a compressor for the zlib ([RFC 1950](https://www.ietf.org/rfc/rfc1950.txt)) format.
```c#
using LibDeflate;

using Compressor compressor = new ZlibCompressor(compressionLevel: 9);
```

Create a compressor for the gzip ([RFC 1952](https://www.ietf.org/rfc/rfc1952.txt)) format.
```c#
using LibDeflate;

using Compressor compressor = new GzipCompressor(compressionLevel: 9);
```

#### Compressing with a `Compressor`

Compress text using a `Compressor`
```c#
using System;
using System.Text;
using LibDeflate;

using Compressor compressor = new DeflateCompressor(compressionLevel: 9);

// This will fail, because it is too short to compress effectively
CompressString("Hello, world!");
// These will succeed, because they are long enough to compress effectively
CompressString("Gases have the lowest density of the three, are highly compressible, and completely fill any container in which they are placed.");
CompressString("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis fermentum lacus a est auctor, ut ultrices leo lobortis. Cras ac enim neque. Maecenas nisi nulla, auctor a nisi ut, mollis auctor nibh. Donec lacinia augue neque, at molestie lectus porta et. Maecenas posuere sed nisi ac vehicula. Ut id nisl a dui fringilla rhoncus sit amet id erat. In sagittis sem est, quis porta diam tempor id. Vestibulum non cursus justo. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Vivamus in malesuada urna, sed volutpat mi. Donec dapibus nibh sodales, cursus mauris quis, tempus mauris. In eget fermentum nunc. Nunc convallis luctus libero. Nulla sed eros nec tortor scelerisque euismod.");

void CompressString(string input)
{
    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
    //Compress returns a System.Buffers.IMemoryOwner<byte>, which should be disposed after use
    using var ownedMemory = compressor.Compress(inputBytes);
    if (ownedMemory == null)
    {
        Console.WriteLine("Compressing this {0} byte input would output greater than {0} bytes!", inputBytes.Length);
        return;
    }
    Console.WriteLine("Compressed this {0} byte input to {1} byte output ({2:0.0%} ratio)", inputBytes.Length, ownedMemory.Length, (double)ownedMemory.Length / inputBytes.Length);
}
```

This produces the output:
```
Compressing this 13 byte input would output greater than 13 bytes!
Compressed this 128 byte input to 95 byte output (74.2% ratio)
Compressed this 717 byte input to 382 byte output (53.3% ratio)
```

### Decompression

#### Creating a `Decompressor`

Create a decompressor for the DEFLATE ([RFC 1951](https://www.ietf.org/rfc/rfc1951.txt)) format.
```c#
using LibDeflate;

using Decompressor decompressor = new DeflateDecompressor();
```

Create a decompressor for the zlib ([RFC 1950](https://www.ietf.org/rfc/rfc1950.txt)) format.
```c#
using LibDeflate;

using Decompressor decompressor = new ZlibDecompressor();
```

Create a decompressor for the gzip ([RFC 1952](https://www.ietf.org/rfc/rfc1952.txt)) format.
```c#
using LibDeflate;

using Decompressor decompressor = new GzipDecompressor();
```

#### Decompressing with a `Decompressor`

Compress text with a `Compressor` and immediately decompress it with a `Decompressor`
```c#
using System;
using System.Buffers;
using System.Text;
using LibDeflate;

///
// Compression
///

using Compressor compressor = new DeflateCompressor(compressionLevel: 9);

// These will succeed, because they are long enough to compress effectively
const string exampleText1 = "Gases have the lowest density of the three, are highly compressible, and completely fill any container in which they are placed.";
const string exampleText2 = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis fermentum lacus a est auctor, ut ultrices leo lobortis. Cras ac enim neque. Maecenas nisi nulla, auctor a nisi ut, mollis auctor nibh. Donec lacinia augue neque, at molestie lectus porta et. Maecenas posuere sed nisi ac vehicula. Ut id nisl a dui fringilla rhoncus sit amet id erat. In sagittis sem est, quis porta diam tempor id. Vestibulum non cursus justo. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Vivamus in malesuada urna, sed volutpat mi. Donec dapibus nibh sodales, cursus mauris quis, tempus mauris. In eget fermentum nunc. Nunc convallis luctus libero. Nulla sed eros nec tortor scelerisque euismod.";
using var compressedString1 = CompressString(exampleText1, out int uncompressedLength1);
using var compressedString2 = CompressString(exampleText2, out int uncompressedLength2);

IMemoryOwner<byte> CompressString(string input, out int uncompressedSize)
{
    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
    // libdeflate recommends storing the uncompressed size and using it for decompression.
    // Otherwise, we would need to guess at how big the uncompressed output buffer is.
    uncompressedSize = inputBytes.Length;
    // Compress returns a System.Buffers.IMemoryOwner<byte>, which should be disposed after use
    var ownedMemory = compressor.Compress(inputBytes);
    if (ownedMemory == null)
    {
        Console.WriteLine("Compressing this {0} byte input would output greater than {0} bytes!", inputBytes.Length);
        return null;
    }
    Console.WriteLine("Compressed this {0} byte input to {1} byte output ({2:0.0%} ratio)", inputBytes.Length, ownedMemory.Length, (double)ownedMemory.Length / inputBytes.Length);
    return ownedMemory;
}

///
// Decompression
///

using Decompressor decompressor = new DeflateDecompressor();

var decompressedString1 = DecompressString(compressedString1.Memory.Span, uncompressedLength1);
var decompressedString2 = DecompressString(compressedString2.Memory.Span, uncompressedLength2);

string DecompressString(ReadOnlySpan<byte> compressedInput, int uncompressedSize)
{
    // Compress returns a System.Buffers.OperationStatus, which should be checked for success before using the output parameters
    var status = decompressor.Decompress(compressedInput, uncompressedSize, out var ownedMemory);
    if (status != OperationStatus.Done)
    {
        Console.WriteLine("Failed to decompress data!");
        return null;
    }

    Console.WriteLine("Decompressed this {0} byte input to {1} byte output", compressedInput.Length, ownedMemory.Length);
    var decompressedBytes = ownedMemory.Memory.Span;
    var decompressedString = Encoding.UTF8.GetString(decompressedBytes);
    Console.WriteLine("Decompressed output: {0}", decompressedString);
    return decompressedString;
}
```

This produces the output:
```
Decompressed this 95 byte input to 128 byte output
Decompressed output: Gases have the lowest density of the three, are highly compressible, and completely fill any container in which they are placed.
Decompressed this 382 byte input to 717 byte output
Decompressed output: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duis fermentum lacus a est auctor, ut ultrices leo lobortis. Cras ac enim neque. Maecenas nisi nulla, auctor a nisi ut, mollis auctor nibh. Donec lacinia augue neque, at molestie lectus porta et. Maecenas posuere sed nisi ac vehicula. Ut id nisl a dui fringilla rhoncus sit amet id erat. In sagittis sem est, quis porta diam tempor id. Vestibulum non cursus justo. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Vivamus in malesuada urna, sed volutpat mi. Donec dapibus nibh sodales, cursus mauris quis, tempus mauris. In eget fermentum nunc. Nunc convallis luctus libero. Nulla sed eros nec tortor scelerisque euismod.
```
