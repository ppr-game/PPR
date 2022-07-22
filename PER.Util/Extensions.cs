using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
public static class RandomExtensions {
    public static float NextSingle(this Random rng, float min, float max) => rng.NextSingle() * (max - min) + min;
}

[PublicAPI]
public static class EnumerableExtensions {
    public static T ElementAtOrDefault<T>(this IList<T> list, int index, Func<T> @default) =>
        index >= 0 && index < list.Count ? list[index] : @default();
}

[PublicAPI]
public static class StringExtensions {
    public static string AddSpaces(this string text) {
        if(string.IsNullOrWhiteSpace(text))
            return "";
        StringBuilder newText = new(text.Length * 2);
        _ = newText.Append(text[0]);
        for(int i = 1; i < text.Length; i++) {
            if(char.IsUpper(text[i]) && text[i - 1] != ' ')
                _ = newText.Append(' ');
            _ = newText.Append(text[i]);
        }
        return newText.ToString();
    }
}

[PublicAPI]
public static class ConversionExtensions {
    // apparently these get inlined automatically but
    // i'm gonna leave the attributes here just in case
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool ToBool(this byte value) => *(bool*)&value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe byte ToByte(this bool value) => *(byte*)&value;
}
