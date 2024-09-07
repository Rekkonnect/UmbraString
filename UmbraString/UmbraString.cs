using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Rekkon.UmbraString;

/// <summary>
/// Represents an Umbra-styled string, also known as Umbra string,
/// German-styled string or German string. It stores the string as a
/// UTF-8 string.
/// </summary>
/// <remarks>
/// This is an unsafe type. Use with caution.<br/>
/// When constructing instances of this type, make sure that the provided
/// references are pinned and will not move around. Once constructed, the
/// pointer is fixed and will always refer to that exact location.
/// It is best advised to use this type in short-lived operations.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct UmbraString
    : IEquatable<UmbraString>,
        IEqualityOperators<UmbraString, UmbraString, bool>
{
    /*
     * Implementation details:
     * 
     * - _length stores the 4-byte length of the string (up to 4 GiB)
     * - _prefix stores the 4-byte prefix of the string
     * - _pointer stores the 8-byte pointer to the underlying byte array
     *      storing the entire string
     * 
     * The above layout will be adjusted slightly when we have a short string
     * which is <= 12 bytes in size. The _prefix and _pointer fields will be
     * treated as the actual contents of the string, in sequential order:
     *
     *                       1 1
     * 0 1 2 3   4 5 6 7 8 9 0 1
     * _ _ _ _ | _ _ _ _ _ _ _ _
     * _prefix   _pointer
     * 
     * The Endianness of the system does not affect our values, and we are thus
     * not accounting for it when reinterpreting the ROS<byte> into a ROS<int>.
     * 
     * We avoid using FieldOffset attributes when unionizing the _prefix and _pointer
     * fields for the sake of comparing and setting their contents.
     * 
     * We avoid using SkipLocalsInit because we might encounter a low-length
     * string (< 4 bytes), which will not occupy the entire _prefix field,
     * and thus encounter bugs in equality operations due to non-deterministic
     * memory state.
     * 
     * References:
     * - https://cedardb.com/blog/german_strings/
     * - https://tunglevo.com/note/an-optimization-thats-impossible-in-rust/
     */

    private const int _maxShortLength = 12;

    private readonly int _length;
    private readonly int _prefix;
    private readonly ulong _pointer;

    public bool IsShort => _length <= _maxShortLength;

    private UmbraString(int length, int prefix, ulong pointer)
    {
        _length = length;
        _prefix = prefix;
        _pointer = pointer;
    }

    public static UmbraString Construct(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length <= _maxShortLength)
        {
            return ConstructShort(bytes);
        }

        return ConstructLong(bytes);
    }

    private static UmbraString ConstructShort(ReadOnlySpan<byte> bytes)
    {
        Debug.Assert(bytes.Length <= _maxShortLength);

        int length = bytes.Length;
        int prefix = GetPrefix(bytes);

        ulong pointer = 0;
        if (length > sizeof(int))
        {
            var pointerSlice = bytes[sizeof(int)..];
            pointer = GetValueFromSpan<ulong>(pointerSlice);
        }

        return new(length, prefix, pointer);
    }
    private static UmbraString ConstructLong(ReadOnlySpan<byte> bytes)
    {
        Debug.Assert(bytes.Length > _maxShortLength);

        int length = bytes.Length;
        int prefix = GetPrefix(bytes);

        ref readonly var bytesRefReadOnly = ref MemoryMarshal.AsRef<byte>(bytes);
        ref var bytesRef = ref Unsafe.AsRef(in bytesRefReadOnly);
        void* ptr = Unsafe.AsPointer(ref bytesRef);
        ulong pointer = (ulong)ptr;

        return new(length, prefix, pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetPrefix(ReadOnlySpan<byte> bytes)
    {
        return GetValueFromSpan<int>(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetValueFromSpan<T>(ReadOnlySpan<byte> bytes)
        where T : unmanaged
    {
        Span<byte> prefixSpan = stackalloc byte[sizeof(T)];
        int length = Math.Min(bytes.Length, sizeof(T));
        var prefixSlice = bytes[..length];
        prefixSlice.CopyTo(prefixSpan);
        T prefix = MemoryMarshal.Cast<byte, T>(prefixSpan)[0];
        return prefix;
    }

    public ReadOnlySpan<byte> GetUnsafeSpan()
    {
        if (!IsShort)
        {
            return GetUnsafeSpanShort();
        }

        return GetUnsafeSpanLong();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ReadOnlySpan<byte> GetUnsafeSpanLong()
    {
        var ptr = (byte*)_pointer;
        ref byte pointerRef = ref Unsafe.AsRef<byte>(ptr);
        return MemoryMarshal.CreateSpan(ref pointerRef, _length);
    }

    private ReadOnlySpan<byte> GetUnsafeSpanShort()
    {
        ref var prefixReference = ref Unsafe.AsRef(in _prefix);
        ref var bytePrefix = ref Unsafe.As<int, byte>(ref prefixReference);
        return MemoryMarshal.CreateReadOnlySpan(ref bytePrefix, _length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong QuickLengthPrefix()
    {
        ref var lengthReference = ref Unsafe.AsRef(in _length);
        return Unsafe.As<int, ulong>(ref lengthReference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UmbraString other)
    {
        ulong quickThis = QuickLengthPrefix();
        ulong quickOther = other.QuickLengthPrefix();
        if (quickThis != quickOther)
            return false;

        // We assume that the same pointer returns the same string at this point
        if (_pointer == other._pointer)
            return true;

        if (IsShort)
        {
            // If we have a short string with the same length, we have already
            // compared its actual contents via the _pointer field
            return false;
        }
        return EqualsSlow(other);
    }

    private bool EqualsSlow(UmbraString other)
    {
        // Now we will have to walk down the entire strings and compare their
        // actual contents

        var thisSpan = GetUnsafeSpanLong();
        var otherSpan = other.GetUnsafeSpanLong();

        // At this point we have ensured that the strings have the same length
        Debug.Assert(thisSpan.Length == otherSpan.Length);

        return thisSpan.SequenceEqual(otherSpan);
    }

    public static bool operator ==(UmbraString left, UmbraString right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UmbraString left, UmbraString right)
    {
        return !left.Equals(right);
    }

    public override bool Equals(object? obj)
    {
        return obj is UmbraString other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _length ^ _prefix ^ _pointer.GetHashCode();
    }
}
