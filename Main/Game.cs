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
    public enum Menu { Main, LevelSelect, Settings, LastStats, Game }
    public class Game {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        static Menu _currentMenu = Menu.Main;
        public static Menu currentMenu {
            get => _currentMenu;
            set {
                if(Map.currentLevel != null && Map.currentLevel.objects.Count > 0 && health > 0) {
                    if(_currentMenu == Menu.Game && value == Menu.LastStats) { // Pause
                        music.Pause();
                    }
                    else if(!editing && _currentMenu == Menu.LastStats && value == Menu.Game) { // Unpause
                        music.Play();
                    }
                }
                if(value == Menu.Game) {
                    if(auto) usedAuto = true;
                }
                if(value == Menu.LastStats && !usedAuto && Map.currentLevel.objects.Count <= 0 && health > 0) {
                    string path = Path.Combine("scores", Map.currentLevel.metadata.name + ".txt");
                    string text = File.Exists(path) ? File.ReadAllText(path) : "";
                    text = Map.TextFromScore(new LevelScore(Vector2.zero, score, accuracy, maxCombo, scores)) + "\n" + text;
                    _ = Directory.CreateDirectory("scores");
                    File.WriteAllText(path, text);
                }
                if((value == Menu.Main || value == Menu.LevelSelect) && music.Status == SoundStatus.Paused) {
                    music.Play();
                }
                if(value == Menu.LevelSelect) {
                    GenerateLevelList();
                }
                _currentMenu = value;
                switch(value) {
                    case Menu.Main:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = "In main menu",
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.LevelSelect:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = "Choosing what to " + (editing ? "edit" : "play"),
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.Game:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = editing ? "Editing" : auto ? "Watching" : "Playing",
                            State = Map.currentLevel.metadata.name,
                            Timestamps = Timestamps.Now
                        });
                        break;
                    case Menu.LastStats:
                        RPC.client.SetPresence(new RichPresence() {
                            Details = "Looking at statistics",
                            State = Map.currentLevel.metadata.name,
                            Timestamps = Timestamps.Now
                        });
                        break;
                }
            }
        }
        public static Time time;
        static float _offset = 0f;
        public static float offset {
            set {
                _offset = value;
                if(!editing) UI.progress = (int)(value / Map.currentLevel.metadata.maxOffset * 80f);
            }
            get => _offset;
        }
        public static int roundedOffset = 0;
        static int prevRoundedOffset = 0;
        public static float prevOffset = 0f;
        public static int currentBPM = 1;
        public static Music music = new Music(Path.Combine("resources", "audio", "mainMenu.ogg"));
        public static SoundBuffer hitsoundBuffer;
        public static SoundBuffer ticksoundBuffer;
        public static Sound hitsound;
        public static Sound ticksound;
        public static int score = 0;
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
        public static int combo = 0;
        public static int maxCombo = 0;
        public static bool editing = false;
        public static bool auto = false;
        public static bool usedAuto = false;
        public void Start() {
            ColorScheme.Reload();
            ReloadSounds();

            // TODO: Automatic settings list generation
            logger.Info("Current settings:");
            foreach(SettingsPropertyValue value in Settings.Default.PropertyValues) {
                logger.Info(value.Name + "=" + value.PropertyValue);
            }

            RPC.Initialize();

            music.Volume = Settings.Default.musicVolume;
            music.Loop = true;
            music.Play();
        }
        public void ReloadSettings() {
            Settings.Default.PropertyChanged -= PropertyChanged;

            Settings.Default.Reload();
            UI.musicVolumeSlider.value = Settings.Default.musicVolume;
            UI.soundsVolumeSlider.value = Settings.Default.soundsVolume;
            UI.bloomSwitch.selected = Settings.Default.bloom;
            UI.showFpsSwitch.selected = Settings.Default.showFps;
            UI.fullscreenSwitch.selected = Settings.Default.fullscreen;

            Core.renderer.SetFullscreen(Settings.Default.fullscreen);

            Settings.Default.PropertyChanged += PropertyChanged;
        }
        public void ReloadSounds() {
            hitsoundBuffer = new SoundBuffer(Path.Combine("resources", "audio", Settings.Default.audio, "hitsound.wav"));
            ticksoundBuffer = new SoundBuffer(Path.Combine("resources", "audio", Settings.Default.audio, "ticksound.wav"));
            hitsound = new Sound(hitsoundBuffer);
            ticksound = new Sound(ticksoundBuffer);
            hitsound.Volume = Settings.Default.soundsVolume;
            ticksound.Volume = Settings.Default.soundsVolume;
        }
        public void End() {
            logger.Info("Exiting");

            Settings.Default.Save();

            RPC.client.ClearPresence();
            RPC.client.Dispose();

            LogManager.Shutdown();

            Core.renderer.window.Close();
        }
        public void Update() {
            if(currentMenu != Menu.Game) return;

            if(MathF.Floor(prevOffset) != MathF.Floor(offset)) {
                prevRoundedOffset = roundedOffset;
                roundedOffset = (int)MathF.Round(offset);
                RecalculatePosition();
            }

            prevOffset = offset;

            if(music.Status == SoundStatus.Playing) {
                offset = MillisecondsToOffset(music.PlayingOffset.AsMilliseconds() + Map.currentLevel.metadata.initialOffsetMS, Map.currentLevel.speeds);
                if(roundedOffset - prevRoundedOffset > 1)
                    logger.Warn("Lag detected: the offset changed too quickly ({0}), current speed: {1} BPM, {2} ms",
                        roundedOffset - prevRoundedOffset, currentBPM, 60000f / currentBPM);
            }
            if(editing) UI.progress = (int)((music.PlayingOffset.AsMilliseconds() + Map.currentLevel.metadata.initialOffsetMS) * 1000 / music.Duration.AsSeconds() * 80f);
        }
        public static void GameStart(string musicPath) {
            usedAuto = auto;
            UI.progress = 80;
            offset = 0;
            roundedOffset = 0;
            prevOffset = 0;
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
                music = new Music(musicPath) {
                    Volume = Settings.Default.musicVolume
                };
                if(!editing) {
                    music.PlayingOffset = Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMS);
                    music.Play();
                }
            }

            logger.Info("Entered level '{0}' by {1}", Map.currentLevel.metadata.name, Map.currentLevel.metadata.author);
        }
        public static void RecalculatePosition() {
            for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                if(Map.currentLevel.speeds[i].offset <= offset) {
                    currentBPM = Map.currentLevel.speeds[i].speed;
                }
                else break;
            }
            Map.StepAll();
        }
        public void PropertyChanged(object caller, PropertyChangedEventArgs e) {
            if(e.PropertyName == "font") {
                string[] fontMappingsLines = File.ReadAllLines(Path.Combine("resources", "fonts", Settings.Default.font, "mappings.txt"));
                string[] fontSizeStr = fontMappingsLines[0].Split(',');
                //Vector2 oldFontSize = new Vector2(Core.renderer.fontSize);
                Core.renderer.fontSize = new Vector2(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));
                //Vector2f fontSizeChange = new Vector2f((float)Core.renderer.fontSize.x / oldFontSize.x, (float)Core.renderer.fontSize.y / oldFontSize.y);
                Core.renderer.windowWidth = Core.renderer.width * Core.renderer.fontSize.x;
                Core.renderer.windowHeight = Core.renderer.height * Core.renderer.fontSize.y;
                Core.renderer.UpdateWindow();

                //Mouse.SetPosition(new Vector2i((int)(Mouse.GetPosition(Core.renderer.window).X * fontSizeChange.X),
                //    (int)(Mouse.GetPosition(Core.renderer.window).Y * fontSizeChange.Y)), Core.renderer.window);

                BitmapFont font = new BitmapFont(new Image(Path.Combine("resources", "fonts", Settings.Default.font, "font.png")), fontMappingsLines[1], Core.renderer.fontSize);
                Core.renderer.text = new BitmapText(font, new Vector2(Core.renderer.width, Core.renderer.height)) {
                    backgroundColors = Core.renderer.backgroundColors,
                    foregroundColors = Core.renderer.foregroundColors,
                    text = Core.renderer.displayString
                };
            }
            else if(e.PropertyName == "colorScheme") {
                ColorScheme.Reload();
            }
            else if(e.PropertyName == "fullscreen") {
                Core.renderer.SetFullscreen(Settings.Default.fullscreen);
            }
            else if(e.PropertyName == "musicVolume") {
                music.Volume = Settings.Default.musicVolume;
            }
            else if(e.PropertyName == "soundsVolume") {
                hitsound.Volume = Settings.Default.soundsVolume;
                ticksound.Volume = Settings.Default.soundsVolume;
            }
            else if(e.PropertyName == "audio") {
                ReloadSounds();
            }
        }
        public static void GenerateLevelList() {
            string[] directories = Directory.GetDirectories("levels");
            List<Button> buttons = new List<Button>();
            List<LevelMetadata?> metadatas = new List<LevelMetadata?>();
            for(int i = 0; i < directories.Length; i++) {
                string name = Path.GetFileName(directories[i]);
                if(name == "_template") continue;
                buttons.Add(new Button(new Vector2(25, 12 + i), name, 30, ColorScheme.black, ColorScheme.white, ColorScheme.white));
                metadatas.Add(new LevelMetadata(File.ReadAllLines(Path.Combine(directories[i], "level.txt")), name));
                logger.Info("Loaded metadata for level {0}", name);
            }
            UI.levelSelectLevels = buttons;
            UI.levelSelectMetadatas = metadatas;

            List<List<LevelScore>> scores = new List<List<LevelScore>> {
                Capacity = buttons.Count
            };
            if(Directory.Exists("scores")) {
                for(int i = 0; i < directories.Length; i++) {
                    string name = Path.GetFileName(directories[i]);
                    if(name == "_template") continue;
                    string scoresPath = Path.Combine("scores", name + ".txt");
                    if(File.Exists(scoresPath)) {
                        scores.Add(Map.ScoresFromLines(File.ReadAllLines(scoresPath), UI.scoresPos));
                        logger.Info("Loaded scores for level {0}, total scores count: {1}", name, scores[i].Count);
                    }
                    else {
                        scores.Add(null);
                    }
                }
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
            return accuracy >= 100 ? ColorScheme.green : accuracy >= 70 ? ColorScheme.yellow : ColorScheme.red;
        }
        public static Color GetComboColor(int accuracy, int misses) {
            return accuracy >= 100 ? ColorScheme.green : misses <= 0 ? ColorScheme.yellow : ColorScheme.blue;
        }
        public void KeyPressed(object caller, KeyEventArgs key) {
            if(key.Code == Keyboard.Key.Escape) {
                if(currentMenu == Menu.Game) currentMenu = Menu.LastStats;
                else if(currentMenu == Menu.LastStats) {
                    currentMenu = Map.currentLevel.objects.Count > 0 ? Menu.Game : Menu.LevelSelect;
                }
                else if(currentMenu == Menu.LevelSelect || currentMenu == Menu.Settings) currentMenu = Menu.Main;
            }
            if(currentMenu == Menu.Game) {
                if(editing) {
                    char character = key.Code switch
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
                    if(character == '\0') {
                        if(key.Code == Keyboard.Key.Backspace) {
                            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.offset == roundedOffset &&
                                                                                                                                                                                                       obj.character != LevelObject.speedChar);
                            foreach(LevelObject obj in objects) {
                                _ = Map.currentLevel.objects.Remove(obj);
                            }
                        }
                        else if(key.Code == Keyboard.Key.Up || key.Code == Keyboard.Key.Down) {
                            int delta = key.Code == Keyboard.Key.Up ? 1 : -1;
                            if(key.Alt) {
                                Map.currentLevel.metadata.linesFrequency += delta;
                            }
                            else {
                                if(!Map.currentLevel.speeds.Select(speed => speed.offset).Contains(roundedOffset)) {
                                    int speedIndex = 0;
                                    for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                                        if(Map.currentLevel.speeds[i].offset <= roundedOffset) speedIndex = i;
                                    }
                                    Map.currentLevel.speeds.Add(new LevelSpeed(Map.currentLevel.speeds[speedIndex].speed, roundedOffset));
                                    Map.currentLevel.speeds.Sort((speed1, speed2) => speed1.offset.CompareTo(speed2.offset));
                                }

                                int index = Map.currentLevel.speeds.Select(speed => speed.offset).ToList().IndexOf(roundedOffset);

                                Map.currentLevel.speeds[index].speed += (key.Shift ? 1 : 10) * delta;

                                if(index >= 1 && Map.currentLevel.speeds[index].speed == Map.currentLevel.speeds[index - 1].speed) {
                                    Map.currentLevel.speeds.RemoveAt(index);
                                }

                                List<LevelObject> speedObjects = Map.currentLevel.objects.FindAll(obj => obj.character == LevelObject.speedChar);
                                foreach(LevelObject obj in speedObjects) {
                                    _ = Map.currentLevel.objects.Remove(obj);
                                }
                                for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                                    Map.currentLevel.objects.Add(new LevelObject(LevelObject.speedChar, Map.currentLevel.speeds[i].offset));
                                }
                            }
                        }
                        else if(key.Code == Keyboard.Key.Left || key.Code == Keyboard.Key.Right) {
                            if(key.Shift) {
                                Map.currentLevel.metadata.hpRestorage += key.Code == Keyboard.Key.Right ? 1 : -1;
                            }
                            else {
                                Map.currentLevel.metadata.hpDrain += key.Code == Keyboard.Key.Right ? 1 : -1;
                            }
                        }
                        else if(key.Code == Keyboard.Key.F1 || key.Code == Keyboard.Key.F2) {
                            if(key.Shift) {
                                Map.currentLevel.metadata.initialOffsetMS += key.Code == Keyboard.Key.F2 ? 10 : -10;
                            }
                            else {
                                Map.currentLevel.metadata.initialOffsetMS += key.Code == Keyboard.Key.F2 ? 1 : -1;
                            }
                            if (music.Status == SoundStatus.Playing) {
                                RecalculateTime();
                            }
                        }
                    }
                    else {
                        if(Map.currentLevel.objects.FindAll(obj => obj.character == character && obj.offset == roundedOffset).Count <= 0) {
                            Map.currentLevel.objects.Add(new LevelObject(character, roundedOffset));
                            if(key.Shift) {
                                character = LevelObject.holdChar;
                                Map.currentLevel.objects.Add(new LevelObject(character, roundedOffset, Map.currentLevel.objects));
                            }
                        }
                    }

                    RecalculatePosition();
                }
                else {
                    for(int y = Map.linePos.y + 2; y >= Map.linePos.y - 2; y--) {
                        if(CheckLine(y)) break;
                    }
                }
            }
        }
        public void LostFocus(object caller, EventArgs args) {
            if(currentMenu == Menu.Game) {
                currentMenu = Menu.LastStats;
            }
            music.Volume = 0;
        }
        public void GainedFocus(object caller, EventArgs args) {
            music.Volume = Settings.Default.musicVolume;
        }
        bool CheckLine(int y) {
            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.character != LevelObject.speedChar &&
                                                                                                                                                                                       obj.character != LevelObject.holdChar &&
                                                                                                                                                                                       !obj.removed &&
                                                                                                                                                                                       !obj.ignore &&
                                                                                                                                                                                       obj.position.y == y);
            for(int i = 0; i < objects.Count; i++) {
                objects[i].CheckPress();
                if(objects[i].removed) return true;
            }
            return false;
        }
        public void MouseWheelScrolled(object caller, MouseWheelScrollEventArgs scroll) {
            if(currentMenu == Menu.LevelSelect) {
                Vector2 mousePos = Core.renderer.mousePosition;
                if(mousePos.y >= 12 && mousePos.y <= 49) {
                    if(mousePos.x >= 28 && mousePos.x <= 51) {
                        if(scroll.Delta > 0 && UI.levelSelectLevels.First().position.y >= 12) return;
                        if(scroll.Delta < 0 && UI.levelSelectLevels.Last().position.y <= 49) return;
                        foreach(Button button in UI.levelSelectLevels) {
                            button.position.y += (int)scroll.Delta;
                        }
                    }
                    else if(mousePos.x >= 1 && mousePos.x <= 26) {
                        if(scroll.Delta > 0 && UI.levelSelectScores[UI.currentLevelSelectIndex].First().scorePosition.y >= 12) return;
                        if(scroll.Delta < 0 && UI.levelSelectScores[UI.currentLevelSelectIndex].Last().scoresPosition.y <= 49) return;
                        foreach(LevelScore score in UI.levelSelectScores[UI.currentLevelSelectIndex]) {
                            int increment = (int)scroll.Delta;
                            score.scorePosition.y += increment;
                            score.accComboPosition.y += increment;
                            score.accComboDividerPosition.y += increment;
                            score.maxComboPosition.y += increment;
                            score.scoresPosition.y += increment;
                            score.linePosition.y += increment;
                        }
                    }
                }
            }
            else if(currentMenu == Menu.Game && editing) {
                offset = roundedOffset;
                offset += scroll.Delta;
                RecalculateTime();
            }
        }
        public static void RecalculateTime() {
            long useMicrosecs = (long)(Math.Abs(OffsetToMilliseconds(offset, Map.currentLevel.speeds)) * 1000f);
            time = Time.FromMicroseconds(useMicrosecs - Map.currentLevel.metadata.initialOffsetMS);
            music.PlayingOffset = time;
        }
        public static float OffsetToMilliseconds(float offset, List<LevelSpeed> sortedSpeeds) {
            float useOffset = offset;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(sortedSpeeds[i].offset <= useOffset) speedIndex = i;
            }
            float time = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) time += (sortedSpeeds[i + 1].offset - sortedSpeeds[i].offset) * (60000f / sortedSpeeds[i].speed);
                else time += (useOffset - sortedSpeeds[i].offset) * (60000f / sortedSpeeds[speedIndex].speed);
            }
            return time;
        }
        public static float MillisecondsToOffset(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(OffsetToMilliseconds(sortedSpeeds[i].offset, sortedSpeeds) <= useTime) speedIndex = i;
                else break;
            }
            float offset = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) {
                    int increment = sortedSpeeds[i + 1].offset - sortedSpeeds[i].offset;
                    offset += increment;
                    useTime -= increment * (60000f / sortedSpeeds[i].speed);
                }
                else offset += useTime / (60000f / sortedSpeeds[i].speed);
            }
            return offset;
        }
    }
}
