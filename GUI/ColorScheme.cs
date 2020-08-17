using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using PPR.Main;
using PPR.Properties;

using SFML.Graphics;

namespace PPR.GUI {
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable NotAccessedField.Global
    public static class ColorScheme {
        static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>();
        public static void Reload() {
            colors.Clear();
            
            string filePath = Path.Combine("resources", "colors", Settings.Default.colorScheme, "colors.txt");
            if(!File.Exists(filePath))
                filePath = Path.Combine("resources", "colors", "Default", "Classic", "colors.txt");
            string[] lines = File.ReadAllLines(filePath);
            
            foreach(string line in lines) {
                string usableLine = line.Split('#')[0].Replace(' ', '\0');
                string[] keyValue = usableLine.Split('=');
                if(keyValue.Length != 2) continue;
                string key = keyValue[0];
                string value = keyValue[1];
                if(string.IsNullOrEmpty(key.Trim()) || string.IsNullOrEmpty(value.Trim())) continue;
                string[] strValues = value.Split(',');
                byte[] values = strValues.Length == 3 || strValues.Length == 4 ?
                    strValues.Select(byte.Parse).ToArray() :
                    Array.Empty<byte>();
                colors[key] = values.Length == 3 || values.Length == 4 ?
                    new Color(values[0], values[1], values[2], values.Length == 4 ? values[3] : byte.MaxValue) :
                    colors[value];
            }

            Game.UpdateSettings();
        }

        public static Color GetColor(string key) {
            return colors.ContainsKey(key) ? colors[key] : Color.Transparent;
        }
    }
}
