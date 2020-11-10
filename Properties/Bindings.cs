using System.Collections.Generic;
using System.IO;
using System.Linq;

using NLog;

namespace PPR.Properties {
    public static class Bindings {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string Path = "keybinds.txt";

        public static Dictionary<string, InputKey> keys;
        
        public static void Reload() {
            keys = new Dictionary<string, InputKey> {
                { "shit", new InputKey("N+O") },
                { "not like", new InputKey("T+H,A+T") },
                { "this menu is still under construc", new InputKey("T+I,O+N") },
                { "back", new InputKey("Escape") },
                { "erase", new InputKey("Backspace") },
                { "speedUp", new InputKey("Up") },
                { "speedDown", new InputKey("Down") },
                { "linesFrequencyUp", new InputKey("LAlt+Up,RAlt+Up") },
                { "linesFrequencyDown", new InputKey("LAlt+Down,RAlt+Down") },
                { "speedUpSlow", new InputKey("LShift+Up,RShift+Up") },
                { "speedDownSlow", new InputKey("LShift+Down,RShift+Down") },
                { "fastScrollUp", new InputKey("PageUp") },
                { "fastScrollDown", new InputKey("PageDown") },
                { "fullscreen", new InputKey("F11") },
                { "cut", new InputKey("LControl+X,RControl+X") },
                { "copy", new InputKey("LControl+C,RControl+C") },
                { "paste", new InputKey("LControl+V,RControl+V") }
            };
            
            LoadBindings();
        }

        private static void LoadBindings() {
            if(!File.Exists(Path)) {
                logger.Info("The bindings file was not found, using default bindings");
                return;
            }
            string[] lines = File.ReadAllLines(Path);
            
            foreach(string line in lines) {
                string usableLine = line.Split('#')[0].Replace(' ', '\0');
                string[] keyValue = usableLine.Split('=');
                if(keyValue.Length != 2) continue;
                string key = keyValue[0];
                string value = keyValue[1];
                if(string.IsNullOrEmpty(key.Trim()) || string.IsNullOrEmpty(value.Trim())) continue;
                keys[key] = new InputKey(value);
            }
        }
        public static void SaveBindings() => File.WriteAllLines(Path, keys.Select(value => $"{value.Key}={value.Value.asString}").ToArray());

        public static InputKey GetBinding(string name) => keys.TryGetValue(name, out InputKey key) ? key : null;
    }
}
