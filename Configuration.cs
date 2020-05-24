using System.IO;

using PPR.Core;
using PPR.GUI;

namespace PPR.Configuration {
    public static class Config {
        public const string CONFIG_FILE_PATH = "config.txt";

        public static int musicVolume = 15;

        public static void LoadConfig() {
            if(File.Exists(CONFIG_FILE_PATH)) {
                string text = File.ReadAllLines(CONFIG_FILE_PATH)[0];
                string[] values = text.Split(',');
                for(int i = 0; i < values.Length; i++) {
                    switch(i) {
                        case 0:
                            musicVolume = int.Parse(values[0]);
                            break;
                    }
                }
            }

            UI.musicVolumeSlider.value = musicVolume;
        }
        public static void SaveConfig() {
            string text = musicVolume.ToString();
            File.WriteAllText(CONFIG_FILE_PATH, text);
        }
        public static void ApplyConfig() {
            Game.music.Volume = musicVolume;
        }
    }
}
