using System;

namespace PPR.Main {
    public class Vector2 {
        public static Vector2 zero => new Vector2();
        public static Vector2 one => new Vector2(1, 1);
        public static Vector2 left => new Vector2(-1, 0);
        public static Vector2 right => new Vector2(1, 0);
        public static Vector2 up => new Vector2(0, -1);
        public static Vector2 down => new Vector2(0, 1);

        public int x;
        public int y;
        public Vector2 normalized => new Vector2(x == 0 ? 0 : (x == 1 ? 1 : -1), y == 0 ? 0 : (y == 1 ? 1 : -1));
        public Vector2() {
            x = 0;
            y = 0;
        }
        public Vector2(int x, int y) {
            this.x = x;
            this.y = y;
        }
        public Vector2(Vector2 vector) {
            x = vector.x;
            y = vector.y;
        }

        public void Normalize() {
            x = x == 0 ? 0 : (x == 1 ? 1 : -1);
            y = y == 0 ? 0 : (y == 1 ? 1 : -1);
        }
        public Vector2 Abs() {
            return new Vector2(Math.Abs(x), Math.Abs(y));
        }
        public bool InBounds(int minX, int minY, int maxX, int maxY) {
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
        }
        public bool InBounds(Vector2 min, Vector2 max) {
            return InBounds(min.x, min.y, max.x, max.y);
        }
        public override string ToString() {
            return "(" + x + ", " + y + ")";
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
            return HashCode.Combine(x, y);
        }
    }
}
