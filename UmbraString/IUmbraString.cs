using System.Numerics;
using System.Text;

namespace Rekkon.UmbraString;

/// <summary>
/// Represents a variant of the Umbra-styled string,
/// also known as Umbra string, German-styled string or German string.
/// </summary>
public interface IUmbraString<TSelf>
    : IEquatable<TSelf>,
        IEqualityOperators<TSelf, TSelf, bool>
    where TSelf : IUmbraString<TSelf>
{
    public static abstract int MaxShortLength { get; }
    public static abstract uint MaxLength { get; }

    public bool IsShort { get; }
    public int Length { get; }

    public TSelf Concat(TSelf other, Span<byte> newBuffer);
    public TSelf Slice(int start, int length);

    public unsafe byte* GetContentPointerUnsafe();

    public SpanString GetUnsafeSpan();
    public string ToString(Encoding encoding);

    public static abstract TSelf Construct(SpanString bytes);

    public static bool FitsShort(SpanString span)
    {
        return UmbraStringHelpers.FitsShort<TSelf>(span);
    }

    public static TSelf ConstructLong(SpanString span, Span<byte> outBuffer)
    {
        return UmbraStringHelpers.ConstructLong<TSelf>(span, outBuffer);
    }
}
