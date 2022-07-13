using System;

using PER.Abstractions.Input;
using PER.Abstractions.Rendering;
using PER.Util;

using QoiSharp;

using SFML.System;
using SFML.Window;

using BlendMode = PER.Abstractions.Rendering.BlendMode;
using Color = PER.Util.Color;

namespace PRR.Sfml;

public static class SfmlConverters {
    public static Color ToPerColor(SFML.Graphics.Color color) =>
        new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

    public static SFML.Graphics.Color ToSfmlColor(Color color) =>
        new((byte)Math.Clamp(color.r * 255f, 0f, 255f), (byte)Math.Clamp(color.g * 255f, 0f, 255f),
            (byte)Math.Clamp(color.b * 255f, 0f, 255f), (byte)Math.Clamp(color.a * 255f, 0f, 255f));

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
            (SFML.Graphics.BlendMode.Equation)( // fallback to Add if using Max or Min because
                blendMode.colorEquation is BlendMode.Equation.Max or BlendMode.Equation.Min ? // the current version
                    BlendMode.Equation.Add : blendMode.colorEquation), // of SFML doesn't support Max and Min
            (SFML.Graphics.BlendMode.Factor)blendMode.alphaSrcFactor,
            (SFML.Graphics.BlendMode.Factor)blendMode.colorDstFactor,
            (SFML.Graphics.BlendMode.Equation)( // same as above
                blendMode.alphaEquation is BlendMode.Equation.Max or BlendMode.Equation.Min ?
                    BlendMode.Equation.Add : blendMode.alphaEquation));

    public static SFML.Graphics.Image ToSfmlImage(Image image) {
        SFML.Graphics.Image sfmlImage = new((uint)image.width, (uint)image.height);
        for(int y = 0; y < image.height; y++)
            for(int x = 0; x < image.width; x++)
                sfmlImage.SetPixel((uint)x, (uint)y, ToSfmlColor(image[x, y]));

        return sfmlImage;
    }

    public static KeyCode ToPerKey(Keyboard.Key key) => (KeyCode)key;
    public static Keyboard.Key ToSfmlKey(KeyCode key) => (Keyboard.Key)key;

    public static MouseButton ToPerMouseButton(Mouse.Button button) => (MouseButton)button;
    public static Mouse.Button ToSfmlMouseButton(MouseButton button) => (Mouse.Button)button;
}
