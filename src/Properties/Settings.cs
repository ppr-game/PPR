using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using NLog;

namespace PPR.Properties {
    public sealed class SettingChangedEventArgs : EventArgs {
        public string settingName { get; }
        public SettingChangedEventArgs(string settingName) => this.settingName = settingName;
    }
    public static class Settings {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const string Path = "config.txt";

        public static EventHandler<SettingChangedEventArgs> settingChanged;

        private static Dictionary<string, bool> _booleans = new Dictionary<string, bool>();
        private static Dictionary<string, int> _integers = new Dictionary<string, int>();
        private static Dictionary<string, string> _strings = new Dictionary<string, string>();

        public static void Reload() {
            _booleans = new Dictionary<string, bool> {
                {"bloom", true}, {"fullscreen", false}, {"uppercaseNotes", false}, {"showFps", false}
            };
            _integers = new Dictionary<string, int> {
                {"musicVolume", 15}, {"soundsVolume", 10}, {"fpsLimit", 480}
            };
            _strings = new Dictionary<string, string> {
                {"audio", "Default"}, {"font", System.IO.Path.Join("Codepage 437", "10x10", "x1")},
                {"colorScheme", System.IO.Path.Join("Default", "Classic")}
            };
            
            LoadConfig();
        }

        private static void LoadConfig() {
            if(!File.Exists(Path)) {
                logger.Info("The config was not found, using default config");
                return;
            }
            string[] lines = File.ReadAllLines(Path);
            
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
                File.WriteAllText(Path, "# Booleans\n");
                File.AppendAllLines(Path, strBools);
            }
            if(strInts.Length > 0) {
                File.AppendAllText(Path, "\n# Integers\n");
                File.AppendAllLines(Path, strInts);
            }
            if(strStrings.Length <= 0) return;
            File.AppendAllText(Path, "\n# Strings\n");
            File.AppendAllLines(Path, strStrings);
        }

        public static bool GetBool(string key) => _booleans.TryGetValue(key, out bool value) && value;
        public static int GetInt(string key) => _integers.TryGetValue(key, out int value) ? value : 0;
        public static string GetString(string key) => _strings.TryGetValue(key, out string value) ? value : "";
        public static string[] GetPathArray(string key) => GetString(key).Split(System.IO.Path.AltDirectorySeparatorChar);
        public static string GetPath(string key) => string.Join(System.IO.Path.DirectorySeparatorChar, GetPathArray(key));

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
        public static void SetPath(string key, string value) => SetString(key, value.Replace(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
    }
}
