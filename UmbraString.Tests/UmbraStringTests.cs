using Rekkon.UmbraString.Tests.Assets;
using System.Text;

namespace Rekkon.UmbraString.Tests;

public abstract class UmbraStringTests
{
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
    }

    public static void PassThroughAll(SpanStringAction action, bool condition, string? ignoreMessage)
    {
        if (!condition)
        {
            Assert.Ignore(ignoreMessage);
        }

        PassThroughAll(action);
    }

    public static void PassThroughAll<T>(SpanStringFunc<T> func, bool condition, string? ignoreMessage)
    {
        if (!condition)
        {
            Assert.Ignore(ignoreMessage);
        }

        PassThroughAll(func);
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
