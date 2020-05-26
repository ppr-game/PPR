using System;
using System.Collections.Generic;

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
}
