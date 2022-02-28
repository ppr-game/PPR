using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NLog;

using PPROld.Properties;

using PPROld.Main;

using SFML.Graphics;

namespace PPROld.UI {
    internal enum ElementColorType { None, Element, Tag }
    public readonly struct ElementColor {
        public readonly int type;
        public readonly int idTag;
        public readonly int name;

        public ElementColor(string type, string idTag, string name) : this(type.GetHashCode(), idTag.GetHashCode(),
            name.GetHashCode()) { }

        public ElementColor(int typeHash, int idTagHash, int nameHash) {
            type = typeHash;
            idTag = idTagHash;
            name = nameHash;
        }
    }

    public static class ColorScheme {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>();
        private static readonly Dictionary<ElementColor, Color> elementColors = new Dictionary<ElementColor, Color>();
        private static readonly Dictionary<ElementColor, Color> tagColors = new Dictionary<ElementColor, Color>();
        public static void Reload() {
            colors.Clear();

            string bloomBlendShaderPath = Path.Join("resources", "colors", Settings.GetPath("colorScheme"),
                "bloom-blend_frag.glsl");
            if(!File.Exists(bloomBlendShaderPath))
                bloomBlendShaderPath = Path.Join("resources", "colors", "Default", "Classic",
                    "bloom-blend_frag.glsl");
            if(!File.Exists(bloomBlendShaderPath)) {
                FileNotFoundException ex =
                    new FileNotFoundException("The default color scheme bloom blend shader was not found");
                logger.Fatal(ex);
                throw ex;
            }
            Core.renderer.bloomBlend = Shader.FromString(
                File.ReadAllText(Path.Join("resources", "bloom_vert.glsl")), null,
                File.ReadAllText(bloomBlendShaderPath));

            LoadSchemes(Path.Join("Default", "Classic"), Settings.GetPath("colorScheme"));

            Game.UpdateSettings();
        }

        public static PER.Util.Color GetColor(string key) => colors.ContainsKey(key) ? colors[key] : Color.Transparent;

        public static bool TryGetElementColor(ElementColor key, out Color color) {
            color = Color.Transparent;
            bool colorExists = elementColors.ContainsKey(key);
            if(colorExists) color = elementColors[key];
            return colorExists;
        }

        public static bool TryGetTagColor(ElementColor key, out Color color) {
            color = Color.Transparent;
            bool colorExists = tagColors.ContainsKey(key);
            if(colorExists) color = tagColors[key];
            return colorExists;
        }

        private static void LoadSchemes(string basePath, string priorityPath) {
            #region Get the paths

            string baseFilePath = Path.Join("resources", "colors", basePath, "colors.txt");
            string priorityFilePath = Path.Join("resources", "colors", priorityPath, "colors.txt");
            if(!File.Exists(priorityFilePath)) {
                logger.Warn($"The color scheme at '{priorityPath}' was not found, falling back to base.");
                priorityFilePath = baseFilePath;
            }
            if(!File.Exists(baseFilePath)) {
                FileNotFoundException ex = new FileNotFoundException("The base color scheme was not found.");
                logger.Fatal(ex);
                throw ex;
            }
            string[] baseLines = File.ReadAllLines(baseFilePath);
            string[] priorityLines = File.ReadAllLines(priorityFilePath);

            #endregion

            Dictionary<string, string> tempColors = new Dictionary<string, string>();

            // Save the base color scheme values in a dictionary
            foreach(string line in baseLines) {
                string[] keyValue = ParseSchemeLine(line);
                if(keyValue == null) continue;
                if(keyValue.Length != 2) {
                    logger.Warn($"The base line '{line}' could not be parsed.");
                    continue;
                }
                string key = keyValue[0];
                string value = keyValue[1];
                tempColors[key] = value;
            }
            // Apply the priority overrides to the dictionary
            foreach(string line in priorityLines) {
                string[] keyValue = ParseSchemeLine(line);
                if(keyValue == null) continue;
                if(keyValue.Length != 2) {
                    logger.Warn($"The priority line '{line}' could not be parsed.");
                    continue;
                }
                string key = keyValue[0];
                string value = keyValue[1];
                tempColors[key] = value;
            }

            // Parse and load the final dictionary as our color scheme
            foreach((string key, string value) in tempColors) {
                Color? color = ParseSchemeColor(value);
                if(color == null) {
                    if(tempColors.ContainsKey(value)) SetColor(key, colors[value]);
                    else logger.Warn($"The color variable '{key} : {value}' could not be parsed.");
                }
                else SetColor(key, (Color)color);
            }
        }

        private static void SetColor(string key, Color color) {
            colors[key] = color;
            ElementColorType elementColorType = TryParseElementColorKey(key, out ElementColor elementColor);
            switch(elementColorType) {
                case ElementColorType.Element: elementColors[elementColor] = colors[key];
                    break;
                case ElementColorType.Tag: tagColors[elementColor] = colors[key];
                    break;
            }
        }

        private static ElementColorType TryParseElementColorKey(string key, out ElementColor color) {
            color = new ElementColor("", "", "");
            string[] keys = key.Split('_');

            if(keys.Length < 3) return ElementColorType.None;

            string type = keys[0];
            string id = string.Join('_', keys, 1, keys.Length - 2);
            string name = keys[^1];

            if(id.StartsWith('@')) {
                color = new ElementColor(type, id[1..], name);
                return ElementColorType.Tag;
            }

            color = new ElementColor(type, id, name);
            return ElementColorType.Element;
        }

        private static string[] ParseSchemeLine(string line) {
            string usableLine = line.Split('#')[0].Replace(' ', '\0');
            string[] keyValue = usableLine.Split('=');
            if(string.IsNullOrEmpty(keyValue[0].Trim()) || string.IsNullOrEmpty(keyValue[1].Trim())) return null;
            return keyValue;
        }

        private static Color? ParseSchemeColor(string color) {
            string[] strValues = color.Split(',');
            byte[] values = strValues.Length == 3 || strValues.Length == 4 ?
                strValues.Select(byte.Parse).ToArray() : Array.Empty<byte>();
            return strValues.Length == 3 || strValues.Length == 4 ?
                new Color(values[0], values[1], values[2],
                values.Length == 4 ? values[3] : byte.MaxValue) : (Color?)null;
        }
    }
}
