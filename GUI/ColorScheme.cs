using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NLog;

using PPR.Main;
using PPR.Properties;

using SFML.Graphics;

namespace PPR.GUI {
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable NotAccessedField.Global
    public static class ColorScheme {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        static readonly Dictionary<string, Color> colors = new Dictionary<string, Color>();
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

        static void LoadSchemes(string basePath, string priorityPath) {
            // Get the paths
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
                    if(tempColors.ContainsKey(value)) colors[key] = colors[value];
                    else logger.Warn($"The color variable '{key} : {value}' could not be parsed.");
                }
                else colors[key] = (Color)color;
            }
        }
        static string[] ParseSchemeLine(string line) {
            string usableLine = line.Split('#')[0].Replace(' ', '\0');
            string[] keyValue = usableLine.Split('=');
            if(string.IsNullOrEmpty(keyValue[0].Trim()) || string.IsNullOrEmpty(keyValue[1].Trim())) return null;
            return keyValue;
        }
        static Color? ParseSchemeColor(string color) {
            string[] strValues = color.Split(',');
            byte[] values = strValues.Length == 3 || strValues.Length == 4 ?
                strValues.Select(byte.Parse).ToArray() : Array.Empty<byte>();
            return strValues.Length == 3 || strValues.Length == 4 ? 
                new Color(values[0], values[1], values[2],
                values.Length == 4 ? values[3] : byte.MaxValue) : (Color?)null;
        }

        public static Color GetColor(string key) => colors.ContainsKey(key) ? colors[key] : Color.Transparent;
    }
}
