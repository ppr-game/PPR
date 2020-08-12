using System;

namespace PPR.Main {
    public struct Vector2 {
        public int x;
        public int y;

        public Vector2(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public Vector2(Vector2 vector) {
            x = vector.x;
            y = vector.y;
        }
        
        public bool InBounds(int minX, int minY, int maxX, int maxY) {
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }
        public override string ToString() {
            return $"({x.ToString()}, {y.ToString()})";
        }

        public static Vector2 operator +(Vector2 left, Vector2 right) {
            return new Vector2(left.x + right.x, left.y + right.y);
        }
        public static Vector2 operator -(Vector2 left, Vector2 right) {
            return new Vector2(left.x - right.x, left.y - right.y);
        }
        public static Vector2 operator *(Vector2 left, int right) {
            return new Vector2(left.x * right, left.y * right);
        }
        public static Vector2 operator /(Vector2 left, int right) {
            return new Vector2(left.x / right, left.y / right);
        }
        public static bool operator ==(Vector2 left, Vector2 right) {
            return left.x == right.x && left.y == right.y;
        }
        public static bool operator !=(Vector2 left, Vector2 right) {
            return left.x != right.x || left.y != right.y;
        }
        public override bool Equals(object obj) {
            return obj is Vector2 vector &&
                   x == vector.x &&
                   y == vector.y;
        }
        public override int GetHashCode() {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return HashCode.Combine(x, y);
        }
    }
}
