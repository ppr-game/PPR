using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;

using DiscordRPC;

using NLog;

using PPR.GUI;
using PPR.GUI.Elements;
using PPR.Main.Levels;
using PPR.Properties;
using PPR.Rendering;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace PPR.Main {
    public enum StatsState { Pause, Fail, Pass }
    public enum Menu { Main, LevelSelect, Settings, KeybindsEditor, LastStats, Game }
    public class Game {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static StatsState statsState { get; private set; }
        static Menu _currentMenu = Menu.Main;
        public static Menu currentMenu {
            get => _currentMenu;
            set {
                if(Map.currentLevel != null && statsState == StatsState.Pause) {
                    if(_currentMenu == Menu.Game && value == Menu.LastStats) music.Pause();
                    else if(!editing && _currentMenu == Menu.LastStats && value == Menu.Game) {
                        music.Play();
                        if(!auto) {
                            music.PlayingOffset =
                                Time.FromMicroseconds(Math.Max(0, music.PlayingOffset.AsMicroseconds() - 3000000));
                            _prevFramePlayingOffset = music.PlayingOffset;
                        }
                    }
                }

                switch(value) {
                    case Menu.Game: {
                        if(auto) _usedAuto = true;
                        break;
                    }
                    case Menu.LastStats when !_usedAuto && statsState == StatsState.Pass && Map.currentLevel != null: {
                        string path = Path.Combine("scores", $"{Map.currentLevel.metadata.name}.txt");
                        string text = File.Exists(path) ? File.ReadAllText(path) : "";
                        text =
                            $"{Map.TextFromScore(new LevelScore(new Vector2(), score, accuracy, maxCombo, scores))}\n{text}";
                        _ = Directory.CreateDirectory("scores");
                        File.WriteAllText(path, text);
                        break;
                    }
                    case Menu.LastStats:
                        switch(statsState) {
                            case StatsState.Fail: failSound.Play();
                                break;
                            case StatsState.Pass: passSound.Play();
                                break;
                        }
                        break;
                }

                if((value == Menu.Main || value == Menu.LevelSelect) && music.Status == SoundStatus.Paused) music.Play();
                if(value == Menu.LevelSelect) {
                    GenerateLevelList();
                    string name = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(currentMusicPath));
                    if(name == "Default" || name == Settings.Default.audio) SwitchMusic();
                }
                _currentMenu = value;
                switch(value) {
                    case Menu.Main:
                        RPC.client.SetPresence(new RichPresence {
                            Details = "In main menu",
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.LevelSelect:
                        RPC.client.SetPresence(new RichPresence {
                            Details = $"Choosing what to {(editing ? "edit" : "play")}",
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.Game:
                        if(Map.currentLevel != null)
                            RPC.client.SetPresence(new RichPresence {
                                Details = editing ? "Editing" :
                                    auto ? "Watching" : "Playing",
                                State = Map.currentLevel.metadata.name,
                                Timestamps = Timestamps.Now
                            });
                        break;
                    case Menu.LastStats:
                        if(Map.currentLevel != null)
                            RPC.client.SetPresence(new RichPresence {
                                Details = editing ? "Paused Editing" :
                                    statsState == StatsState.Pause ? "Paused" :
                                    statsState == StatsState.Pass ? "Passed" : "Failed",
                                State = Map.currentLevel.metadata.name,
                                Timestamps = Timestamps.Now
                            });
                        break;
                }
            }
        }
        public static Time timeFromStart;
        static Time _prevPlayingOffset;
        static Time _prevFramePlayingOffset;
        static Time _interpolatedPlayingOffset;
        static float _offset;
        public static int roundedOffset;
        static float _steps;
        public static float steps {
            get => _steps;
            set {
                _steps = value;
                if(!editing) UI.progress = (int)(value / Map.currentLevel.metadata.maxStep * 80f);
            }
        }
        public static int roundedSteps;
        static float _prevSteps;
        public static int currentDirectionLayer;
        public static int currentBPM = 1;
        static float _currentSpeedSec = 60f;
        static float _absoluteCurrentSpeedSec = 60f;
        static readonly Random random = new Random();
        public static string currentMusicPath;
        public static string currentMusicName {
            get {
                string name = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(currentMusicPath));
                return name == "Default" || name == Settings.Default.audio ? "Waterflame - Cove" : name;
            }
        }
        public static Music music;
        public static Sound hitSound;
        public static Sound tickSound;
        public static Sound failSound;
        public static Sound passSound;
        public static Sound buttonClickSound;
        public static Sound sliderSound;
        public static int score;
        static int _health = 80;
        public static int health {
            get => _health;
            set {
                value = Math.Clamp(value, 0, 80);
                _health = value;
                UI.health = value;
            }
        }
        public static int accuracy = 100;
        public static int[] scores = new int[3]; // score / 5 = index
        public static int combo;
        public static int maxCombo;
        public static bool editing = false;
        public static bool auto = false;
        static bool _usedAuto;
        float _accumulator;
        public static void Start() {
            Settings.Default.PropertyChanged += PropertyChanged;
            ColorScheme.Reload();
            ReloadSounds();

            // TODO: Automatic settings list generation
            logger.Info("Current settings:");
            foreach(SettingsPropertyValue value in Settings.Default.PropertyValues) logger.Info($"{value.Name}={value.PropertyValue}");
            logger.Info("Current keybinds:");
            foreach(SettingsPropertyValue value in Bindings.Default.PropertyValues) logger.Info($"{value.Name}={value.PropertyValue}");

            RPC.Initialize();

            try {
                currentMusicPath = GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "mainMenu"));
                music = new Music(currentMusicPath);
            }
            catch(SFML.LoadingFailedException) {
                currentMusicPath = GetSoundFilePath(Path.Combine("resources", "audio", "Default", "mainMenu"));
                music = new Music(currentMusicPath);
            }

            music.Volume = Settings.Default.musicVolume;
            music.Play();
        }
        public static void UpdateSettings() {
            UI.RecreateButtons();
            UI.musicVolumeSlider.value = Settings.Default.musicVolume;
            UI.soundsVolumeSlider.value = Settings.Default.soundsVolume;
            UI.bloomSwitch.selected = Settings.Default.bloom;
            UI.showFpsSwitch.selected = Settings.Default.showFps;
            UI.fullscreenSwitch.selected = Settings.Default.fullscreen;
            UI.uppercaseSwitch.selected = Settings.Default.uppercaseNotes;

            LevelObject.linesColors = new Color[] {
                ColorScheme.GetColor("line_1_color"),
                ColorScheme.GetColor("line_2_color"),
                ColorScheme.GetColor("line_3_color"),
                ColorScheme.GetColor("line_4_color")
            };
            LevelObject.linesDarkColors = new Color[] {
                ColorScheme.GetColor("dark_line_1_color"),
                ColorScheme.GetColor("dark_line_2_color"),
                ColorScheme.GetColor("dark_line_3_color"),
                ColorScheme.GetColor("dark_line_4_color")
            };
            if(Map.currentLevel != null) foreach(LevelObject obj in Map.currentLevel.objects) obj.UpdateColors();
            LevelObject.speedColor = ColorScheme.GetColor("speed");
        }

        public static string GetSoundFilePath(string pathWithoutExtension) {
            string[] extensions = { ".ogg", ".wav", ".flac" };

            foreach(string extension in extensions) {
                string path = $"{pathWithoutExtension}{extension}";
                if(File.Exists(path)) return path;
            }

            return "";
        }

        static bool TryLoadSoundBuffer(string path, out SoundBuffer buffer) {
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
        static bool TryLoadSound(string path, out Sound sound) {
            if(TryLoadSoundBuffer(path, out SoundBuffer buffer)) {
                sound = new Sound(buffer);
                return true;
            }
            logger.Warn("Tried loading a sound at {0} but no success :(", path);
            sound = null;
            return false;
        }

        static void ReloadSounds() {
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "hit")), out hitSound) ||
                TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "hit")), out hitSound))
                hitSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "tick")), out tickSound) ||
                TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "tick")), out tickSound))
                tickSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "fail")), out failSound) ||
                TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "fail")), out failSound))
                failSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "pass")), out passSound) ||
                TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "pass")), out passSound))
                passSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "buttonClick")), out buttonClickSound) ||
                TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "buttonClick")), out buttonClickSound))
                buttonClickSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "slider")), out sliderSound) ||
                TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "slider")), out sliderSound))
                sliderSound.Volume = Settings.Default.soundsVolume;
        }
        public static void End() {
            logger.Info("Exiting");
            
            music.Stop();
            logger.Info("Stopped music");

            Settings.Default.Save();
            logger.Info("Saved settings");

            RPC.client.ClearPresence();
            RPC.client.Dispose();
            logger.Info("Removed Discord RPC");

            logger.Info("F to the logger");
            LogManager.Shutdown();

            Core.renderer.window.Close();
        }

        static void PropertyChanged(object caller, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "font": {
                    string[] fontMappingsLines = File.ReadAllLines(Path.Combine("resources", "fonts", Settings.Default.font, "mappings.txt"));
                    string[] fontSizeStr = fontMappingsLines[0].Split(',');
                    Core.renderer.fontSize = new Vector2(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));
                    Core.renderer.windowWidth = Core.renderer.width * Core.renderer.fontSize.x;
                    Core.renderer.windowHeight = Core.renderer.height * Core.renderer.fontSize.y;

                    BitmapFont font = new BitmapFont(new Image(Path.Combine("resources", "fonts", Settings.Default.font, "font.png")),
                        fontMappingsLines[1], Core.renderer.fontSize);
                    Core.renderer.text = new BitmapText(font, new Vector2(Core.renderer.width, Core.renderer.height)) {
                        backgroundColors = Core.renderer.backgroundColors,
                        foregroundColors = Core.renderer.foregroundColors,
                        text = Core.renderer.displayString
                    };

                    Core.renderer.UpdateWindow();
                    break;
                }
                case "colorScheme": ColorScheme.Reload();
                    break;
                case "fullscreen": Core.renderer.SetFullscreen(Settings.Default.fullscreen);
                    break;
                case "musicVolume": music.Volume = Settings.Default.musicVolume;
                    break;
                case "soundsVolume":
                    hitSound.Volume = Settings.Default.soundsVolume;
                    tickSound.Volume = Settings.Default.soundsVolume;
                    failSound.Volume = Settings.Default.soundsVolume;
                    passSound.Volume = Settings.Default.soundsVolume;
                    buttonClickSound.Volume = Settings.Default.soundsVolume;
                    sliderSound.Volume = Settings.Default.soundsVolume;
                    break;
                case "audio": ReloadSounds();
                    break;
            }
        }
        public static void LostFocus(object caller, EventArgs args) {
            if(currentMenu == Menu.Game) currentMenu = Menu.LastStats;
            music.Volume = 0;
        }
        public static void GainedFocus(object caller, EventArgs args) {
            music.Volume = Settings.Default.musicVolume;
        }
        public static void GameStart(string musicPath) {
            // Reset everything when we enter the level or bad things will happen

            statsState = StatsState.Pause;
            _usedAuto = auto;
            UI.progress = 80;
            _offset = 0;
            roundedOffset = 0;
            steps = 0;
            roundedSteps = 0;
            _prevSteps = 0;
            currentDirectionLayer = 0;
            currentBPM = Map.currentLevel.speeds[0].speed;
            _currentSpeedSec = 60f / currentBPM;
            timeFromStart = Time.Zero;
            _interpolatedPlayingOffset = Time.Zero;
            _prevFramePlayingOffset = Time.Zero;
            _prevPlayingOffset = Time.Zero;
            UI.health = 0;
            health = 80;
            score = 0;
            UI.prevScore = 0;
            scores = new int[3];
            accuracy = 100;
            combo = 0;
            maxCombo = 0;
            music.Stop();

            if(File.Exists(musicPath)) {
                Game.currentMusicPath = musicPath;
                music = new Music(musicPath) {
                    Volume = Settings.Default.musicVolume,
                    PlayingOffset = Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMs)
                };
                if(!editing) music.Play();
            }

            logger.Info("Entered level '{0}' by {1}", Map.currentLevel.metadata.name, Map.currentLevel.metadata.author);
        }
        public static void SwitchMusic() {
            string newPath = "";
            string[] paths = Directory.GetDirectories("levels");
            int index = 0;
            switch(paths.Length) {
                case 0:
                case 1: newPath = currentMusicPath;
                    break;
                default: {
                    while(currentMusicPath == newPath ||
                          Path.GetFileNameWithoutExtension(Path.GetDirectoryName(newPath)) == "_template" ||
                          string.IsNullOrEmpty(newPath)) {
                        index = random.Next(0, paths.Length);
                        newPath = GetSoundFilePath(Path.Combine(paths[index], "music"));
                    }
                    break;
                }
            }
            UI.currentLevelSelectIndex = index;
            currentMusicPath = newPath;
            music.Stop();
            music = new Music(currentMusicPath) {
                Volume = Settings.Default.musicVolume
            };
            music.Play();
        }
        public void Update() {
            if(currentMenu == Menu.Main && music.Status == SoundStatus.Stopped) SwitchMusic();
            if(currentMenu != Menu.Game) return;

            // If we're in the editor, update logic every frame
            // instead of using a fixed time step to remove bugs that happen when scrolling
            if(editing) {
                _interpolatedPlayingOffset = music.PlayingOffset;
                FixedUpdate();
            }
            else {
                float fixedDeltaTime = _absoluteCurrentSpeedSec / 16f;

                _accumulator += Core.deltaTime;
                float totalTimesToExec = 0f;
                if(_accumulator >= fixedDeltaTime)
                    totalTimesToExec = MathF.Ceiling((_accumulator - fixedDeltaTime) / fixedDeltaTime);
                while(_accumulator >= fixedDeltaTime) {
                    float interpT = 1f - MathF.Ceiling((_accumulator - fixedDeltaTime) / fixedDeltaTime) /
                        totalTimesToExec;
                    _interpolatedPlayingOffset =
                        music.PlayingOffset * interpT + _prevFramePlayingOffset * (1f - interpT);
                    //if(interpT > 0f) logger.Debug(interpT);
                    FixedUpdate();
                    _accumulator -= fixedDeltaTime;
                }
            }
            _prevFramePlayingOffset = music.PlayingOffset;
        }

        static void FixedUpdate() {
            if(_interpolatedPlayingOffset != _prevPlayingOffset) {
                timeFromStart = _interpolatedPlayingOffset - Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMs);
                steps = MillisecondsToSteps(timeFromStart.AsMicroseconds() / 1000f);
                if(music.Status != SoundStatus.Playing) steps = MathF.Round(steps);
                _offset = StepsToOffset(steps);
            }
            _prevPlayingOffset = _interpolatedPlayingOffset;

            //if(steps - prevSteps > 1f) logger.Warn("Lag detected: steps increased too quickly ({0})", steps - prevSteps);

            if(MathF.Floor(_prevSteps) != MathF.Floor(steps)) {
                roundedSteps = (int)MathF.Round(steps);
                roundedOffset = (int)MathF.Round(_offset);
                RecalculatePosition();
            }
            _prevSteps = steps;

            if(editing) {
                float initialOffset = Map.currentLevel.metadata.initialOffsetMs / 1000f;
                float duration = music.Duration.AsSeconds() - initialOffset;
                if(Core.renderer.mousePosition.y == 0 && Core.renderer.leftButtonPressed) {
                    float mouseProgress = Math.Clamp(Core.renderer.mousePositionF.X / 80f, 0f, 1f);
                    music.PlayingOffset = Time.FromSeconds(duration * mouseProgress + initialOffset);
                    timeFromStart = music.PlayingOffset - Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMs);
                    steps = MathF.Round(MillisecondsToSteps(timeFromStart.AsMilliseconds()));
                    _offset = StepsToOffset(steps);
                }
                UI.progress = (int)(music.PlayingOffset.AsSeconds() / duration * 80f);
            }

            statsState = health > 0 ? Map.currentLevel.objects.Count(obj => !obj.ignore) > 0 ? StatsState.Pause :
                StatsState.Pass : StatsState.Fail;

            Map.SimulateAll();

            if(statsState != StatsState.Pause) currentMenu = Menu.LastStats;
        }
        public static void UpdateTime() {
            long useMicrosecs = (long)((MathF.Round(StepsToMilliseconds(steps)) + Map.currentLevel.metadata.initialOffsetMs) * 1000f);
            music.PlayingOffset = Time.FromMicroseconds(useMicrosecs);
        }

        static void UpdateSpeeds() {
            foreach (LevelSpeed speed in Map.currentLevel.speeds)
                if(speed.step <= steps) {
                    currentBPM = speed.speed;
                    _currentSpeedSec = 60f / currentBPM;
                    _absoluteCurrentSpeedSec = Math.Abs(_currentSpeedSec);
                    currentDirectionLayer = StepsToDirectionLayer(roundedSteps);

                    int rangeModifier = (int)(Math.Abs(currentBPM) / 600f);
                    LevelObject.perfectRange = 1 + rangeModifier;
                    LevelObject.hitRange = LevelObject.perfectRange + (int)(rangeModifier / 2f);
                    LevelObject.missRange = LevelObject.hitRange + 1;
                }
                else break;
        }
        public static void RecalculatePosition() {
            UpdateSpeeds();
            Map.StepAll();
            //logger.Debug(previousSpeedMS + " , " + currentSpeedMS);
        }

        public static char GetNoteBinding(Keyboard.Key key) {
            return key switch
            {
                Keyboard.Key.Num1 => '1',
                Keyboard.Key.Num2 => '2',
                Keyboard.Key.Num3 => '3',
                Keyboard.Key.Num4 => '4',
                Keyboard.Key.Num5 => '5',
                Keyboard.Key.Num6 => '6',
                Keyboard.Key.Num7 => '7',
                Keyboard.Key.Num8 => '8',
                Keyboard.Key.Num9 => '9',
                Keyboard.Key.Num0 => '0',
                Keyboard.Key.Hyphen => '-',
                Keyboard.Key.Equal => '=',
                Keyboard.Key.Q => 'q',
                Keyboard.Key.W => 'w',
                Keyboard.Key.E => 'e',
                Keyboard.Key.R => 'r',
                Keyboard.Key.T => 't',
                Keyboard.Key.Y => 'y',
                Keyboard.Key.U => 'u',
                Keyboard.Key.I => 'i',
                Keyboard.Key.O => 'o',
                Keyboard.Key.P => 'p',
                Keyboard.Key.LBracket => '[',
                Keyboard.Key.RBracket => ']',
                Keyboard.Key.A => 'a',
                Keyboard.Key.S => 's',
                Keyboard.Key.D => 'd',
                Keyboard.Key.F => 'f',
                Keyboard.Key.G => 'g',
                Keyboard.Key.H => 'h',
                Keyboard.Key.J => 'j',
                Keyboard.Key.K => 'k',
                Keyboard.Key.L => 'l',
                Keyboard.Key.Semicolon => ';',
                Keyboard.Key.Quote => '\'',
                Keyboard.Key.Z => 'z',
                Keyboard.Key.X => 'x',
                Keyboard.Key.C => 'c',
                Keyboard.Key.V => 'v',
                Keyboard.Key.B => 'b',
                Keyboard.Key.N => 'n',
                Keyboard.Key.M => 'm',
                Keyboard.Key.Comma => ',',
                Keyboard.Key.Period => '.',
                Keyboard.Key.Slash => '/',
                _ => '\0'
            };
        }

        static void ChangeSpeed(int delta) {
            // Create a new speed if we don't have a speed at the current position
            List<int> flooredSpeedsSteps = Map.currentLevel.speeds.Select(speed => speed.step).ToList();
            if(!flooredSpeedsSteps.Contains((int)steps)) {
                int speedIndex = 0;
                for(int i = 0; i < Map.currentLevel.speeds.Count; i++)
                    if(Map.currentLevel.speeds[i].step <= steps) speedIndex = i;
                Map.currentLevel.speeds.Add(new LevelSpeed(Map.currentLevel.speeds[speedIndex].speed, (int)steps));
                Map.currentLevel.speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
                //Map.currentLevel.speeds = SortLevelSpeeds(Map.currentLevel.speeds);
            }

            // Get the index of the speed we want to change
            flooredSpeedsSteps = Map.currentLevel.speeds.Select(speed => speed.step).ToList();
            int index = flooredSpeedsSteps.IndexOf((int)steps);

            Map.currentLevel.speeds[index].speed += delta;
            
            // Remove the current speed if it's the same as the previous one
            if(index >= 1 && Map.currentLevel.speeds[index].speed == Map.currentLevel.speeds[index - 1].speed) Map.currentLevel.speeds.RemoveAt(index);

            // Recreate the objects that show the speeds
            List<LevelObject> speedObjects = Map.currentLevel.objects.FindAll(obj => obj.character == LevelObject.SPEED_CHAR);
            foreach(LevelObject obj in speedObjects) obj.toDestroy = true;
            foreach(LevelSpeed speed in Map.currentLevel.speeds) Map.currentLevel.objects.Add(new LevelObject(LevelObject.SPEED_CHAR, speed.step, Map.currentLevel.speeds));
        }
        public static void KeyPressed(object caller, KeyEventArgs key) {
            // Back
            if(Bindings.Default.back.IsPressed(key) && currentMenu != Menu.LevelSelect)
                currentMenu = currentMenu switch {
                    Menu.Game => Menu.LastStats,
                    Menu.LastStats => statsState == StatsState.Pause ? Menu.Game : Menu.LevelSelect,
                    Menu.KeybindsEditor => Menu.Settings,
                    _ => Menu.Main
                };
            if(currentMenu != Menu.Game) return;
            if(editing) {
                char character = GetNoteBinding(key.Code);
                if(character == '\0') {
                    // Erase
                    if(Bindings.Default.erase.IsPressed(key)) {
                        List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.step == (int)steps &&
                                                                                            obj.character != LevelObject.SPEED_CHAR);
                        foreach(LevelObject obj in objects) obj.toDestroy = true;
                    }

                    // Speed
                    else if(Bindings.Default.speedUpSlow.IsPressed(key)) ChangeSpeed(1);
                    else if(Bindings.Default.speedDownSlow.IsPressed(key)) ChangeSpeed(-1);
                    else if(Bindings.Default.speedUp.IsPressed(key)) ChangeSpeed(10);
                    else if(Bindings.Default.speedDown.IsPressed(key)) ChangeSpeed(-10);

                    // Lines
                    else if(Bindings.Default.linesFrequencyUp.IsPressed(key))
                        Map.currentLevel.metadata.linesFrequency++;
                    else if(Bindings.Default.linesFrequencyDown.IsPressed(key))
                        Map.currentLevel.metadata.linesFrequency--;

                    // HP Drain/Restorage
                    else if(Bindings.Default.hpRestorageUp.IsPressed(key))
                        Map.currentLevel.metadata.hpRestorage++;
                    else if(Bindings.Default.hpRestorageDown.IsPressed(key))
                        Map.currentLevel.metadata.hpRestorage--;
                    else if(Bindings.Default.hpDrainUp.IsPressed(key))
                        Map.currentLevel.metadata.hpDrain++;
                    else if(Bindings.Default.hpDrainDown.IsPressed(key))
                        Map.currentLevel.metadata.hpDrain--;

                    // Initial offset
                    else if(Bindings.Default.initialOffsetUpBoost.IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs += 10;
                    else if(Bindings.Default.initialOffsetDownBoost.IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs -= 10;
                    else if(Bindings.Default.initialOffsetUp.IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs++;
                    else if(Bindings.Default.initialOffsetDown.IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs--;

                    // Fast scroll
                    else if(Bindings.Default.fastScrollUp.IsPressed(key)) ScrollTime(10);
                    else if(Bindings.Default.fastScrollDown.IsPressed(key)) ScrollTime(-10);
                }
                else {
                    if(Map.currentLevel.objects.FindAll(obj => obj.character == character && obj.step == (int)steps).Count <= 0) {
                        Map.currentLevel.objects.Add(new LevelObject(character, (int)steps, Map.currentLevel.speeds));
                        if(key.Shift) {
                            character = LevelObject.HOLD_CHAR;
                            Map.currentLevel.objects.Add(new LevelObject(character, (int)steps, Map.currentLevel.speeds, Map.currentLevel.objects));
                        }
                    }
                }

                RecalculatePosition();
            }
            else if(!auto)
                for(int step = roundedSteps - LevelObject.missRange; StepPassedLine(step, -LevelObject.missRange); step++)
                    if(CheckLine(step)) break;
        }

        static bool CheckLine(int step) {
            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.character != LevelObject.SPEED_CHAR &&
                        obj.character != LevelObject.HOLD_CHAR &&
                        !obj.removed &&
                        !obj.toDestroy &&
                        !obj.ignore &&
                        obj.step == step);
            foreach(LevelObject obj in objects) {
                obj.CheckPress();
                if(obj.removed || obj.toDestroy) return true;
            }
            return false;
        }

        public static void MouseWheelScrolled(object caller, MouseWheelScrollEventArgs scroll) {
            switch(currentMenu) {
                case Menu.LevelSelect: {
                    Vector2 mousePos = Core.renderer.mousePosition;
                    if(mousePos.y >= 12 && mousePos.y <= 49) {
                        if(mousePos.x >= 28 && mousePos.x <= 51) {
                            if(scroll.Delta > 0 && UI.levelSelectLevels.First().position.y >= 12) return;
                            if(scroll.Delta < 0 && UI.levelSelectLevels.Last().position.y <= 49) return;
                            foreach(Button button in UI.levelSelectLevels) button.position.y += (int)scroll.Delta;
                        }
                        else if(mousePos.x >= 1 && mousePos.x <= 26 &&
                                UI.levelSelectScores[UI.currentLevelSelectIndex] != null &&
                                UI.levelSelectScores[UI.currentLevelSelectIndex].Count > 0) {
                            if(scroll.Delta > 0 && UI.levelSelectScores[UI.currentLevelSelectIndex].First().scorePosition.y >= 12) return;
                            if(scroll.Delta < 0 && UI.levelSelectScores[UI.currentLevelSelectIndex].Last().scoresPosition.y <= 49) return;
                            for(int i = 0; i < UI.levelSelectScores[UI.currentLevelSelectIndex].Count; i++) {
                                int increment = (int)scroll.Delta;
                                LevelScore score = UI.levelSelectScores[UI.currentLevelSelectIndex][i];
                                score.scorePosition.y += increment;
                                score.accComboPosition.y += increment;
                                score.accComboDividerPosition.y += increment;
                                score.maxComboPosition.y += increment;
                                score.scoresPosition.y += increment;
                                score.linePosition.y += increment;
                                UI.levelSelectScores[UI.currentLevelSelectIndex][i] = score;
                            }
                        }
                    }

                    break;
                }
                case Menu.Game when editing: ScrollTime((int)scroll.Delta);
                    break;
            }
        }

        static void ScrollTime(int delta) {
            steps = Math.Clamp(steps + delta, 0, MillisecondsToSteps(music.Duration.AsMicroseconds() / 1000f));
            UpdateTime();
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static float StepsToMilliseconds(float steps) {
            return StepsToMilliseconds(steps, Map.currentLevel.speeds);
        }
        public static float StepsToMilliseconds(float steps, List<LevelSpeed> sortedSpeeds) {
            float useSteps = steps;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(sortedSpeeds[i].step <= useSteps) speedIndex = i;
            float time = 0;
            for(int i = 0; i <= speedIndex; i++)
                if(i == speedIndex) time += (useSteps - sortedSpeeds[i].step) * (60000f / Math.Abs(sortedSpeeds[i].speed));
                else time += (sortedSpeeds[i + 1].step - sortedSpeeds[i].step) * (60000f / Math.Abs(sortedSpeeds[i].speed));
            return time;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static float MillisecondsToSteps(float time) {
            return MillisecondsToSteps(time, Map.currentLevel.speeds);
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static float MillisecondsToSteps(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(StepsToMilliseconds(sortedSpeeds[i].step) <= useTime) speedIndex = i;
                else break;
            float steps = 0;
            for(int i = 0; i <= speedIndex; i++)
                if(i == speedIndex) steps += useTime / (60000f / Math.Abs(sortedSpeeds[i].speed));
                else {
                    int stepsIncrement = sortedSpeeds[i + 1].step - sortedSpeeds[i].step;
                    steps += stepsIncrement;
                    useTime -= stepsIncrement * (60000f / Math.Abs(sortedSpeeds[i].speed));
                }

            return steps;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static float StepsToOffset(float steps) {
            return StepsToOffset(steps, Map.currentLevel.speeds);
        }
        public static float StepsToOffset(float steps, List<LevelSpeed> sortedSpeeds) {
            float useSteps = steps;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(sortedSpeeds[i].step <= useSteps) speedIndex = i;
                else break;
            float offset = 0;
            for(int i = 0; i <= speedIndex; i++)
                if(i == speedIndex) offset += useSteps * MathF.Sign(sortedSpeeds[i].speed);
                else {
                    int stepsDecrement = sortedSpeeds[i + 1].step - sortedSpeeds[i].step;
                    offset += stepsDecrement * MathF.Sign(sortedSpeeds[i].speed);
                    useSteps -= stepsDecrement;
                }

            return offset;
        }
        // ReSharper disable once MemberCanBePrivate.Global
        public static int StepsToDirectionLayer(float steps) {
            return StepsToDirectionLayer(steps, Map.currentLevel.speeds);
        }
        public static int StepsToDirectionLayer(float steps, List<LevelSpeed> sortedSpeeds) {
            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(sortedSpeeds[i].step <= steps) speedIndex = i;
                else break;
            int directionLayer = 0;
            for(int i = 1; i <= speedIndex; i++)
                if(MathF.Sign(sortedSpeeds[i].speed) != MathF.Sign(sortedSpeeds[i - 1].speed)) directionLayer++;
            return directionLayer;
        }
        public static bool StepPassedLine(int step, int lineOffset = 0) {
            return roundedSteps >= step + lineOffset;
        }

        static void GenerateLevelList() {
            string[] directories = Directory.GetDirectories("levels");
            List<Button> buttons = new List<Button>();
            List<LevelMetadata?> metadatas = new List<LevelMetadata?>();
            for(int i = 0; i < directories.Length; i++) {
                string name = Path.GetFileName(directories[i]);
                if(name == "_template") continue;
                buttons.Add(new Button(new Vector2(25, 12 + i), name, "levelSelect.level", 30));
                metadatas.Add(new LevelMetadata(File.ReadAllLines(Path.Combine(directories[i], "level.txt")), name));
                logger.Info("Loaded metadata for level {0}", name);
            }
            UI.levelSelectLevels = buttons;
            UI.levelSelectMetadatas = metadatas;

            List<List<LevelScore>> scores = new List<List<LevelScore>> {
                Capacity = buttons.Count
            };
            if(Directory.Exists("scores"))
                for(int i = 0; i < directories.Length; i++) {
                    string name = Path.GetFileName(directories[i]);
                    if(name == "_template") continue;
                    string scoresPath = Path.Combine("scores", $"{name}.txt");
                    if(File.Exists(scoresPath)) {
                        scores.Add(Map.ScoresFromLines(File.ReadAllLines(scoresPath), UI.scoresPos));
                        logger.Info("Loaded scores for level {0}, total scores count: {1}", name, scores[i].Count);
                    }
                    else scores.Add(null);
                }

            UI.levelSelectScores = scores;

            logger.Info("Loaded levels, total level count: {0}", buttons.Count);
        }
        public static void RecalculateAccuracy() {
            float sum = scores[0] + scores[1] + scores[2];
            float mulSum = scores[1] * 0.5f + scores[2];
            accuracy = (int)MathF.Floor(mulSum / sum * 100f);
        }
        public static Color GetAccuracyColor(int accuracy) {
            return accuracy >= 100 ? ColorScheme.GetColor("perfect_hit") :
                accuracy >= 70 ? ColorScheme.GetColor("hit") : ColorScheme.GetColor("miss");
        }
        public static Color GetComboColor(int accuracy, int misses) {
            return accuracy >= 100 ? ColorScheme.GetColor("perfect_hit") :
                misses <= 0 ? ColorScheme.GetColor("hit") : ColorScheme.GetColor("score");
        }
    }
}
