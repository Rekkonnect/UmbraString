namespace Rekkon.UmbraString.Tests;

public abstract class BaseUmbraStringTests<TUmbraString>
    : BaseUmbraStringTests
    where TUmbraString : IUmbraString<TUmbraString>
{
    protected sealed override void TestEquality(SpanString spanString)
    {
        var umbraString = TUmbraString.Construct(spanString);
        var umbraString2 = TUmbraString.Construct(spanString);
        Assert.That(umbraString, Is.EqualTo(umbraString2));
    }

    protected sealed override void TestEqualityViaSpan(SpanString spanString)
    {
        var umbraString = TUmbraString.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        Assert.That(returnedSpan.SequenceEqual(spanString), Is.True);
    }

    protected sealed override void TestLength(SpanString spanString)
    {
        var umbraString = TUmbraString.Construct(spanString);
        var length = umbraString.Length;
        Assert.That(length, Is.EqualTo(spanString.Length), ExceptionMessageGetter(spanString));
    }

    protected sealed override void TestLengthViaSpan(SpanString spanString)
    {
        var umbraString = TUmbraString.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        var length = returnedSpan.Length;
        Assert.That(length, Is.EqualTo(spanString.Length), ExceptionMessageGetter(spanString));
    }

    protected sealed override void TestSliceViaSpan(SpanString spanString, int offset, int length)
    {
        var umbraString = TUmbraString.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        var spanSlice = returnedSpan.Slice(offset, length);
        var umbraSlice = umbraString.Slice(offset, length);
        var umbraSliceSpan = umbraSlice.GetUnsafeSpan();
        Assert.That(spanSlice.SequenceEqual(umbraSliceSpan), Is.True, ExceptionMessageGetter(spanString));
    }

    protected sealed override void TestConcatViaSpan(
        SpanString left, SpanString right)
    {
        int targetLength = left.Length + right.Length;
        Span<byte> newBuffer = stackalloc byte[targetLength];

        var leftUmbra = TUmbraString.Construct(left);
        var rightUmbra = TUmbraString.Construct(right);

        var concatUmbra = leftUmbra.Concat(rightUmbra, newBuffer);
        SpanString concatSpan = [.. left, .. right];

        var concatUmbraSpan = concatUmbra.GetUnsafeSpan();
        Assert.That(concatUmbraSpan.SequenceEqual(concatSpan), Is.True, ExceptionMessageGetter(concatSpan));
    }
}
