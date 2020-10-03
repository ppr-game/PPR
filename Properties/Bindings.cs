using System.Collections.Generic;
using System.IO;
using System.Linq;

using NLog;

namespace PPR.Properties {
    public static class Bindings {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        const string PATH = "keybinds.txt";

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
                { "hpDrainUp", new InputKey("Right") },
                { "hpDrainDown", new InputKey("Left") },
                { "hpRestorageUp", new InputKey("LShift+Right,RShift+Right") },
                { "hpRestorageDown", new InputKey("LShift+Left,RShift+Left") },
                { "linesFrequencyUp", new InputKey("LAlt+Up,RAlt+Up") },
                { "linesFrequencyDown", new InputKey("LAlt+Down,RAlt+Down") },
                { "initialOffsetUp", new InputKey("F2") },
                { "initialOffsetDown", new InputKey("F1") },
                { "initialOffsetUpBoost", new InputKey("LShift+F2,RShift+F2") },
                { "initialOffsetDownBoost", new InputKey("LShift+F1,RShift+F1") },
                { "speedUpSlow", new InputKey("LShift+Up,RShift+Up") },
                { "speedDownSlow", new InputKey("LShift+Down,RShift+Down") },
                { "fastScrollUp", new InputKey("PageUp") },
                { "fastScrollDown", new InputKey("PageDown") },
                { "fullscreen", new InputKey("F11") }
            };
            
            LoadBindings();
        }
        
        static void LoadBindings() {
            if(!File.Exists(PATH)) {
                logger.Info("The bindings file was not found, using default bindings");
                return;
            }
            string[] lines = File.ReadAllLines(PATH);
            
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
        public static void SaveBindings() => File.WriteAllLines(PATH, keys.Select(value => $"{value.Key}={value.Value.asString}").ToArray());

        public static InputKey GetBinding(string name) => keys.TryGetValue(name, out InputKey key) ? key : null;
    }
}
