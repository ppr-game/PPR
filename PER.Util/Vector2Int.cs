using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace PER.Util;

[PublicAPI]
[JsonConverter(typeof(JsonConverter))]
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

    public class JsonConverter : JsonConverter<Vector2Int> {
        public override Vector2Int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            bool isObject = true;
            if(reader.TokenType != JsonTokenType.Number) isObject = reader.TokenType == JsonTokenType.StartObject;
            reader.Read();
            int x = 0;
            int y = 0;
            if(isObject) {
                for(int i = 0; i < 2; i++) {
                    string? propertyType = reader.GetString();
                    reader.Read();
                    switch(propertyType) {
                        case nameof(Vector2Int.x):
                            x = reader.GetInt32();
                            break;
                        case nameof(Vector2Int.y):
                            y = reader.GetInt32();
                            break;
                    }
                    reader.Read();
                }
            }
            else {
                x = reader.GetInt32();
                reader.Read();
                y = reader.GetInt32();
            }
            return new Vector2Int(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2Int value, JsonSerializerOptions options) {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.x);
            writer.WriteNumberValue(value.y);
            writer.WriteEndArray();
        }
    }
}
