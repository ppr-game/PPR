using System;
using System.IO;

using PPR.Core;
using PPR.GUI;

namespace PPR.Configuration {
    public static class Config {
        public const string CONFIG_FILE_PATH = "config.txt";
        public static int musicVolume {
            set => Game.music.Volume = value;
            get => (int)MathF.Round(Game.music.Volume);
        }

        public static void LoadConfig() {
            string text = File.ReadAllLines(CONFIG_FILE_PATH)[0];
            string[] values = text.Split(',');
            for(int i = 0; i < values.Length; i++) {
                switch(i) {
                    case 0:
                        musicVolume = int.Parse(values[0]);
                        break;
                }
            }

            UI.musicVolumeSlider.value = musicVolume;
        }
        public static void SaveConfig() {
            /*string text = File.ReadAllLines(CONFIG_FILE_PATH)[0];
            string[] values = text.Split(',');
            for(int i = 0; i < values.Length; i++) {
                switch(i) {
                    case 0:
                        musicVolume = int.Parse(values[0]);
                        break;
                }
            }*/
            string text = musicVolume.ToString();
            File.WriteAllText(CONFIG_FILE_PATH, text);
        }
    }
}
