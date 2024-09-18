using System;

public static class NullExtensions {
    public static T ThrowIfNull<T>(this T? argument) where T : struct {
        if (argument is null)
            throw new ArgumentNullException();

        return (T)argument;
    }
}