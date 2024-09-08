using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Rekkon.UmbraString.Benchmarks;

#if false

So far good:

| Method              | Mean     | Error     | StdDev    |
|-------------------- |---------:|----------:|----------:|
| EqualsClassicPrefix | 6.555 ns | 0.0852 ns | 0.0797 ns |
| EqualsUmbraPrefix   | 4.578 ns | 0.0445 ns | 0.0348 ns |
| EqualsClassicShort  | 6.262 ns | 0.0730 ns | 0.0682 ns |
| EqualsUmbraShort    | 4.572 ns | 0.0301 ns | 0.0282 ns |

#endif

[IterationTime(250)]
public class UmbraStringVsClassicEquality
{
    public static SpanString PrefixStringA => "1"u8;
    public static SpanString ShortStringA => "123456"u8;
    public static SpanString MaxShortStringA => "123456789012"u8;
    public static SpanString LongStringA => "123456789012345"u8;

    public static SpanString PrefixStringB => "2"u8;
    public static SpanString ShortStringB => "223456"u8;
    public static SpanString ShortStringC => "123455"u8;
    public static SpanString MaxShortStringB => "223456789012"u8;
    public static SpanString MaxShortStringC => "123456779012"u8;
    public static SpanString LongStringB => "223456789012345"u8;
    public static SpanString LongStringC => "123456789012355"u8;

    public UmbraString UmbraPrefixStringA = UmbraString.Construct(PrefixStringA);
    public UmbraString UmbraShortStringA = UmbraString.Construct(ShortStringA);
    public UmbraString UmbraMaxShortStringA = UmbraString.Construct(MaxShortStringA);
    public UmbraString UmbraLongStringA = UmbraString.Construct(LongStringA);

    public UmbraString UmbraPrefixStringB = UmbraString.Construct(PrefixStringB);
    public UmbraString UmbraShortStringB = UmbraString.Construct(ShortStringB);
    public UmbraString UmbraShortStringC = UmbraString.Construct(ShortStringC);
    public UmbraString UmbraMaxShortStringB = UmbraString.Construct(MaxShortStringB);
    public UmbraString UmbraMaxShortStringC = UmbraString.Construct(MaxShortStringC);
    public UmbraString UmbraLongStringB = UmbraString.Construct(LongStringB);
    public UmbraString UmbraLongStringC = UmbraString.Construct(LongStringC);

#pragma warning disable CA1822 // Mark members as static -- benchmarks

    [Benchmark]
    public bool EqualsClassicPrefix()
    {
        return Equals(PrefixStringA, PrefixStringA);
    }

    [Benchmark]
    public bool EqualsUmbraPrefix()
    {
        return Equals(UmbraPrefixStringA, UmbraPrefixStringA);
    }

    [Benchmark]
    public bool EqualsClassicShort()
    {
        return Equals(ShortStringA, ShortStringA);
    }

    [Benchmark]
    public bool EqualsUmbraShort()
    {
        return Equals(UmbraShortStringA, UmbraShortStringA);
    }

#pragma warning restore CA1822 // Mark members as static

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Equals(SpanString left, SpanString right)
    {
        return left.SequenceEqual(right);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool Equals(UmbraString left, UmbraString right)
    {
        return left.Equals(right);
    }
}
