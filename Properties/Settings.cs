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
        
        static Dictionary<string, bool> _booleans;
        static Dictionary<string, int> _integers;
        static Dictionary<string, string> _strings;

        public static void Reload() {
            _booleans = new Dictionary<string, bool> {
                {"bloom", true}, {"fullscreen", false}, {"uppercaseNotes", false}, {"showFps", false}
            };
            _integers = new Dictionary<string, int> {
                {"musicVolume", 15}, {"soundsVolume", 10}
            };
            _strings = new Dictionary<string, string> {
                {"audio", "Default"}, {"font", Path.Combine("Codepage 437", "10x10", "x1")},
                {"colorScheme", Path.Combine("Default", "Classic")}
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
                string usableLine = line.Split('#')[0].Replace(' ', '\0');
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
            string[][] allArrs = { strBools, strInts, strStrings };
            string[] lines = new string[strBools.Length + strInts.Length + strStrings.Length];
            int curIndex = 0;
            int curArr = 0;
            for(int i = 0; i < lines.Length; i++) {
                if(curArr >= allArrs.Length) continue;
                if(curIndex < allArrs[curArr].Length) lines[i] = allArrs[curArr][curIndex++];
                else {
                    curIndex = 0;
                    curArr++;
                }
            }
            File.WriteAllLines(PATH, lines);
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
        
        public static void SetBool(string key, bool value) {
            settingChanged?.Invoke(null, new SettingChangedEventArgs(key));
            _booleans[key] = value;
        }
        public static void SetInt(string key, int value) {
            settingChanged?.Invoke(null, new SettingChangedEventArgs(key));
            _integers[key] = value;
        }
        public static void SetString(string key, string value) {
            settingChanged?.Invoke(null, new SettingChangedEventArgs(key));
            _strings[key] = value;
        }
    }
}
