namespace Rekkon.UmbraString.Tests;

public class EqualityTests : UmbraStringTests
{
    [Test]
    public void TestEquality()
    {
        PassThroughAll(TestEquality);
    }

    [Test]
    public void TestEqualityViaSpan()
    {
        PassThroughAll(TestEqualityViaSpan);
    }

    [Test]
    public void TestLength()
    {
        PassThroughAll(TestLength);
    }

    [Test]
    public void TestLengthViaSpan()
    {
        PassThroughAll(TestLengthViaSpan);
    }

    private static void TestEquality(SpanString spanString)
    {
        var umbraString = UmbraString.Construct(spanString);
        var umbraString2 = UmbraString.Construct(spanString);
        Assert.That(umbraString, Is.EqualTo(umbraString2));
    }

    private static void TestEqualityViaSpan(SpanString spanString)
    {
        var umbraString = UmbraString.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        Assert.That(returnedSpan.SequenceEqual(spanString), Is.True);
    }

    private static void TestLength(SpanString spanString)
    {
        var umbraString = UmbraString.Construct(spanString);
        var length = umbraString.Length;
        Assert.That(length, Is.EqualTo(spanString.Length), ExceptionMessageGetter(spanString));
    }

    private static void TestLengthViaSpan(SpanString spanString)
    {
        var umbraString = UmbraString.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        var length = returnedSpan.Length;
        Assert.That(length, Is.EqualTo(spanString.Length), ExceptionMessageGetter(spanString));
    }
}
