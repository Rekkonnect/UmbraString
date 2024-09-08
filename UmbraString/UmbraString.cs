using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Rekkon.UmbraString;

/// <summary>
/// Represents an Umbra-styled string, also known as Umbra string,
/// German-styled string or German string.
/// It stores the string as an array of bytes, without accounting for the
/// encoding.
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
    : IUmbraString<UmbraString>
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

    public static int MaxShortLength => _maxShortLength;
    public static uint MaxLength => uint.MaxValue;

    private readonly int _length;
    private readonly uint _prefix;
    private readonly ulong _pointer;

    public bool IsShort => _length <= _maxShortLength;

    public int Length => _length;

    private UmbraString(int length, uint prefix, ulong pointer)
    {
        _length = length;
        _prefix = prefix;
        _pointer = pointer;
    }

    public static UmbraString Construct(SpanString bytes)
    {
        if (bytes.Length <= _maxShortLength)
        {
            return ConstructShort(bytes);
        }

        return ConstructLong(bytes);
    }

    private static UmbraString ConstructShort(SpanString bytes)
    {
        Debug.Assert(bytes.Length <= _maxShortLength);

        int length = bytes.Length;
        uint prefix = GetPrefix(bytes);

        ulong pointer = 0;
        if (length > sizeof(int))
        {
            var pointerSlice = bytes[sizeof(int)..];
            pointer = GetValueFromSpan<ulong>(pointerSlice);
        }

        return new(length, prefix, pointer);
    }
    private static UmbraString ConstructLong(SpanString bytes)
    {
        Debug.Assert(bytes.Length > _maxShortLength);

        int length = bytes.Length;
        uint prefix = GetPrefix(bytes);

        ref readonly var bytesRefReadOnly = ref MemoryMarshal.AsRef<byte>(bytes);
        ref var bytesRef = ref Unsafe.AsRef(in bytesRefReadOnly);
        void* ptr = Unsafe.AsPointer(ref bytesRef);
        ulong pointer = (ulong)ptr;

        return new(length, prefix, pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetPrefix(SpanString bytes)
    {
        return GetValueFromSpan<uint>(bytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetValueFromSpan<T>(SpanString bytes)
        where T : unmanaged
    {
        T value = default;
        ref var valueBytes = ref Unsafe.As<T, byte>(ref value);
        var valueSpan = MemoryMarshal.CreateSpan(ref valueBytes, sizeof(T));
        int length = Math.Min(bytes.Length, sizeof(T));
        var prefixSlice = bytes[..length];
        prefixSlice.CopyTo(valueSpan);
        return value;
    }

    public SpanString GetUnsafeSpan()
    {
        if (IsShort)
        {
            return GetUnsafeSpanShort();
        }

        return GetUnsafeSpanLong();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private SpanString GetUnsafeSpanLong()
    {
        var ptr = (byte*)_pointer;
        ref byte pointerRef = ref Unsafe.AsRef<byte>(ptr);
        return MemoryMarshal.CreateSpan(ref pointerRef, _length);
    }

    private SpanString GetUnsafeSpanShort()
    {
        ref var prefixReference = ref Unsafe.AsRef(in _prefix);
        ref var bytePrefix = ref Unsafe.As<uint, byte>(ref prefixReference);
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
        return _length ^ unchecked((int)_prefix);
    }

    /// <summary>
    /// Creates a string out of the contents of this string.
    /// Uses the <see cref="Encoding.UTF8"/> encoding. To use
    /// another encoding, use the <see cref="ToString(Encoding)"/>
    /// method.
    /// </summary>
    /// <returns>
    /// The string representation of this string using the
    /// <see cref="Encoding.UTF8"/> encoding.
    /// </returns>
    public override string ToString()
    {
        return ToString(Encoding.UTF8);
    }

    public string ToString(Encoding encoding)
    {
        var span = GetUnsafeSpan();
        return encoding.GetString(span);
    }
}
