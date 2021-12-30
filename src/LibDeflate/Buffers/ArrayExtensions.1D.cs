using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibDeflate.Buffers;

internal static class ArrayExtensions
{
    /// <summary>
    /// Returns a reference to an element at a specified index within a given <typeparamref name="T"/> array, with no bounds checks.
    /// </summary>
    /// <typeparam name="T">The type of elements in the input <typeparamref name="T"/> array instance.</typeparam>
    /// <param name="array">The input <typeparamref name="T"/> array instance.</param>
    /// <param name="i">The index of the element to retrieve within <paramref name="array"/>.</param>
    /// <returns>A reference to the element within <paramref name="array"/> at the index specified by <paramref name="i"/>.</returns>
    /// <remarks>This method doesn't do any bounds checks, therefore it is responsibility of the caller to ensure the <paramref name="i"/> parameter is valid.</remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T DangerousGetReferenceAt<T>(this T[] array, int i)
    {
#if NET6_0_OR_GREATER
        ref T r0 = ref MemoryMarshal.GetArrayDataReference(array);
        ref T ri = ref Unsafe.Add(ref r0, (nint)(uint)i);

        return ref ri;
#elif NETCOREAPP3_1
        RawArrayData? arrayData = Unsafe.As<RawArrayData>(array)!;
        ref T r0 = ref Unsafe.As<byte, T>(ref arrayData.Data);
        ref T ri = ref Unsafe.Add(ref r0, (nint)(uint)i);

        return ref ri;
#else
        IntPtr offset = RuntimeHelpers.GetArrayDataByteOffset<T>();
        ref T r0 = ref ObjectMarshal.DangerousGetObjectDataReferenceAt<T>(array, offset);
        ref T ri = ref Unsafe.Add(ref r0, (nint)(uint)i);

        return ref ri;
#endif
    }

}
