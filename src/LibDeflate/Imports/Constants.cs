using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo($"{nameof(LibDeflate)}.Tests")]
namespace LibDeflate.Imports
{
    internal static class Constants
    {
        public const string DllName = @"libdeflate-1.7-windows-x86_64-bin\libdeflate.dll";
    }
}
