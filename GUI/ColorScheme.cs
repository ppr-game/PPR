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
            
            Dictionary<string, string> queue = new Dictionary<string, string>();
            
            LoadScheme(Path.Join("Default", "Classic"), queue);
            LoadScheme(Settings.GetPath("colorScheme"), queue);
            
            LoadQueue(queue);

            Game.UpdateSettings();
        }

        static void LoadScheme(string path, IDictionary<string, string> queue) {
            string filePath = Path.Join("resources", "colors", path, "colors.txt");
            if(!File.Exists(filePath)) {
                logger.Warn($"The color scheme at '{path}' was not found, falling back to default");
                filePath = Path.Join("resources", "colors", "Default", "Classic", "colors.txt");
            }
            if(!File.Exists(filePath)) {
                FileNotFoundException ex = new FileNotFoundException("The default color scheme was not found");
                logger.Fatal(ex);
                throw ex;
            }
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
                if(values.Length == 3 || values.Length == 4)
                    colors[key] = new Color(values[0], values[1], values[2],
                        values.Length == 4 ? values[3] : byte.MaxValue);
                else queue[key] = value;
            }
        }
        static void LoadQueue(Dictionary<string, string> queue) {
            foreach((string key, string value) in queue) colors[key] = colors[value];
        }

        public static Color GetColor(string key) {
            return colors.ContainsKey(key) ? colors[key] : Color.Transparent;
        }
    }
}
