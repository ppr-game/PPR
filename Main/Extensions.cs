using System;
using System.Collections.Generic;
using System.Text;

using PPR.GUI;
using PPR.Properties;

using PRR;

using SFML.Graphics;
using SFML.System;

namespace PPR.Main {
    public static class Random_Extensions {
        public static float NextFloat(this Random rng, float min, float max) {
            return (float)rng.NextDouble() * (max - min) + min;
        }
    }
    public static class IEnumerable_Extensions {
        public static T ElementAtOrDefault<T>(this IList<T> list, int index, Func<T> @default) {
            return index >= 0 && index < list.Count ? list[index] : @default();
        }
    }
    public static class String_Extensions {
        public static string AddSpaces(this string text) {
            if(string.IsNullOrWhiteSpace(text))
                return "";
            StringBuilder newText = new StringBuilder(text.Length * 2);
            _ = newText.Append(text[0]);
            for(int i = 1; i < text.Length; i++) {
                if(char.IsUpper(text[i]) && text[i - 1] != ' ')
                    _ = newText.Append(' ');
                _ = newText.Append(text[i]);
            }
            return newText.ToString();
        }
    }
    public static class Vector2i_Extensions {
        public static bool InBounds(this Vector2i vector, int minX, int minY, int maxX, int maxY) {
            return vector.X >= minX && vector.X <= maxX && vector.Y >= minY && vector.Y <= maxY;
        }
    }
    public static class Renderer_Extensions {
        public static void UpdateFramerateSetting(this Renderer renderer) {
            renderer.SetFramerateSetting(Settings.GetInt("fpsLimit"));
        }
        public static void UpdateWindow(this Renderer renderer) {
            renderer.UpdateWindow(Settings.GetBool("fullscreen"), Settings.GetInt("fpsLimit"));
        }
        public static void DrawText(this Renderer renderer, Vector2i position, string text,
            Renderer.Alignment align = Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false) {
            renderer.DrawText(position, text, ColorScheme.GetColor("foreground"), align,
                replacingSpaces, invertOnDarkBG);
        }
        public static void DrawText(this Renderer renderer, Vector2i position, string text, Color color,
            Renderer.Alignment align = Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false) {
            renderer.DrawText(position, text, color, ColorScheme.GetColor("transparent"), align, replacingSpaces,
                invertOnDarkBG);
        }
        public static void DrawText(this Renderer renderer, Vector2i position, string text, Color foregroundColor, Color backgroundColor,
            Renderer.Alignment align = Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false) {
            renderer.DrawText(position, text, foregroundColor, backgroundColor,
                ColorScheme.GetColor("background"), align, replacingSpaces, invertOnDarkBG);
        }
        public static void DrawText(this Renderer renderer, Vector2i position, string[] lines, Renderer.Alignment align = Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            renderer.DrawLines(position, lines, align, replacingSpaces, invertOnDarkBG);
        }
        
        public static void DrawText(this Renderer renderer, Vector2i position, string[] lines, Color color, Renderer.Alignment align = Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            renderer.DrawLines(position, lines, color, align, replacingSpaces, invertOnDarkBG);
        }
        public static void DrawText(this Renderer renderer, Vector2i position, string[] lines, Color foregroundColor, Color backgroundColor,
            Renderer.Alignment align = Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            renderer.DrawLines(position, lines, foregroundColor, backgroundColor, align, replacingSpaces, invertOnDarkBG);
        }
        public static void DrawLines(this Renderer renderer, Vector2i position, string[] lines, Renderer.Alignment align = Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            renderer.DrawLines(position, lines, ColorScheme.GetColor("foreground"), align, replacingSpaces, invertOnDarkBG);
        }
        public static void DrawLines(this Renderer renderer, Vector2i position, string[] lines, Color color, Renderer.Alignment align = Renderer.Alignment.Left,
            bool replacingSpaces = false, bool invertOnDarkBG = false) {
            renderer.DrawLines(position, lines, color, ColorScheme.GetColor("transparent"), align, replacingSpaces,
                invertOnDarkBG);
        }
        public static void DrawLines(this Renderer renderer, Vector2i position, string[] lines, Color foregroundColor,
            Color backgroundColor, Renderer.Alignment align = Renderer.Alignment.Left, bool replacingSpaces = false,
            bool invertOnDarkBG = false) {
            renderer.DrawLines(position, lines, foregroundColor, backgroundColor,
                ColorScheme.GetColor("background"), align, replacingSpaces,
                invertOnDarkBG);
        }
        public static void SetCharacter(this Renderer renderer, Vector2i position, RenderCharacter character) {
            renderer.SetCharacter(position, character, ColorScheme.GetColor("background"));
        }
        public static void SetCellColor(this Renderer renderer, Vector2i position, Color foregroundColor,
            Color backgroundColor) {
            renderer.SetCellColor(position, foregroundColor, backgroundColor, ColorScheme.GetColor("background"));
        }
    }
}
