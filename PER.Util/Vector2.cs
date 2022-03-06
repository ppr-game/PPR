using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PER.Util;

[JsonConverter(typeof(ArrayJsonConverter))]
public readonly struct Vector2 : IEquatable<Vector2> {
    public float x { get; }
    public float y { get; }

    [JsonConstructor]
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

    public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(x, y);

    public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
    public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);

    public class ArrayJsonConverter : JsonConverter<Vector2> {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetSingle(), reader.GetSingle());

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }
}
