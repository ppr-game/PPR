using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using NLog;

namespace PPR.Properties {
    public sealed class SettingChangedEventArgs : EventArgs {
        public string settingName { get; }
        public SettingChangedEventArgs(string settingName) {
            this.settingName = settingName;
        }
    }
    public static class Settings {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        const string PATH = "config.txt";

        public static EventHandler<SettingChangedEventArgs> settingChanged;
        
        static Dictionary<string, bool> _booleans = new Dictionary<string, bool>();
        static Dictionary<string, int> _integers = new Dictionary<string, int>();
        static Dictionary<string, string> _strings = new Dictionary<string, string>();

        public static void Reload() {
            _booleans = new Dictionary<string, bool> {
                {"bloom", true}, {"fullscreen", false}, {"uppercaseNotes", false}, {"showFps", false}
            };
            _integers = new Dictionary<string, int> {
                {"musicVolume", 15}, {"soundsVolume", 10}, {"fpsLimit", 480}
            };
            _strings = new Dictionary<string, string> {
                {"audio", "Default"}, {"font", Path.Join("Codepage 437", "10x10", "x1")},
                {"colorScheme", Path.Join("Default", "Classic")}
            };
            
            LoadConfig();
        }
        
        static void LoadConfig() {
            if(!File.Exists(PATH)) {
                logger.Info("The config was not found, using default config");
                return;
            }
            string[] lines = File.ReadAllLines(PATH);
            
            foreach(string line in lines) {
                string usableLine = line.Split('#')[0];
                string[] keyValue = usableLine.Split('=');
                if(keyValue.Length != 2) continue;
                string key = keyValue[0];
                string value = keyValue[1];
                if(string.IsNullOrEmpty(key.Trim()) || string.IsNullOrEmpty(value.Trim())) continue;
                if(bool.TryParse(value, out bool boolValue)) _booleans[key] = boolValue;
                else if(int.TryParse(value, out int intValue)) _integers[key] = intValue;
                else _strings[key] = value;
            }
        }
        public static void SaveConfig() {
            CultureInfo culture = CultureInfo.InvariantCulture;
            string[] strBools = _booleans.Select(value => $"{value.Key}={value.Value.ToString(culture)}").ToArray();
            string[] strInts = _integers.Select(value => $"{value.Key}={value.Value.ToString(culture)}").ToArray();
            string[] strStrings = _strings.Select(value => $"{value.Key}={value.Value}").ToArray();
            if(strBools.Length > 0) {
                File.WriteAllText(PATH, "# Booleans\n");
                File.AppendAllLines(PATH, strBools);
            }
            if(strInts.Length > 0) {
                File.AppendAllText(PATH, "\n# Integers\n");
                File.AppendAllLines(PATH, strInts);
            }
            if(strStrings.Length > 0) {
                File.AppendAllText(PATH, "\n# Strings\n");
                File.AppendAllLines(PATH, strStrings);
            }
        }

        public static bool GetBool(string key) {
            return _booleans.TryGetValue(key, out bool value) && value;
        }
        public static int GetInt(string key) {
            return _integers.TryGetValue(key, out int value) ? value : 0;
        }
        public static string GetString(string key) {
            return _strings.TryGetValue(key, out string value) ? value : "";
        }
        public static string[] GetPathArray(string key) {
            return GetString(key).Split(Path.AltDirectorySeparatorChar);
        }
        public static string GetPath(string key) {
            return string.Join(Path.DirectorySeparatorChar, GetPathArray(key));
        }
        
        public static void SetBool(string key, bool value) {
            _booleans[key] = value;
            settingChanged?.Invoke(null, new SettingChangedEventArgs(key));
        }
        public static void SetInt(string key, int value) {
            _integers[key] = value;
            settingChanged?.Invoke(null, new SettingChangedEventArgs(key));
        }
        public static void SetString(string key, string value) {
            _strings[key] = value;
            settingChanged?.Invoke(null, new SettingChangedEventArgs(key));
        }
        public static void SetPath(string key, string value) {
            SetString(key, value.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }
    }
}
