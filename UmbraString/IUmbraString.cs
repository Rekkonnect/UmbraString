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

    public SpanString GetUnsafeSpan();
    public string ToString(Encoding encoding);

    public static abstract TSelf Construct(SpanString bytes);
}
