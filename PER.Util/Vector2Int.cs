using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PER.Util;

[JsonConverter(typeof(ArrayJsonConverter))]
public readonly struct Vector2Int : IEquatable<Vector2Int> {
    public int x { get; }
    public int y { get; }

    [JsonConstructor]
    public Vector2Int(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public bool InBounds(int minX, int minY, int maxX, int maxY) =>
        x >= minX && x <= maxX && y >= minY && y <= maxY;
    public bool InBounds(Vector2Int min, Vector2Int max) => InBounds(min.x, min.y, max.x, max.y);
    public bool InBounds(Bounds bounds) => InBounds(bounds.min, bounds.max);

    public static Vector2Int operator +(Vector2Int left, Vector2Int right) =>
        new(left.x + right.x, left.y + right.y);

    public static Vector2Int operator -(Vector2Int left, Vector2Int right) =>
        new(left.x - right.x, left.y - right.y);

    public static Vector2Int operator *(Vector2Int left, int right) =>
        new(left.x * right, left.y * right);

    public static Vector2Int operator *(int left, Vector2Int right) => right * left;

    public static Vector2Int operator /(Vector2Int left, int right) =>
        new(left.x / right, left.y / right);

    public bool Equals(Vector2Int other) => x == other.x && y == other.y;

    public override bool Equals(object? obj) => obj is Vector2Int other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(x, y);

    public static bool operator ==(Vector2Int left, Vector2Int right) => left.Equals(right);
    public static bool operator !=(Vector2Int left, Vector2Int right) => !left.Equals(right);

    public class ArrayJsonConverter : JsonConverter<Vector2Int> {
        public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetInt32(), reader.GetInt32());

        public override void Write(Utf8JsonWriter writer, Vector2Int value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }
}
