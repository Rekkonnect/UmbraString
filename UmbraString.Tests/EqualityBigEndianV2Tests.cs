namespace Rekkon.UmbraString.Tests;

public class EqualityBigEndianV2Tests : UmbraStringTests
{
    private const string _ignoreMessage = "This platform does not support big endian.";
    private readonly bool _canBeRun = !BitConverter.IsLittleEndian;

    [Test]
    public void TestEquality()
    {
        PassThroughAll(TestEquality, _canBeRun, _ignoreMessage);
    }

    [Test]
    public void TestEqualityViaSpan()
    {
        PassThroughAll(TestEqualityViaSpan, _canBeRun, _ignoreMessage);
    }

    [Test]
    public void TestLength()
    {
        PassThroughAll(TestLength, _canBeRun, _ignoreMessage);
    }

    [Test]
    public void TestLengthViaSpan()
    {
        PassThroughAll(TestLengthViaSpan, _canBeRun, _ignoreMessage);
    }

#pragma warning disable CS0618 // Type or member is obsolete
// The type is marked as obsolete since the tests have not been run

    private static void TestEquality(SpanString spanString)
    {
        var umbraString = BigEndianUmbraStringV2.Construct(spanString);
        var umbraString2 = BigEndianUmbraStringV2.Construct(spanString);
        Assert.That(umbraString, Is.EqualTo(umbraString2), ExceptionMessageGetter(spanString));
    }

    private static void TestEqualityViaSpan(SpanString spanString)
    {
        var umbraString = BigEndianUmbraStringV2.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        Assert.That(returnedSpan.SequenceEqual(spanString), Is.True, ExceptionMessageGetter(spanString));
    }

    private static void TestLength(SpanString spanString)
    {
        var umbraString = BigEndianUmbraStringV2.Construct(spanString);
        var length = umbraString.Length;
        Assert.That(length, Is.EqualTo(spanString.Length), ExceptionMessageGetter(spanString));
    }

    private static void TestLengthViaSpan(SpanString spanString)
    {
        var umbraString = BigEndianUmbraStringV2.Construct(spanString);
        var returnedSpan = umbraString.GetUnsafeSpan();
        var length = returnedSpan.Length;
        Assert.That(length, Is.EqualTo(spanString.Length), ExceptionMessageGetter(spanString));
    }

#pragma warning restore CS0618 // Type or member is obsolete
}
