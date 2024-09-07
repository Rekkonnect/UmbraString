namespace Rekkon.UmbraString.Tests;

public class ConstructionTests
{
    public static ReadOnlySpan<byte> PrefixStringA => "1"u8;
    public static ReadOnlySpan<byte> ShortStringA => "123456"u8;
    public static ReadOnlySpan<byte> MaxShortStringA => "123456789012"u8;
    public static ReadOnlySpan<byte> LongStringA => "123456789012345"u8;

    [Test]
    public void TestLength1()
    {
        UmbraString.Construct(PrefixStringA);
    }

    [Test]
    public void TestLength6()
    {
        UmbraString.Construct(ShortStringA);
    }

    [Test]
    public void TestLength12()
    {
        UmbraString.Construct(MaxShortStringA);
    }

    [Test]
    public void TestLongString()
    {
        UmbraString.Construct(LongStringA);
    }
}