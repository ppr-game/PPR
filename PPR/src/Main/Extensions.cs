using System;
using System.Collections.Generic;
using System.Text;

using MoonSharp.Interpreter;

using PER.Abstractions.Renderer;

using PPR.UI;
using PPR.Properties;

using PRR;
using PRR.Sfml;

using SFML.Graphics;
using SFML.System;

namespace PPR.Main {
    public static class RandomExtensions {
        public static float NextFloat(this Random rng, float min, float max) => (float)rng.NextDouble() * (max - min) + min;
    }
    
    public static class EnumerableExtensions {
        public static T ElementAtOrDefault<T>(this IList<T> list, int index, Func<T> @default) => index >= 0 && index < list.Count ? list[index] : @default();
    }
    
    public static class StringExtensions {
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
    
    public static class Vector2IExtensions {
        public static bool InBounds(this Vector2i vector, int minX, int minY, int maxX, int maxY) =>
            vector.X >= minX && vector.X <= maxX && vector.Y >= minY && vector.Y <= maxY;
        public static bool InBounds(this Vector2i vector, Vector2i min, Vector2i max) =>
            InBounds(vector, min.X, min.Y, max.X, max.Y);
        public static bool InBounds(this Vector2i vector, Bounds bounds) =>
            InBounds(vector, bounds.min, bounds.max);
    }

    public static class ScriptExtensions {
        public static void SendMessage(this Script script, string name) { 
            if(script.Globals.Get(name).Function != null) 
                script.Call(script.Globals.Get(name));
        }
        
        public static void SendMessage(this Script script, string name, params DynValue[] args) { 
            if(script.Globals.Get(name).Function != null) 
                script.Call(script.Globals.Get(name), args);
        }
    }
    
    public static class RendererExtensions {
        public static void UpdateFramerateSetting(this Renderer renderer) {
            int setting = Settings.GetInt("fpsLimit");
            renderer.framerate = renderer.focused ? setting < 60 ? -1 : setting > 960 ? 0 : setting : 30;
        }

        public static void ResetBackground(this Renderer renderer) =>
            renderer.background = SfmlConverters.ToPerColor(ColorScheme.GetColor("background"));
    }
}
