using System;

namespace LibDeflate.Tests;

internal static class Helpers
{
    public static Random GetRepeatableRandom() => new(63 * 13 * 17 * 13);
}
