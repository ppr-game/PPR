using System;
using System.Collections.Generic;
using System.Text;

namespace PPR.Main {
    public static class Random_Extensions {
        public static float NextFloat(this Random rng, float min, float max) {
            return (float)rng.NextDouble() * (max - min) + min;
        }
    }
    public static class IEnumerable_Extensions {
        public static T ElementAtOrDefault<T>(this IList<T> list, int index, Func<T> @default) {
            return index >= 0 && index < list.Count ? list[index] : @default();
        }
    }
    public static class String_Extensions {
        public static string AddSpaces(this string text) {
            if(string.IsNullOrWhiteSpace(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            _ = newText.Append(text[0]);
            for(int i = 1; i < text.Length; i++) {
                if(char.IsUpper(text[i]) && text[i - 1] != ' ')
                    _ = newText.Append(' ');
                _ = newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
}
