using System;

namespace PER.Util {
    public readonly struct Bounds : IEquatable<Bounds> {
        public readonly Vector2Int min;
        public readonly Vector2Int max;
        
        public Bounds(Vector2Int min, Vector2Int max) {
            this.min = min;
            this.max = max;
        }

        public bool Equals(Bounds other) => min.Equals(other.min) && max.Equals(other.max);

        public override bool Equals(object obj) => obj is Bounds other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(min, max);

        public static bool operator ==(Bounds left, Bounds right) => left.Equals(right);
        public static bool operator !=(Bounds left, Bounds right) => !left.Equals(right);
    }
}
