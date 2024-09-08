using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Rekkon.UmbraString;

/// <summary>
/// Represents a variant of the Umbra-styled string,
/// also known as Umbra string, German-styled string or German string.
/// It stores the string as an array of bytes, without accounting for the
/// encoding.
/// This variant enables short strings to have a length of up to 15 bytes,
/// taking advantage of the unused 3 bytes of the length field.
/// This reduces the maximum supported length of a string to 3.5 GiB,
/// compared to the original of 4 GiB.
/// </summary>
/// <remarks>
/// This is an unsafe type. Use with caution.<br/>
/// When constructing instances of this type, make sure that the provided
/// references are pinned and will not move around. Once constructed, the
/// pointer is fixed and will always refer to that exact location.
/// It is best advised to use this type in short-lived operations.
/// This type is only supported on little endian architectures.
/// Use <see cref="UmbraStringV2"/> for big endian architectures.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public unsafe readonly struct UmbraStringV2
    : IUmbraString<UmbraStringV2>
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
     * which is <= 15 bytes in size. Parts of the _length field, and the whole
     * _prefix and _pointer fields will be treated as the actual contents of
     * the string, in sequential order:
     *
     *                           1 1 1 1 1
     * x 0 1 2   3 4 5 6   7 8 9 0 1 2 3 4
     * _ _ _ _ | _ _ _ _ | _ _ _ _ _ _ _ _
     * _length   _prefix   _pointer
     * 
     * x signifies the byte that contains the length of the short string, while
     * all the other bytes are the actual contents of the string itself.
     * 
     * The Endianness of the system plays a cardinal role in the layout of the
     * bytes in the _length field, and we thus implement this type on a per-
     * Endianness basis. This type specifically targets little-endian architectures.
     * 
     * For this implementation, we cheat on the _length field by always storing the
     * value in big endian ordering, to avoid confusing bit operations when the
     * stored string is a large string, and the field uses all its 4 bytes to
     * determine the length of the string.
     * 
     * The difference with the classic Umbra string is that this one eliminates the
     * need to attach a pointer for strings of 13~15 bytes in length, which can be
     * common for more fields.
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

    private const int _maxShortLength = 15;
    private const int _shortStringMask = 0xF0;
    private const uint _maxLength = 0xF000_0000u - 1;

    public static int MaxShortLength => _maxShortLength;
    public static uint MaxLength => _maxLength;

    private readonly int _length;
    private readonly uint _prefix;
    private readonly ulong _pointer;

    public bool IsShort => (_length & _shortStringMask) is _shortStringMask;

    public int Length
    {
        get
        {
            if (IsShort)
            {
                return _length & 0b1111;
            }
            return BinaryPrimitives.ReverseEndianness(_length);
        }
    }

    private UmbraStringV2(int length, uint prefix, ulong pointer)
    {
        _length = length;
        _prefix = prefix;
        _pointer = pointer;
    }

    public static UmbraStringV2 Construct(SpanString bytes)
    {
        if (!BitConverter.IsLittleEndian)
        {
            ThrowHelpers.ThrowUnsupportedBigEndian();
        }

        if (bytes.Length <= _maxShortLength)
        {
            return ConstructShort(bytes);
        }

        return ConstructLong(bytes);
    }

    private static UmbraStringV2 ConstructShort(SpanString bytes)
    {
        Debug.Assert(bytes.Length <= _maxShortLength);

        int length = bytes.Length;
        ulong leftBits = GetValueFromSpan<ulong>(bytes);
        leftBits <<= 8;
        leftBits |= (uint)(length | _shortStringMask);
        int lengthBytes = unchecked((int)(leftBits & 0xFFFFFFFF));
        var prefix = (uint)(leftBits >> 32);

        ulong pointer = 0;
        const int pointerOffset = 7;
        if (length > pointerOffset)
        {
            var pointerSlice = bytes[pointerOffset..];
            pointer = GetValueFromSpan<ulong>(pointerSlice);
        }

        return new(lengthBytes, prefix, pointer);
    }
    private static UmbraStringV2 ConstructLong(SpanString bytes)
    {
        Debug.Assert(bytes.Length > _maxShortLength);

        // We don't need to check the length exceedance here, since
        // a span cannot exceed 2^31 - 1 bytes
        int length = bytes.Length;

        // Use big endian ordering for the length, to comply with the compact
        // representation of the length of the short string, which also contains
        // 3 bytes of its content
        length = BinaryPrimitives.ReverseEndianness(length);
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
        return MemoryMarshal.CreateSpan(ref pointerRef, Length);
    }

    private SpanString GetUnsafeSpanShort()
    {
        ref var lengthReference = ref Unsafe.AsRef(in _length);
        ref var bytePrefix = ref Unsafe.As<int, byte>(ref lengthReference);
        bytePrefix = ref Unsafe.AddByteOffset(ref bytePrefix, 1);
        return MemoryMarshal.CreateReadOnlySpan(ref bytePrefix, Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong QuickLengthPrefix()
    {
        ref var lengthReference = ref Unsafe.AsRef(in _length);
        return Unsafe.As<int, ulong>(ref lengthReference);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UmbraStringV2 other)
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

    private bool EqualsSlow(UmbraStringV2 other)
    {
        // Now we will have to walk down the entire strings and compare their
        // actual contents

        var thisSpan = GetUnsafeSpanLong();
        var otherSpan = other.GetUnsafeSpanLong();

        // At this point we have ensured that the strings have the same length
        Debug.Assert(thisSpan.Length == otherSpan.Length);

        return thisSpan.SequenceEqual(otherSpan);
    }

    public UmbraStringV2 Concat(UmbraStringV2 other, Span<byte> newBuffer)
    {
        int thisLength = Length;
        int otherLength = other.Length;

        int resultLength = thisLength + otherLength;

        if (resultLength > _maxShortLength)
        {
            return UmbraStringHelpers.ConcatLong(this, other, newBuffer);
        }

        // If we can fit the new content in a short string, we also have short strings
        Debug.Assert(IsShort);
        Debug.Assert(other.IsShort);

        Span<byte> resultSpan = stackalloc byte[sizeof(UmbraStringV2)];

        // Set the length
        resultSpan[0] = (byte)(resultLength | 0xF0);

        // Copy the contents directly
        var contentSpan = resultSpan[1..];
        var thisSpan = GetUnsafeSpanShort();
        var otherSpan = other.GetUnsafeSpanShort();
        thisSpan.CopyTo(contentSpan);
        otherSpan.CopyTo(contentSpan[thisLength..]);
        ref var result = ref MemoryMarshal.AsRef<UmbraStringV2>(resultSpan);
        return result;
    }

    public UmbraStringV2 Slice(int start, int length)
    {
        if (length == 0)
            return default;

        int sourceLength = Length;
        if (start < 0 || start >= sourceLength)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException(nameof(start),
                "The start index falls out of range of the string.");
        }

        if (length < 0)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException(nameof(length),
                "The length cannot be negative.");
        }

        if (start + length > sourceLength)
        {
            ThrowHelpers.ThrowArgumentOutOfRangeException(nameof(length),
                "The specified range falls out of the range of the string.");
        }

        if (length == sourceLength)
            return this;

        if (IsShort)
        {
            return SliceShort(start, length);
        }

        return SliceLong(start, length);
    }

    private UmbraStringV2 SliceShort(int start, int length)
    {
        Span<byte> resultSpan = stackalloc byte[sizeof(UmbraStringV2)];

        // Set the length
        resultSpan[0] = (byte)(length | 0xF0);

        // Copy the contents directly
        var contentSpan = resultSpan[1..];
        var thisSpan = GetUnsafeSpanShort().Slice(start, length);
        thisSpan.CopyTo(contentSpan);
        ref var result = ref MemoryMarshal.AsRef<UmbraStringV2>(resultSpan);
        return result;
    }

    private UmbraStringV2 SliceLong(int start, int length)
    {
        // Always try to reduce from long to short
        if (length <= _maxShortLength)
        {
            var span = GetUnsafeSpanLong().Slice(start, length);
            return ConstructShort(span);
        }

        uint prefix = default;
        ulong pointer = _pointer + (ulong)start;
        if (start is 0)
        {
            prefix = _prefix;
        }
        else
        {
            var span = GetUnsafeSpanLong();
            var prefixSpan = span.Slice(start, sizeof(uint));
            prefix = MemoryMarshal.AsRef<uint>(prefixSpan);
        }

        var lengthBytes = BinaryPrimitives.ReverseEndianness(length);
        return new(lengthBytes, prefix, pointer);
    }

    public byte* GetContentPointerUnsafe()
    {
        if (IsShort)
            return null;

        return (byte*)_pointer;
    }

    public static bool operator ==(UmbraStringV2 left, UmbraStringV2 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(UmbraStringV2 left, UmbraStringV2 right)
    {
        return !left.Equals(right);
    }

    public override bool Equals(object? obj)
    {
        return obj is UmbraStringV2 other && Equals(other);
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
