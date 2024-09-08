using BenchmarkDotNet.Attributes;
using Rekkon.UmbraString.Tests.Assets;

namespace Rekkon.UmbraString.Benchmarks;

#if false

Example results:

| Method            | Mean      | Error     | StdDev    |
|------------------ |----------:|----------:|----------:|
| ConstructLength01 | 14.630 ns | 0.1509 ns | 0.1260 ns |
| ConstructLength02 | 14.652 ns | 0.2701 ns | 0.2255 ns |
| ConstructLength03 | 14.473 ns | 0.0936 ns | 0.0781 ns |
| ConstructLength04 | 14.797 ns | 0.1409 ns | 0.1249 ns |
| ConstructLength05 | 14.624 ns | 0.0378 ns | 0.0316 ns |
| ConstructLength06 | 14.704 ns | 0.0369 ns | 0.0288 ns |
| ConstructLength07 | 14.241 ns | 0.0722 ns | 0.0640 ns |
| ConstructLength08 | 16.270 ns | 0.1360 ns | 0.1205 ns |
| ConstructLength09 | 16.778 ns | 0.1149 ns | 0.0960 ns |
| ConstructLength10 | 16.798 ns | 0.2962 ns | 0.2626 ns |
| ConstructLength11 | 16.413 ns | 0.0910 ns | 0.0760 ns |
| ConstructLength12 | 16.609 ns | 0.1313 ns | 0.1228 ns |
| ConstructLength13 | 16.766 ns | 0.3205 ns | 0.2998 ns |
| ConstructLength14 | 16.556 ns | 0.0902 ns | 0.0799 ns |
| ConstructLength15 | 12.550 ns | 0.1154 ns | 0.1079 ns |
| ConstructLength16 |  8.764 ns | 0.0495 ns | 0.0439 ns |

#endif

[IterationTime(250)]
public class UmbraStringV2Construction
{
#pragma warning disable CA1822 // Mark members as static -- Benchmarks

    [Benchmark]
    public UmbraStringV2 ConstructLength01()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length01);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength02()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length02);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength03()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length03);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength04()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length04);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength05()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length05);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength06()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length06);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength07()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length07);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength08()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length08);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength09()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length09);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength10()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length10);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength11()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length11);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength12()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length12);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength13()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length13);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength14()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length14);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength15()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length15);
    }
    [Benchmark]
    public UmbraStringV2 ConstructLength16()
    {
        return UmbraStringV2.Construct(CommonTestStrings.Length16);
    }

#pragma warning restore CA1822 // Mark members as static
}
