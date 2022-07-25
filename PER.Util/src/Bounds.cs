using System;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
public readonly struct Bounds : IEquatable<Bounds> {
    public Vector2Int min { get; }
    public Vector2Int max { get; }

    [JsonConstructor]
    public Bounds(Vector2Int min, Vector2Int max) {
        this.min = min;
        this.max = max;
    }

    // https://stackoverflow.com/a/100165/10484146
    public bool IntersectsLine(Vector2Int point0, Vector2Int point1) {
        // Find min and max X for the segment
        int minX = Math.Min(point0.x, point1.x);
        int maxX = Math.Max(point0.x, point1.x);

        // Find the intersection of the segment's and rectangle's X-projections
        if(maxX > max.x) maxX = max.x;
        if(minX < min.x) minX = min.x;

        // If their projections do not intersect return false
        if(minX > maxX) return false;

        // Find corresponding min and max Y for min and max X we found before
        int minY = point0.y;
        int maxY = point1.y;

        int dx = point1.x - point0.x;
        if(Math.Abs(dx) > 0) {
            int a = (point1.y - point0.y) / dx;
            int b = point0.y - a * point0.x;
            minY = a * minX + b;
            maxY = a * maxX + b;
        }

        if(minY > maxY) (maxY, minY) = (minY, maxY);

        if(maxY > max.y) maxY = max.y;
        if(minY < min.y) minY = min.y;

        // If Y-projections do not intersect return false
        return minY <= maxY;
    }

    public bool Equals(Bounds other) => min.Equals(other.min) && max.Equals(other.max);

    public override bool Equals(object? obj) => obj is Bounds other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(min, max);

    public static bool operator ==(Bounds left, Bounds right) => left.Equals(right);
    public static bool operator !=(Bounds left, Bounds right) => !left.Equals(right);
}
