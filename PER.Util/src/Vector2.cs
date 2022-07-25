using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
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
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            bool isObject = true;
            if(reader.TokenType != JsonTokenType.Number) isObject = reader.TokenType == JsonTokenType.StartObject;
            reader.Read();
            float x = 0;
            float y = 0;
            if(isObject)
                for(int i = 0; i < 2; i++) {
                    string? propertyType = reader.GetString();
                    reader.Read();
                    switch(propertyType) {
                        case nameof(Vector2Int.x):
                            x = reader.GetSingle();
                            break;
                        case nameof(Vector2Int.y):
                            y = reader.GetSingle();
                            break;
                    }
                    reader.Read();
                }
            else {
                x = reader.GetSingle();
                reader.Read();
                y = reader.GetSingle();
            }
            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }
}
