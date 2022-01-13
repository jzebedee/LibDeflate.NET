using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo($"{nameof(LibDeflate)}.Tests")]
[assembly: InternalsVisibleTo($"{nameof(LibDeflate)}.DangerousTests")]
namespace LibDeflate.Imports;

internal static class Constants
{
    public const string DllName = "libdeflate";
}
