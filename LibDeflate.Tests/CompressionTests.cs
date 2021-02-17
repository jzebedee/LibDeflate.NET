using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LibDeflate.Tests
{
    public class CompressionTests
    {
        public static IEnumerable<object[]> CompressionLevels => Enumerable.Range(0, 13).Select(i => new object[] { i });

        [Theory]
        [MemberData(nameof(CompressionLevels))]
        public void AllocCompressorTest(int compressionLevel)
        {
            var compressor = Imports.Compression.libdeflate_alloc_compressor(compressionLevel);
        }
    }
}
