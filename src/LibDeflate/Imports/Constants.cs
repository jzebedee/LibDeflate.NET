using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo($"{nameof(LibDeflate)}.Tests")]
[assembly: InternalsVisibleTo($"{nameof(LibDeflate)}.DangerousTests")]
namespace LibDeflate.Imports;

internal static class Constants
{
    public const string DllName = "libdeflate";
    public const CallingConvention CallConv = CallingConvention.Cdecl;
}
