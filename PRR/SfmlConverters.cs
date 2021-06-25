using PER.Abstractions.Renderer;
using PER.Util;

using SFML.System;

namespace PRR {
    public static class SfmlConverters {
        public static Color ToPerColor(SFML.Graphics.Color color) =>
            new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        
        public static SFML.Graphics.Color ToSfmlColor(Color color) =>
            new((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        
        public static Vector2Int ToPerVector2Int(Vector2i vector) => new(vector.X, vector.Y);
        public static Vector2i ToSfmlVector2Int(Vector2Int vector) => new(vector.x, vector.y);
        
        public static Vector2 ToPerVector2(Vector2f vector) => new(vector.X, vector.Y);
        public static Vector2f ToSfmlVector2(Vector2 vector) => new(vector.x, vector.y);
    }
}
