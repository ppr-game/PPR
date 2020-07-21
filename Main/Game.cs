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
    public enum Menu { Main, LevelSelect, Settings, KeybindsEditor, LastStats, Game }
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
        public static Time timeFromStart;
        static Time prevPlayingOffset;
        static Time prevFramePlayingOffset;
        static Time interpolatedPlayingOffset;
        public static float offset = 0f;
        public static int roundedOffset = 0;
        static float _steps = 0f;
        public static float steps {
            get => _steps;
            set {
                _steps = value;
                if(!editing) UI.progress = (int)(value / Map.currentLevel.metadata.maxStep * 80f);
            }
        }
        public static int roundedSteps = 0;
        public static float prevSteps = 0f;
        public static int currentDirectionLayer = 0;
        public static int currentBPM = 1;
        public static float currentSpeedSec = 60f;
        public static float absoluteCurrentSpeedSec = 60f;
        public static Music music;
        public static Sound hitSound;
        public static Sound tickSound;
        public static Sound failSound;
        public static Sound passSound;
        public static Sound buttonClickSound;
        public static Sound sliderSound;
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
            logger.Info("Current keybinds:");
            foreach(SettingsPropertyValue value in Bindings.Default.PropertyValues) {
                logger.Info(value.Name + "=" + value.PropertyValue);
            }

            RPC.Initialize();

            try {
                music = new Music(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "mainMenu")));
            }
            catch(SFML.LoadingFailedException) {
                music = new Music(GetSoundFilePath(Path.Combine("resources", "audio", "Default", "mainMenu")));
            }

            music.Volume = Settings.Default.musicVolume;
            music.Loop = true;
            music.Play();
        }
        public void ReloadSettings() {
            Bindings.Default.Reload();

            Settings.Default.PropertyChanged -= PropertyChanged;

            Settings.Default.Reload();

            Core.renderer.SetFullscreen(Settings.Default.fullscreen);

            UI.RecreateButtons();
            UI.musicVolumeSlider.value = Settings.Default.musicVolume;
            UI.soundsVolumeSlider.value = Settings.Default.soundsVolume;
            UI.bloomSwitch.selected = Settings.Default.bloom;
            UI.showFpsSwitch.selected = Settings.Default.showFps;
            UI.fullscreenSwitch.selected = Settings.Default.fullscreen;

            LevelObject.color = ColorScheme.white;
            LevelObject.nextDirLayerColor = ColorScheme.lightDarkGray;
            LevelObject.speedColor = ColorScheme.blue;

            Settings.Default.PropertyChanged += PropertyChanged;
        }

        public static string GetSoundFilePath(string pathWithoutExtension) {
            string[] extensions = { ".ogg", ".wav", ".flac" };

            foreach(string extension in extensions) {
                string path = pathWithoutExtension + extension;
                if(File.Exists(path)) {
                    return path;
                }
            }

            return "";
        }
        public static bool TryLoadSoundBuffer(string path, out SoundBuffer buffer) {
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
        public static bool TryLoadSound(string path, out Sound sound) {
            if(TryLoadSoundBuffer(path, out SoundBuffer buffer)) {
                sound = new Sound(buffer);
                return true;
            }
            sound = null;
            return false;
        }

        public void ReloadSounds() {
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "hit")), out hitSound))
                hitSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "tick")), out tickSound))
                tickSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "fail")), out failSound))
                failSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "pass")), out passSound))
                passSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "buttonClick")), out buttonClickSound))
                buttonClickSound.Volume = Settings.Default.soundsVolume;
            if(TryLoadSound(GetSoundFilePath(Path.Combine("resources", "audio", Settings.Default.audio, "slider")), out sliderSound))
                sliderSound.Volume = Settings.Default.soundsVolume;
        }
        public void End() {
            logger.Info("Exiting");

            Settings.Default.Save();

            RPC.client.ClearPresence();
            RPC.client.Dispose();

            LogManager.Shutdown();

            Core.renderer.window.Close();
        }
        public void PropertyChanged(object caller, PropertyChangedEventArgs e) {
            if(e.PropertyName == "font") {
                string[] fontMappingsLines = File.ReadAllLines(Path.Combine("resources", "fonts", Settings.Default.font, "mappings.txt"));
                string[] fontSizeStr = fontMappingsLines[0].Split(',');
                Core.renderer.fontSize = new Vector2(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));
                Core.renderer.windowWidth = Core.renderer.width * Core.renderer.fontSize.x;
                Core.renderer.windowHeight = Core.renderer.height * Core.renderer.fontSize.y;
                Core.renderer.UpdateWindow();

                BitmapFont font = new BitmapFont(new Image(Path.Combine("resources", "fonts", Settings.Default.font, "font.png")),
                    fontMappingsLines[1], Core.renderer.fontSize);
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
                hitSound.Volume = Settings.Default.soundsVolume;
                tickSound.Volume = Settings.Default.soundsVolume;
                failSound.Volume = Settings.Default.soundsVolume;
                passSound.Volume = Settings.Default.soundsVolume;
                buttonClickSound.Volume = Settings.Default.soundsVolume;
                sliderSound.Volume = Settings.Default.soundsVolume;
            }
            else if(e.PropertyName == "audio") {
                ReloadSounds();
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
        public static void GameStart(string musicPath) {
            usedAuto = auto;
            UI.progress = 80;
            offset = 0;
            roundedOffset = 0;
            steps = 0;
            roundedSteps = 0;
            prevSteps = 0;
            currentDirectionLayer = 0;
            currentBPM = Map.currentLevel.speeds[0].speed;
            currentSpeedSec = 60f / currentBPM;
            timeFromStart = Time.Zero;
            interpolatedPlayingOffset = Time.Zero;
            prevFramePlayingOffset = Time.Zero;
            prevPlayingOffset = Time.Zero;
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
                music.PlayingOffset = Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMS);
                if(!editing) music.Play();
            }

            logger.Info("Entered level '{0}' by {1}", Map.currentLevel.metadata.name, Map.currentLevel.metadata.author);
        }
        private float accumulator = 0f;
        public void Update() {
            if(currentMenu != Menu.Game) return;

            float fixedDeltaTime = absoluteCurrentSpeedSec / 12f;

            accumulator += Core.deltaTime;
            float totalTimesToExec = 0f;
            if(accumulator >= fixedDeltaTime) totalTimesToExec = MathF.Ceiling((accumulator - fixedDeltaTime) / fixedDeltaTime);
            while(accumulator >= fixedDeltaTime) {
                float interpT = 1f - MathF.Ceiling((accumulator - fixedDeltaTime) / fixedDeltaTime) / totalTimesToExec;
                interpolatedPlayingOffset = music.PlayingOffset * interpT + prevFramePlayingOffset * (1f - interpT);
                if(interpT > 0f) logger.Debug(interpT);
                FixedUpdate();
                accumulator -= fixedDeltaTime;
            }
            prevFramePlayingOffset = music.PlayingOffset;
        }
        public void FixedUpdate() {
            if(interpolatedPlayingOffset != prevPlayingOffset) {
                timeFromStart = interpolatedPlayingOffset - Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMS);
                steps = MillisecondsToSteps(timeFromStart.AsMicroseconds() / 1000f);
                if(music.Status != SoundStatus.Playing) steps = MathF.Round(steps);
                offset = StepsToOffset(steps);
            }
            prevPlayingOffset = interpolatedPlayingOffset;

            //if(steps - prevSteps > 1f) logger.Warn("Lag detected: steps increased too quickly ({0})", steps - prevSteps);

            if(MathF.Floor(prevSteps) != MathF.Floor(steps)) {
                roundedSteps = (int)MathF.Round(steps);
                roundedOffset = (int)MathF.Round(offset);
                RecalculatePosition();
            }
            prevSteps = steps;

            if(editing) {
                float initialOffset = Map.currentLevel.metadata.initialOffsetMS / 1000f;
                float duration = music.Duration.AsSeconds() - initialOffset;
                if(Core.renderer.mousePosition.y == 0 && Core.renderer.leftButtonPressed) {
                    float mouseProgress = Math.Clamp(Core.renderer.mousePositionF.X / 80f, 0f, 1f);
                    music.PlayingOffset = Time.FromSeconds(duration * mouseProgress + initialOffset);
                    timeFromStart = music.PlayingOffset - Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMS);
                    steps = MathF.Round(MillisecondsToSteps(timeFromStart.AsMilliseconds()));
                    offset = StepsToOffset(steps);
                }
                UI.progress = (int)(music.PlayingOffset.AsSeconds() / duration * 80f);
            }

            Map.SimulateAll();
        }
        public static void UpdateTime() {
            long useMicrosecs = (long)((MathF.Round(StepsToMilliseconds(steps)) + Map.currentLevel.metadata.initialOffsetMS) * 1000f);
            music.PlayingOffset = Time.FromMicroseconds(useMicrosecs);
        }
        public static void UpdateSpeeds() {
            for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                if(Map.currentLevel.speeds[i].step <= steps) {
                    currentBPM = Map.currentLevel.speeds[i].speed;
                    currentSpeedSec = 60f / currentBPM;
                    absoluteCurrentSpeedSec = Math.Abs(currentSpeedSec);
                    currentDirectionLayer = StepsToDirectionLayer(roundedSteps);
                }
                else break;
            }
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
        public static void ChangeSpeed(int delta) {
            List<int> flooredSpeedsSteps = Map.currentLevel.speeds.Select(speed => speed.step).ToList();
            if(!flooredSpeedsSteps.Contains((int)steps)) {
                int speedIndex = 0;
                for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                    if(Map.currentLevel.speeds[i].step <= steps) speedIndex = i;
                }
                Map.currentLevel.speeds.Add(new LevelSpeed(Map.currentLevel.speeds[speedIndex].speed, (int)steps));
                Map.currentLevel.speeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
                //Map.currentLevel.speeds = SortLevelSpeeds(Map.currentLevel.speeds);
            }

            flooredSpeedsSteps = Map.currentLevel.speeds.Select(speed => speed.step).ToList();
            int index = flooredSpeedsSteps.IndexOf((int)steps);

            Map.currentLevel.speeds[index].speed += delta;

            if(index >= 1 && Map.currentLevel.speeds[index].speed == Map.currentLevel.speeds[index - 1].speed) {
                Map.currentLevel.speeds.RemoveAt(index);
            }

            List<LevelObject> speedObjects = Map.currentLevel.objects.FindAll(obj => obj.character == LevelObject.speedChar);
            foreach(LevelObject obj in speedObjects) {
                _ = Map.currentLevel.objects.Remove(obj);
            }
            for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                Map.currentLevel.objects.Add(new LevelObject(LevelObject.speedChar, Map.currentLevel.speeds[i].step, Map.currentLevel.speeds));
            }
        }
        public void KeyPressed(object caller, KeyEventArgs key) {
            // Back
            if(Bindings.Default.back.IsPressed(key)) {
                currentMenu = currentMenu == Menu.Game ? Menu.LastStats
                    : currentMenu == Menu.LastStats ? Map.currentLevel.objects.Count > 0 ? Menu.Game : Menu.LevelSelect :
                    currentMenu == Menu.KeybindsEditor ? Menu.Settings : Menu.Main;
            }
            if(currentMenu == Menu.Game) {
                if(editing) {
                    char character = GetNoteBinding(key.Code);
                    if(character == '\0') {
                        // Erase
                        if(Bindings.Default.erase.IsPressed(key)) {
                            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.step == (int)steps &&
                                                                                                                                                                                                       obj.character != LevelObject.speedChar);
                            foreach(LevelObject obj in objects) {
                                _ = Map.currentLevel.objects.Remove(obj);
                            }
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
                            Map.currentLevel.metadata.initialOffsetMS += 10;
                        else if(Bindings.Default.initialOffsetDownBoost.IsPressed(key))
                            Map.currentLevel.metadata.initialOffsetMS -= 10;
                        else if(Bindings.Default.initialOffsetUp.IsPressed(key))
                            Map.currentLevel.metadata.initialOffsetMS++;
                        else if(Bindings.Default.initialOffsetDown.IsPressed(key))
                            Map.currentLevel.metadata.initialOffsetMS--;

                        // Fast scroll
                        else if(Bindings.Default.fastScrollUp.IsPressed(key)) ScrollTime(10);
                        else if(Bindings.Default.fastScrollDown.IsPressed(key)) ScrollTime(-10);
                    }
                    else {
                        if(Map.currentLevel.objects.FindAll(obj => obj.character == character && obj.step == (int)steps).Count <= 0) {
                            Map.currentLevel.objects.Add(new LevelObject(character, (int)steps, Map.currentLevel.speeds));
                            if(key.Shift) {
                                character = LevelObject.holdChar;
                                Map.currentLevel.objects.Add(new LevelObject(character, (int)steps, Map.currentLevel.speeds, Map.currentLevel.objects));
                            }
                        }
                    }

                    RecalculatePosition();
                }
                else if(!auto) {
                    for(int step = roundedSteps - LevelObject.missRange; StepPassedLine(step, -LevelObject.missRange); step++) {
                        if(CheckLine(step)) break;
                    }
                }
            }
        }
        bool CheckLine(int step) {
            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.character != LevelObject.speedChar &&
                        obj.character != LevelObject.holdChar &&
                        !obj.removed &&
                        !obj.ignore &&
                        obj.step == step);
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
                ScrollTime((int)scroll.Delta);
            }
        }
        public static void ScrollTime(int delta) {
            steps = Math.Clamp(steps + delta, 0, MillisecondsToSteps(music.Duration.AsMicroseconds() / 1000f));
            UpdateTime();
        }
        public static float StepsToMilliseconds(float steps) {
            return StepsToMilliseconds(steps, Map.currentLevel.speeds);
        }
        public static float StepsToMilliseconds(float steps, List<LevelSpeed> sortedSpeeds) {
            float useSteps = steps;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(sortedSpeeds[i].step <= useSteps) speedIndex = i;
            }
            float time = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i == speedIndex) time += (useSteps - sortedSpeeds[i].step) * (60000f / Math.Abs(sortedSpeeds[i].speed));
                else time += (sortedSpeeds[i + 1].step - sortedSpeeds[i].step) * (60000f / Math.Abs(sortedSpeeds[i].speed));
            }
            return time;
        }
        public static float MillisecondsToSteps(float time) {
            return MillisecondsToSteps(time, Map.currentLevel.speeds);
        }
        public static float MillisecondsToSteps(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(StepsToMilliseconds(sortedSpeeds[i].step) <= useTime) speedIndex = i;
                else break;
            }
            float steps = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) {
                    int stepsIncrement = sortedSpeeds[i + 1].step - sortedSpeeds[i].step;
                    steps += stepsIncrement;
                    useTime -= stepsIncrement * (60000f / Math.Abs(sortedSpeeds[i].speed));
                }
                else steps += useTime / (60000f / Math.Abs(sortedSpeeds[i].speed));
            }
            return steps;
        }
        public static float StepsToOffset(float steps) {
            return StepsToOffset(steps, Map.currentLevel.speeds);
        }
        public static float StepsToOffset(float steps, List<LevelSpeed> sortedSpeeds) {
            float useSteps = steps;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(sortedSpeeds[i].step <= useSteps) speedIndex = i;
                else break;
            }
            float offset = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) {
                    int stepsDecrement = sortedSpeeds[i + 1].step - sortedSpeeds[i].step;
                    offset += stepsDecrement * MathF.Sign(sortedSpeeds[i].speed);
                    useSteps -= stepsDecrement;
                }
                else offset += useSteps * MathF.Sign(sortedSpeeds[i].speed);
            }
            return offset;
        }
        public static int StepsToDirectionLayer(float steps) {
            return StepsToDirectionLayer(steps, Map.currentLevel.speeds);
        }
        public static int StepsToDirectionLayer(float steps, List<LevelSpeed> sortedSpeeds) {
            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(sortedSpeeds[i].step <= steps) speedIndex = i;
                else break;
            }
            int directionLayer = 0;
            for(int i = 1; i <= speedIndex; i++) {
                if(MathF.Sign(sortedSpeeds[i].speed) != MathF.Sign(sortedSpeeds[i - 1].speed)) directionLayer++;
            }
            return directionLayer;
        }
        public static bool StepPassedLine(int step, int lineOffset = 0) {
            return roundedSteps >= step + lineOffset;
        }
        public static void GenerateLevelList() {
            string[] directories = Directory.GetDirectories("levels");
            List<Button> buttons = new List<Button>();
            List<LevelMetadata?> metadatas = new List<LevelMetadata?>();
            for(int i = 0; i < directories.Length; i++) {
                string name = Path.GetFileName(directories[i]);
                if(name == "_template") continue;
                buttons.Add(new Button(new Vector2(25, 12 + i), name, 30, ColorScheme.black, ColorScheme.white, ColorScheme.lightDarkGray));
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
    }
}
