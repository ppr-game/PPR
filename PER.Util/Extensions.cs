using System;
using System.Collections.Generic;
using System.Text;

namespace PER.Util;

public static class RandomExtensions {
    public static float NextSingle(this Random rng, float min, float max) => rng.NextSingle() * (max - min) + min;
}

public static class EnumerableExtensions {
    public static T ElementAtOrDefault<T>(this IList<T> list, int index, Func<T> @default) =>
        index >= 0 && index < list.Count ? list[index] : @default();
}

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
