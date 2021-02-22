using LibDeflate.Checksums;
using System;
using Xunit;

namespace LibDeflate.Tests
{
    public class ChecksumTests
    {
        private static readonly Random _rand = new();
        private static byte[] GetRandomBuffer(int length)
        {
            var input = new byte[length];
            _rand.NextBytes(input);
            return input;
        }

        private uint NaiveCrc32(ReadOnlySpan<byte> input, uint poly = 0xEDB88320)
        {
            uint crc = unchecked((uint)-1);

            while (!input.IsEmpty)
            {
                crc ^= input[0];
                input = input.Slice(1);
                for (int bit = 0; bit < 8; bit++)
                {
                    if ((crc & 1) == 1) crc = (crc >> 1) ^ poly;
                    else crc >>= 1;
                }
            }
            return ~crc;
        }

        [Fact]
        public void Crc32ComputeTest()
        {
            var testBuffer = GetRandomBuffer(0x1000);

            var naiveCrc32 = NaiveCrc32(testBuffer);
            var lbdCrc32 = new Crc32().Compute(testBuffer);

            Assert.Equal(naiveCrc32, lbdCrc32);
        }

        [Fact]
        public void Crc32AppendTest()
        {
            Span<byte> testBuffer = GetRandomBuffer(0x1000);

            var naiveCrc32 = NaiveCrc32(testBuffer);
            var lbdCrc32 = new Crc32();
            while (!testBuffer.IsEmpty)
            {
                var chunk = testBuffer[..0x100];
                lbdCrc32.Append(chunk);
                testBuffer = testBuffer[0x100..];
            }

            Assert.Equal(naiveCrc32, lbdCrc32.Hash);
        }

        private uint NaiveAdler32(ReadOnlySpan<byte> input, uint mparam = 65521)
        {
            uint a = 1, b = 0;
            for (int i = 0; i < input.Length; i++)
            {
                uint p = input[i];
                b = (b + (a = (a + p) % (p = mparam))) % p;
            }
            return b << 16 | a;
        }

        [Fact]
        public void Adler32ComputeTest()
        {
            var testBuffer = GetRandomBuffer(0x1000);

            var naiveAdler32 = NaiveAdler32(testBuffer);
            var lbdAdler32 = new Adler32().Compute(testBuffer);

            Assert.Equal(naiveAdler32, lbdAdler32);
        }

        [Fact]
        public void Adler32AppendTest()
        {
            Span<byte> testBuffer = GetRandomBuffer(0x1000);

            var naiveAdler32 = NaiveAdler32(testBuffer);
            var lbdAdler32 = new Adler32();
            while (!testBuffer.IsEmpty)
            {
                var chunk = testBuffer[..0x100];
                lbdAdler32.Append(chunk);
                testBuffer = testBuffer[0x100..];
            }

            Assert.Equal(naiveAdler32, lbdAdler32.Hash);
        }
    }
}
