namespace Rekkon.UmbraString;

/// <summary>
/// Provides extensions for <see cref="IUmbraString{TSelf}"/> instances.
/// Too complex string operations are preferred to be handled via the
/// span returned from <see cref="IUmbraString{TSelf}.GetUnsafeSpan"/>.
/// </summary>
public static class IUmbraStringExtensions
{
    public static void CopyTo<TSelf>(this TSelf self, Span<byte> other)
        where TSelf : IUmbraString<TSelf>
    {
        var span = self.GetUnsafeSpan();
        span.CopyTo(other);
    }

    public static TSelf Slice<TSelf>(this TSelf self, Range range)
        where TSelf : IUmbraString<TSelf>
    {
        int stringLength = self.Length;
        var (start, length) = range.GetOffsetAndLength(stringLength);
        return self.Slice(start, length);
    }

    public static TSelf SliceUntil<TSelf>(this TSelf self, int length)
        where TSelf : IUmbraString<TSelf>
    {
        return self.Slice(0, length);
    }

    public static TSelf SliceAfter<TSelf>(this TSelf self, int offset)
        where TSelf : IUmbraString<TSelf>
    {
        int length = self.Length;
        return self.Slice(offset, length - offset);
    }
}
