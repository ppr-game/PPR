using System.IO;

using PPR.Properties;

using SFML.Audio;

namespace PPR.Lua.API.Console.Main.Managers {
    public class SoundManager {
        public static string currentMusicName => PPR.Main.Managers.SoundManager.currentMusicName;
        public static SoundStatus musicStatus => PPR.Main.Managers.SoundManager.music.Status;
        public static void PlayMusic() => PPR.Main.Managers.SoundManager.PlayMusic();
        public static void PauseMusic() => PPR.Main.Managers.SoundManager.PauseMusic();
        public static void StopMusic() => PPR.Main.Managers.SoundManager.StopMusic();
        public static void SwitchMusic() => PPR.Main.Managers.SoundManager.SwitchMusic();

        public static void LoadLevelMusic(string levelName) {
            string levelPath = Path.Join("levels", levelName);
            string musicPath = PPR.Main.Managers.SoundManager.GetSoundFilePath(Path.Join(levelPath, "music"));
            if(!File.Exists(musicPath))
                throw new FileNotFoundException($"Music file for level {levelName} was not found.");
            PPR.Main.Managers.SoundManager.currentMusicPath = musicPath;
            StopMusic();
            PPR.Main.Managers.SoundManager.music = new Music(musicPath) { Volume = Settings.GetInt("musicVolume") };
            PlayMusic();
        }
    }
}
