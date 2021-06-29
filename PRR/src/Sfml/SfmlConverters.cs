using System;

using PER.Util;

using SFML.Graphics;
using SFML.System;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using BlendMode = PER.Abstractions.Renderer.BlendMode;
using Color = PER.Util.Color;

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

        public static BlendMode ToPerBlendMode(SFML.Graphics.BlendMode blendMode) =>
            new((BlendMode.Factor)blendMode.ColorSrcFactor, (BlendMode.Factor)blendMode.ColorDstFactor,
                (BlendMode.Equation)blendMode.ColorEquation, (BlendMode.Factor)blendMode.AlphaSrcFactor,
                (BlendMode.Factor)blendMode.AlphaDstFactor, (BlendMode.Equation)blendMode.AlphaEquation);

        public static SFML.Graphics.BlendMode ToSfmlBlendMode(BlendMode blendMode) =>
            new((SFML.Graphics.BlendMode.Factor)blendMode.colorSrcFactor,
                (SFML.Graphics.BlendMode.Factor)blendMode.colorDstFactor,
                (SFML.Graphics.BlendMode.Equation)blendMode.colorEquation,
                (SFML.Graphics.BlendMode.Factor)blendMode.alphaSrcFactor,
                (SFML.Graphics.BlendMode.Factor)blendMode.colorDstFactor,
                (SFML.Graphics.BlendMode.Equation)blendMode.alphaEquation);

        public static SFML.Graphics.Image ToSfmlImage(Image<Rgba32> image) {
            SFML.Graphics.Image sfmlImage = new((uint)image.Width, (uint)image.Height);
            for(int y = 0; y < image.Height; y++) {
                Span<Rgba32> row = image.GetPixelRowSpan(y);
                for(int x = 0; x < row.Length; x++) {
                    Rgba32 pixel = row[x];
                    sfmlImage.SetPixel((uint)x, (uint)y, new SFML.Graphics.Color(pixel.R, pixel.G, pixel.B, pixel.A));
                }
            }

            return sfmlImage;
        }
    }
}
