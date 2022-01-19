using System;
using System.Diagnostics.Contracts;
#if !NETSTANDARD2_1_OR_GREATER
using System.Reflection;
#endif
using System.Runtime.CompilerServices;

namespace LibDeflate.Buffers;

/// <summary>
/// A helper class that with utility methods for dealing with references, and other low-level details.
/// It also contains some APIs that act as polyfills for .NET Standard 2.0 and below.
/// </summary>
internal static class RuntimeHelpers
{
#if !NETCOREAPP3_1_OR_GREATER
    /// <summary>
    /// Gets the byte offset to the first <typeparamref name="T"/> element in a SZ array.
    /// </summary>
    /// <typeparam name="T">The type of values in the array.</typeparam>
    /// <returns>The byte offset to the first <typeparamref name="T"/> element in a SZ array.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntPtr GetArrayDataByteOffset<T>() => TypeInfo<T>.ArrayDataByteOffset;
#endif
#if !NETSTANDARD2_1_OR_GREATER
    /// <summary>
    /// Checks whether or not a given type is a reference type or contains references.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>Whether or not <typeparamref name="T"/> respects the <see langword="unmanaged"/> constraint.</returns>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReferenceOrContainsReferences<T>() => TypeInfo<T>.IsReferenceOrContainsReferences;

    /// <summary>
    /// Implements the logic for <see cref="IsReferenceOrContainsReferences{T}"/>.
    /// </summary>
    /// <param name="type">The current type to check.</param>
    /// <returns>Whether or not <paramref name="type"/> is a reference type or contains references.</returns>
    [Pure]
    private static bool IsReferenceOrContainsReferences(Type type)
    {
        // Common case, for primitive types
        if (type.IsPrimitive)
        {
            return false;
        }

        // Explicitly check for pointer types first
        if (type.IsPointer)
        {
            return false;
        }

        // Check for value types (this has to be after checking for pointers)
        if (!type.IsValueType)
        {
            return true;
        }

        // Check if the type is Nullable<T>
        if (Nullable.GetUnderlyingType(type) is Type nullableType)
        {
            type = nullableType;
        }

        if (type.IsEnum)
        {
            return false;
        }

        // Complex struct, recursively inspect all fields
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (IsReferenceOrContainsReferences(field.FieldType))
            {
                return true;
            }
        }

        return false;
    }
#endif

    /// <summary>
    /// A private generic class to preload type info for arbitrary runtime types.
    /// </summary>
    /// <typeparam name="T">The type to load info for.</typeparam>
    private static class TypeInfo<T>
    {
        /// <summary>
        /// The byte offset to the first <typeparamref name="T"/> element in a SZ array.
        /// </summary>
        public static readonly IntPtr ArrayDataByteOffset = MeasureArrayDataByteOffset();

#if !NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Indicates whether <typeparamref name="T"/> does not respect the <see langword="unmanaged"/> constraint.
        /// </summary>
        public static readonly bool IsReferenceOrContainsReferences = IsReferenceOrContainsReferences(typeof(T));
#endif

        /// <summary>
        /// Computes the value for <see cref="ArrayDataByteOffset"/>.
        /// </summary>
        /// <returns>The value of <see cref="ArrayDataByteOffset"/> for the current runtime.</returns>
        [Pure]
        private static IntPtr MeasureArrayDataByteOffset()
        {
            T[]? array = new T[1];

            return ObjectMarshal.DangerousGetObjectDataByteOffset(array, ref array[0]);
        }
    }
}
