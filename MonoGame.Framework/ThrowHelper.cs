using System;
using System.Diagnostics.CodeAnalysis;

namespace MonoGame.Framework;

internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void InvalidOperation(string? message)
    {
        throw new InvalidOperationException(message);
    }

    [DoesNotReturn]
    internal static void Argument(string? message, string? paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    internal static void ArgumentEmpty(string? message, string? paramName)
    {
        throw new ArgumentEmptyException(message, paramName);
    }

    internal static void ArgumentOutOfRange(string? message, string? paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, message);
    }

    internal static void Argument(string? message) => Argument(message, null);

    internal static void NotSupported(string? message)
    {
        throw new NotSupportedException(message);
    }

    internal static void OffThreadNotSupported(string? message)
    {
        throw new OffThreadNotSupportedException(message);
    }

    internal static void Timeout()
    {
        throw new TimeoutException();
    }
}
