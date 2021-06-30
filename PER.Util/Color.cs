using System;

namespace PER.Util {
    public readonly struct Color : IEquatable<Color> {
        public static Color transparent => new(0f, 0f, 0f, 0f);
        public static Color black => new(0f, 0f, 0f, 1f);
        public static Color white => new(1f, 1f, 1f, 1f);
        
        public readonly float r;
        public readonly float g;
        public readonly float b;
        public readonly float a;

        public Color(float r, float g, float b, float a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(byte r, byte g, byte b, byte a) {
            this.r = r / 255f;
            this.g = g / 255f;
            this.b = b / 255f;
            this.a = a / 255f;
        }

        public static Color Blend(Color bottom, Color top) {
            float t = (1f - top.a) * bottom.a;
            float a = t + top.a;

            float r = (t * bottom.r + top.a * top.r) / a;
            float g = (t * bottom.g + top.a * top.g) / a;
            float b = (t * bottom.b + top.a * top.b) / a;

            return new Color(r, g, b, a);
        }

        public Color Blend(Color top) => Blend(this, top);

        public static Color LerpColors(Color a, Color b, float t) => t <= 0f ? a :
            t >= 1f ? b :
            new Color(MathF.Floor(a.r + (b.r - a.r) * t),
                g: MathF.Floor(a.g + (b.g - a.g) * t),
                MathF.Floor(a.b + (b.b - a.b) * t),
                MathF.Floor(a.a + (b.a - a.a) * t));

        public static Color operator +(Color left, Color right) =>
            new(left.r + right.r, left.g + right.g, left.b + right.b, left.a + right.a);

        public static Color operator -(Color left, Color right) =>
            new(left.r - right.r, left.g - right.g, left.b - right.b, left.a - right.a);

        public static Color operator *(Color left, float right) =>
            new(left.r * right, left.g * right, left.b * right, left.a * right);

        public static Color operator *(float left, Color right) => right * left;

        public static Color operator /(Color left, float right) =>
            new(left.r / right, left.g / right, left.b / right, left.a / right);

        public bool Equals(Color other) =>
            r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);

        public override bool Equals(object obj) => obj is Color other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(r, g, b, a);

        public static bool operator ==(Color left, Color right) => left.Equals(right);
        public static bool operator !=(Color left, Color right) => !left.Equals(right);
    }
}
