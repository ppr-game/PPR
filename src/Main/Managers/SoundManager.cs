using System;
using System.IO;
using System.Linq;

using MoonSharp.Interpreter;

using NLog;

using PPR.GUI;
using PPR.Main.Levels;
using PPR.Properties;

using SFML.Audio;

namespace PPR.Main.Managers {
    public static class SoundManager {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Random random = new Random();
        
        private static string _currentMusicPath;

        public static string currentMusicPath {
            get => _currentMusicPath;
            set {
                _currentMusicPath = value;
                if(currentMusicName == "Waterflame - Cove") return;
                string[] menusAnimLines = Array.Empty<string>();
                float prevDiff = -1f;
                foreach(string file in Directory.GetFiles(Path.GetDirectoryName(value)!)) {
                    if(!file.EndsWith(".txt")) continue;
                    string currDiffName = Path.GetFileNameWithoutExtension(file);
                    string[] lines = File.ReadAllLines(Path.Join(Path.GetDirectoryName(value)!, $"{currDiffName}.txt"));
                    if(!Level.IsLevelValid(lines)) continue;
                    LevelMetadata metadata = new LevelMetadata(lines, currentMusicName, currDiffName);
                    if(metadata.difficulty > prevDiff) menusAnimLines = lines;
                    prevDiff = metadata.difficulty;
                }
                Game.menusAnimInitialOffset = Game.GetInitialOffset(menusAnimLines);
                Game.menusAnimSpeeds = Game.GetSpeeds(menusAnimLines);
            }
        }

        public static string currentMusicName {
            get {
                string name = Path.GetFileName(Path.GetDirectoryName(currentMusicPath));
                return name == "Default" || name == Settings.GetPath("audio") ? "Waterflame - Cove" : name;
            }
        }

        public static Music music { get; set; }
        private static Sound _hitSound;
        private static Sound _holdSound;
        private static Sound _failSound;
        private static Sound _passSound;
        private static Sound _clickSound;
        private static Sound _sliderSound;
        public static string GetSoundFilePath(string pathWithoutExtension) {
            string[] extensions = { ".ogg", ".wav", ".flac" };

            foreach(string extension in extensions) {
                string path = $"{pathWithoutExtension}{extension}";
                if(File.Exists(path)) return path;
            }

            return "";
        }
        
        private static bool TryLoadSoundBuffer(string path, out SoundBuffer buffer) {
            try {
                buffer = new SoundBuffer(path);
            }
            catch(Exception ex) {
                logger.Error(ex);
                buffer = null;
                return false;
            }
            return true;
        }
        
        private static bool TryLoadSound(string path, out Sound sound) {
            if(TryLoadSoundBuffer(path, out SoundBuffer buffer)) {
                sound = new Sound(buffer);
                return true;
            }
            logger.Warn("Tried loading a sound at {0} but no success :(", path);
            sound = null;
            return false;
        }
        
        public static void UpdateSoundsVolume() {
            int volume = Settings.GetInt("soundsVolume");
            _hitSound.Volume = volume;
            _holdSound.Volume = volume;
            _failSound.Volume = volume;
            _passSound.Volume = volume;
            _clickSound.Volume = volume;
            _sliderSound.Volume = volume;
        }
        
        public static void ReloadSounds() {
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "hit")), out _hitSound) || TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "hit")), out _hitSound)) _hitSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "tick")), out _holdSound) || TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "tick")), out _holdSound)) _holdSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "fail")), out _failSound) || TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "fail")), out _failSound)) _failSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "pass")), out _passSound) || TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "pass")), out _passSound)) _passSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "buttonClick")), out _clickSound) || TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "buttonClick")), out _clickSound)) _clickSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "slider")), out _sliderSound) || TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "slider")), out _sliderSound)) _sliderSound.Volume = Settings.GetInt("soundsVolume");
        }
        
        public static void PlaySound(SoundType type) {
            switch(type) {
                case SoundType.Hit:
                    _hitSound.Play();
                    break;
                case SoundType.Hold:
                    _holdSound.Play();
                    break;
                case SoundType.Pass:
                    _passSound.Play();
                    break;
                case SoundType.Fail:
                    _failSound.Play();
                    break;
                case SoundType.Click:
                    _clickSound.Play();
                    break;
                case SoundType.Slider:
                    _sliderSound.Play();
                    break;
            }
        }
        
        public static void SwitchMusic() {
            if(Game.exiting) return;
            
            string newPath = currentMusicPath;
            string[] paths = Directory.GetDirectories("levels")
                .Where(path => Path.GetFileName(Path.GetDirectoryName(path)) != "_template")
                .Select(path => GetSoundFilePath(Path.Join(path, "music")))
                .Where(path => !string.IsNullOrEmpty(path)).ToArray();
            switch(paths.Length) {
                case 0 when music.Status != SoundStatus.Stopped: return;
                case 0 when music.Status == SoundStatus.Stopped:
                case 1: newPath = currentMusicPath;
                    break;
                default: {
                    while(currentMusicPath == newPath) newPath = paths[random.Next(0, paths.Length)];
                    break;
                }
            }
            UI.currSelectedLevel = Path.GetFileName(Path.GetDirectoryName(newPath));
            currentMusicPath = newPath;
            StopMusic();
            music = new Music(currentMusicPath) {
                Volume = Core.renderer.window.HasFocus() ? Settings.GetInt("musicVolume") : 0f
            };
            PlayMusic();
        }

        public static void PlayMusic() {
            music.Play();
            Lua.SendMessageToConsoles("onMusicStatusChange");
        }
        
        public static void PauseMusic() {
            music.Pause();
            Lua.SendMessageToConsoles("onMusicStatusChange");
        }
        
        public static void StopMusic() {
            music.Stop();
            Lua.SendMessageToConsoles("onMusicStatusChange");
        }
    }
}
