using Rekkon.UmbraString.Tests.Assets;
using System.Text;

namespace Rekkon.UmbraString.Tests;

public abstract class BaseUmbraStringTests
{
    #region Standard tests

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

    [Test]
    public void TestSlice()
    {
        TestSliceViaSpan(CommonTestStrings.Length03, 0, 2);
        TestSliceViaSpan(CommonTestStrings.Length03, 1, 1);
        TestSliceViaSpan(CommonTestStrings.Length03, 2, 1);

        TestSliceViaSpan(CommonTestStrings.Length05, 0, 2);
        TestSliceViaSpan(CommonTestStrings.Length05, 1, 2);
        TestSliceViaSpan(CommonTestStrings.Length05, 2, 2);

        TestSliceViaSpan(CommonTestStrings.Length16, 0, 10);
        TestSliceViaSpan(CommonTestStrings.Length16, 1, 10);
        TestSliceViaSpan(CommonTestStrings.Length16, 5, 10);

        var length16 = CommonTestStrings.Length16;
        SpanString length64 = [.. length16, .. length16, .. length16, .. length16];

        TestSliceViaSpan(length64, 0, 20);
        TestSliceViaSpan(length64, 1, 20);
        TestSliceViaSpan(length64, 35, 20);
        TestSliceViaSpan(length64, 44, 20);
    }

    [Test]
    public void TestSliceTrivial()
    {
        PassThroughAll(TestTrivialSlice);
    }

    private void TestTrivialSlice(SpanString spanString)
    {
        TestSliceViaSpan(spanString, 0, spanString.Length);
        TestSliceViaSpan(spanString, 0, 0);

        if (spanString.Length is 0)
            return;

        TestSliceViaSpan(spanString, 1, spanString.Length - 1);
        TestSliceViaSpan(spanString, 0, spanString.Length - 1);
        TestSliceViaSpan(spanString, 0, spanString.Length - 1);
    }

    // TODO: Test bad slice operations

    // TODO: Test empty strings

    [Test]
    public void TestConcat()
    {
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length03, CommonTestStrings.Length01);
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length05, CommonTestStrings.Length07);
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length06, CommonTestStrings.Length08);
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length07, CommonTestStrings.Length09);
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length16, CommonTestStrings.Length09);

        var length16 = CommonTestStrings.Length16;
        SpanString length32 = [.. length16, .. length16];
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length09, length32);
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length16, length32);
    }

    [Test]
    public void TestConcatTrivial()
    {
        PassThroughAll(TestTrivialConcat);
    }

    private void TestTrivialConcat(SpanString spanString)
    {
        TestConcatViaSpan(spanString, spanString);
        TestConcatViaSpanBidirectionally(CommonTestStrings.Length01, spanString);
    }

    private void TestConcatViaSpanBidirectionally(SpanString a, SpanString b)
    {
        TestConcatViaSpan(a, b);
        TestConcatViaSpan(b, a);
    }

    protected abstract void TestEquality(SpanString spanString);
    protected abstract void TestEqualityViaSpan(SpanString spanString);
    protected abstract void TestLength(SpanString spanString);
    protected abstract void TestLengthViaSpan(SpanString spanString);
    protected abstract void TestSliceViaSpan(SpanString spanString, int offset, int length);
    protected abstract void TestConcatViaSpan(SpanString left, SpanString right);

    #endregion

    public static void PassThroughAll(SpanStringAction action)
    {
        action(CommonTestStrings.Length01);
        action(CommonTestStrings.Length02);
        action(CommonTestStrings.Length03);
        action(CommonTestStrings.Length04);
        action(CommonTestStrings.Length05);
        action(CommonTestStrings.Length06);
        action(CommonTestStrings.Length07);
        action(CommonTestStrings.Length08);
        action(CommonTestStrings.Length09);
        action(CommonTestStrings.Length10);
        action(CommonTestStrings.Length11);
        action(CommonTestStrings.Length12);
        action(CommonTestStrings.Length13);
        action(CommonTestStrings.Length14);
        action(CommonTestStrings.Length15);
        action(CommonTestStrings.Length16);

        var length16 = CommonTestStrings.Length16;
        SpanString length32 = [.. length16, .. length16];
        SpanString length64 = [.. length32, .. length32];
        action(length32);
        action(length64);
    }

    public static void PassThroughAll<T>(SpanStringFunc<T> func)
    {
        func(CommonTestStrings.Length01);
        func(CommonTestStrings.Length02);
        func(CommonTestStrings.Length03);
        func(CommonTestStrings.Length04);
        func(CommonTestStrings.Length05);
        func(CommonTestStrings.Length06);
        func(CommonTestStrings.Length07);
        func(CommonTestStrings.Length08);
        func(CommonTestStrings.Length09);
        func(CommonTestStrings.Length10);
        func(CommonTestStrings.Length11);
        func(CommonTestStrings.Length12);
        func(CommonTestStrings.Length13);
        func(CommonTestStrings.Length14);
        func(CommonTestStrings.Length15);
        func(CommonTestStrings.Length16);

        var length16 = CommonTestStrings.Length16;
        SpanString length32 = [.. length16, .. length16];
        SpanString length64 = [.. length32, .. length32];
        func(length32);
        func(length64);
    }

    protected static Func<string> ExceptionMessageGetter(SpanString spanString)
    {
        var value = GetUtf8String(spanString);
        return () => ExceptionMessage(value);
    }

    protected static string ExceptionMessage(SpanString spanString)
    {
        return ExceptionMessage(GetUtf8String(spanString));
    }

    protected static string ExceptionMessage(string value)
    {
        return $"The tested string was: '{value}'";
    }

    protected static string GetUtf8String(SpanString s)
    {
        return Encoding.UTF8.GetString(s);
    }
}
