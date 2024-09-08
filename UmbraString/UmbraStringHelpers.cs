using System.Runtime.CompilerServices;

namespace Rekkon.UmbraString;

public static unsafe class UmbraStringHelpers
{
    public static bool FitsShort<TSelf>(SpanString span)
        where TSelf : IUmbraString<TSelf>
    {
        return span.Length <= TSelf.MaxShortLength;
    }

    public static TSelf ConstructLong<TSelf>(SpanString span, Span<byte> outBuffer)
        where TSelf : IUmbraString<TSelf>
    {
        span.CopyTo(outBuffer);
        return TSelf.Construct(outBuffer);
    }

    // Using ref readonly here completely breaks some cases
    // For example, concatenating "1" and "123" will only copy "1" from "123"
    // It's probably not critical to use this for the performance
    // Investigate this as a potential JIT bug
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TSelf ConcatLong<TSelf>(
        TSelf left, TSelf right, Span<byte> newBuffer)
        where TSelf : IUmbraString<TSelf>
    {
        var leftSpan = left.GetUnsafeSpan();
        var rightSpan = right.GetUnsafeSpan();
        int leftLength = left.Length;
        leftSpan.CopyTo(newBuffer);
        var offsetNewBuffer = newBuffer.Slice(leftLength);
        rightSpan.CopyTo(offsetNewBuffer);
        int resultLength = leftLength + right.Length;
        return TSelf.Construct(newBuffer[..resultLength]);
    }
}
