using System;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
public readonly struct Color : IEquatable<Color> {
    public static Color transparent => new(0f, 0f, 0f, 0f);
    public static Color black => new(0f, 0f, 0f, 1f);
    public static Color white => new(1f, 1f, 1f, 1f);

    public float r { get; }
    public float g { get; }
    public float b { get; }
    public float a { get; }

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
        // i've spent SO MUCH time fixing this bug_
        // so, when i tried drawing something over a character which previously had a transparent background,
        // the background was transparent, even tho i wasn't drawing it with a transparent background.
        // i tried *everything*, and when i finally decided to actually debug it,
        // it turned out the the RGB of the color was NaN for some reason.
        // i immediately realized i was dividing by 0 somewhere.
        // i went here, and the only place that had division was... yep, it's here, RGB of the color.
        // when i drew transparent over transparent, both bottom.a and top.a were 0,
        // which caused a to be 0, which caused a division by 0, which caused the RGB of the color be NaN,NaN,NaN,
        // which caused any other operation with that color return NaN, which was displaying as if it was black...
        // i wanna f---ing die.
        if(bottom.a == 0f && top.a == 0f) return transparent;

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
        new Color(MoreMath.LerpUnclamped(a.r, b.r, t),
            MoreMath.LerpUnclamped(a.g, b.g, t),
            MoreMath.LerpUnclamped(a.b, b.b, t),
            MoreMath.LerpUnclamped(a.a, b.a, t));

    public static Color operator +(Color left, Color right) =>
        new(left.r + right.r, left.g + right.g, left.b + right.b, left.a + right.a);

    public static Color operator -(Color left, Color right) =>
        new(left.r - right.r, left.g - right.g, left.b - right.b, right.a);

    public static Color operator *(Color left, float right) =>
        new(left.r * right, left.g * right, left.b * right, left.a * right);

    public static Color operator *(float left, Color right) => right * left;

    public static Color operator /(Color left, float right) =>
        new(left.r / right, left.g / right, left.b / right, left.a / right);

    public bool Equals(Color other) =>
        r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);

    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(r, g, b, a);

    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => !left.Equals(right);
}
