using System;

namespace PER.Util {
    public readonly struct Vector2 {
        public readonly float x;
        public readonly float y;

        public Vector2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 left, Vector2 right) =>
            new(left.x + right.x, left.y + right.y);

        public static Vector2 operator -(Vector2 left, Vector2 right) =>
            new(left.x - right.x, left.y - right.y);

        public static Vector2 operator *(Vector2 left, float right) =>
            new(left.x * right, left.y * right);

        public static Vector2 operator *(float left, Vector2 right) => right * left;

        public static Vector2 operator /(Vector2 left, float right) =>
            new(left.x / right, left.y / right);

        public bool Equals(Vector2 other) => x == other.x && y == other.y;

        public override bool Equals(object obj) => obj is Vector2Int other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(x, y);

        public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
        public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);
    }
}
