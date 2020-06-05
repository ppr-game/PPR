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
        public static Time timeFromStart;
        static Time prevPlayingOffset;
        static float _offset = 0f;
        public static float offset {
            set {
                _offset = value;
                if(!editing) UI.progress = (int)(timeFromStart.AsMilliseconds() / (float)Map.currentLevel.metadata.maxTime * 80f);
            }
            get => _offset;
        }
        public static int roundedOffset = 0;
        public static float steps = 0f;
        public static int roundedSteps = 0;
        public static float prevSteps = 0f;
        public static int currentBPM = 1;
        public static Music music;
        public static SoundBuffer hitsoundBuffer;
        public static SoundBuffer ticksoundBuffer;
        public static SoundBuffer failsoundBuffer;
        public static SoundBuffer passsoundBuffer;
        public static SoundBuffer buttonclicksoundBuffer;
        public static SoundBuffer slidersoundBuffer;
        public static Sound hitsound;
        public static Sound ticksound;
        public static Sound failsound;
        public static Sound passsound;
        public static Sound buttonclicksound;
        public static Sound slidersound;
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

            try {
                music = new Music(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "mainMenu")));
            }
            catch(SFML.LoadingFailedException) {
                music = new Music(GetSoundFile(Path.Combine("resources", "audio", "Default", "mainMenu")));
            }

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

        public static string GetSoundFile(string pathWithoutExtension) {
            string[] extensions = { ".ogg", ".wav", ".flac" };

            foreach(string extension in extensions) {
                string path = pathWithoutExtension + extension;
                if(File.Exists(path)) {
                    return path;
                }
            }

            return "";
        }

        public void ReloadSounds() {
            try {
                hitsoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "hitsound")));
            }
            catch(SFML.LoadingFailedException) {
                hitsoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", "Default", "hitsound")));
            }
            try {
                ticksoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "ticksound")));
            }
            catch(SFML.LoadingFailedException) {
                ticksoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", "Default", "ticksound")));
            }
            try {
                failsoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "failsound")));
            }
            catch(SFML.LoadingFailedException) {
                failsoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", "Default", "failsound")));
            }
            try {
                passsoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "passsound")));
            }
            catch(SFML.LoadingFailedException) {
                passsoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", "Default", "passsound")));
            }
            try {
                buttonclicksoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "buttonclicksound")));
            }
            catch(SFML.LoadingFailedException) {
                buttonclicksoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", "Default", "buttonclicksound")));
            }
            try {
                slidersoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", Settings.Default.audio, "slidersound")));
            }
            catch(SFML.LoadingFailedException) {
                slidersoundBuffer = new SoundBuffer(GetSoundFile(Path.Combine("resources", "audio", "Default", "slidersound")));
            }
            hitsound = new Sound(hitsoundBuffer);
            ticksound = new Sound(ticksoundBuffer);
            failsound = new Sound(failsoundBuffer);
            passsound = new Sound(passsoundBuffer);
            buttonclicksound = new Sound(buttonclicksoundBuffer);
            slidersound = new Sound(slidersoundBuffer);
            hitsound.Volume = Settings.Default.soundsVolume;
            ticksound.Volume = Settings.Default.soundsVolume;
            failsound.Volume = Settings.Default.soundsVolume;
            passsound.Volume = Settings.Default.soundsVolume;
            buttonclicksound.Volume = Settings.Default.soundsVolume;
            slidersound.Volume = Settings.Default.soundsVolume;
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
                failsound.Volume = Settings.Default.soundsVolume;
                passsound.Volume = Settings.Default.soundsVolume;
                buttonclicksound.Volume = Settings.Default.soundsVolume;
                slidersound.Volume = Settings.Default.soundsVolume;
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
            currentBPM = Map.currentLevel.speeds[0].speed;
            timeFromStart = Time.Zero;
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
        public void Update() {
            if(currentMenu != Menu.Game) return;

            if(music.PlayingOffset != prevPlayingOffset) {
                timeFromStart = music.PlayingOffset - Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMS);
                offset = MillisecondsToOffset(timeFromStart.AsMicroseconds() / 1000f);
                steps = MillisecondsToSteps(timeFromStart.AsMicroseconds() / 1000f);
            }
            prevPlayingOffset = music.PlayingOffset;

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
                    offset = MillisecondsToOffset(timeFromStart.AsMilliseconds());
                    steps = MillisecondsToSteps(timeFromStart.AsMilliseconds());
                }
                UI.progress = (int)(music.PlayingOffset.AsSeconds() / duration * 80f);
            }
        }
        public static void UpdateTime() {
            long useMicrosecs = (long)((MathF.Round(StepsToMilliseconds(steps)) + Map.currentLevel.metadata.initialOffsetMS) * 1000f);
            music.PlayingOffset = Time.FromMicroseconds(useMicrosecs);
        }
        public static void UpdateSpeeds() {
            for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                if(Map.currentLevel.speeds[i].time <= timeFromStart.AsMilliseconds()) {
                    currentBPM = Map.currentLevel.speeds[i].speed;
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
            List<int> flooredSpeedsSteps = Map.currentLevel.speeds.Select(speed => (int)MillisecondsToSteps(speed.time)).ToList();
            if(!flooredSpeedsSteps.Contains((int)steps)) {
                int speedIndex = 0;
                for(int i = 0; i < Map.currentLevel.speeds.Count; i++) {
                    if(Map.currentLevel.speeds[i].time <= timeFromStart.AsMilliseconds()) speedIndex = i;
                }
                Map.currentLevel.speeds.Add(new LevelSpeed(Map.currentLevel.speeds[speedIndex].speed, timeFromStart.AsMilliseconds()));
                Map.currentLevel.speeds.Sort((speed1, speed2) => speed1.time.CompareTo(speed2.time));
                //Map.currentLevel.speeds = SortLevelSpeeds(Map.currentLevel.speeds);
            }

            flooredSpeedsSteps = Map.currentLevel.speeds.Select(speed => (int)MillisecondsToSteps(speed.time)).ToList();
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
                Map.currentLevel.objects.Add(new LevelObject(LevelObject.speedChar, Map.currentLevel.speeds[i].time, Map.currentLevel.speeds));
            }

            foreach(LevelObject obj in Map.currentLevel.objects) {
                obj.startPosition.y = (int)MathF.Round(Map.linePos.y - MillisecondsToOffset(obj.time, Map.currentLevel.speeds));
                obj.position.y = obj.startPosition.y;
                obj.steps = (int)MillisecondsToSteps(obj.time, Map.currentLevel.speeds);
            }
        }
        public void KeyPressed(object caller, KeyEventArgs key) {
            // Back
            if(Bindings.Default.back.IsPressed(key)) {
                if(currentMenu == Menu.Game) currentMenu = Menu.LastStats;
                else if(currentMenu == Menu.LastStats) {
                    currentMenu = Map.currentLevel.objects.Count > 0 ? Menu.Game : Menu.LevelSelect;
                }
                else if(currentMenu == Menu.LevelSelect || currentMenu == Menu.Settings) currentMenu = Menu.Main;
            }
            if(currentMenu == Menu.Game) {
                if(editing) {
                    char character = GetNoteBinding(key.Code);
                    if(character == '\0') {
                        // Erase
                        if(Bindings.Default.erase.IsPressed(key)) {
                            List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => (int)MillisecondsToSteps(obj.time) == (int)steps &&
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
                        if(Map.currentLevel.objects.FindAll(obj => obj.character == character && MillisecondsToSteps(obj.time) == (int)steps).Count <= 0) {
                            Map.currentLevel.objects.Add(new LevelObject(character, timeFromStart.AsMilliseconds(), Map.currentLevel.speeds));
                            if(key.Shift) {
                                character = LevelObject.holdChar;
                                Map.currentLevel.objects.Add(new LevelObject(character, timeFromStart.AsMilliseconds(), Map.currentLevel.speeds, Map.currentLevel.objects));
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
                                                                                                                                                                                       obj.steps == step);
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
            /*int steps = (int)MathF.Round(MillisecondsToSteps(music.PlayingOffset.AsMilliseconds()));
            Time newTime = Time.FromSeconds(StepsToMilliseconds(steps + delta) / 1000f);
            music.PlayingOffset = newTime < Time.Zero ? Time.Zero : newTime > music.Duration ? music.Duration : newTime;
            timeFromStart = music.PlayingOffset - Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMS);
            logger.Debug("{0} , {1} , {2}", music.PlayingOffset.AsMilliseconds(), steps + delta, StepsToMilliseconds(steps + delta));*/
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
                if(MillisecondsToSteps(sortedSpeeds[i].time, sortedSpeeds) <= useSteps) speedIndex = i;
            }
            float time = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) time += sortedSpeeds[i + 1].time - sortedSpeeds[i].time;
                else time += (useSteps - MillisecondsToSteps(sortedSpeeds[i].time)) * (60000f / Math.Abs(sortedSpeeds[i].speed));
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
                if(sortedSpeeds[i].time <= useTime) speedIndex = i;
                else break;
            }
            float steps = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) {
                    int timeDecrement = sortedSpeeds[i + 1].time - sortedSpeeds[i].time;
                    steps += timeDecrement / (60000f / Math.Abs(sortedSpeeds[i].speed));
                    useTime -= timeDecrement;
                }
                else steps += useTime / (60000f / Math.Abs(sortedSpeeds[i].speed));
            }
            return steps;
        }
        public static float MillisecondsToOffset(float time) {
            return MillisecondsToOffset(time, Map.currentLevel.speeds);
        }
        public static float MillisecondsToOffset(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++) {
                if(sortedSpeeds[i].time <= useTime) speedIndex = i;
                else break;
            }
            float offset = 0;
            for(int i = 0; i <= speedIndex; i++) {
                if(i != speedIndex) {
                    int timeDecrement = sortedSpeeds[i + 1].time - sortedSpeeds[i].time;
                    offset += timeDecrement / (60000f / sortedSpeeds[i].speed);
                    useTime -= timeDecrement;
                }
                else offset += useTime / (60000f / sortedSpeeds[i].speed);
            }
            return offset;
        }
        public static bool StepPassedLine(int step, int lineOffset = 0) {
            return roundedSteps >= step + lineOffset;
        }
        /*public static List<LevelSpeed> SortLevelSpeeds(List<LevelSpeed> list) {
            List<LevelSpeed> unsorted = new List<LevelSpeed>(list);
            List<LevelSpeed> sorted = new List<LevelSpeed>();
            unsorted.Sort((speed1, speed2) => speed1.offset.CompareTo(speed2.offset));

            int direction = 0;
            int index = 0;
            for(int i = 0; i < unsorted.Count; i++) {
                if(unsorted[i].offset == 0) {
                    direction = Math.Sign(unsorted[i].speed);
                    index = i;
                    sorted.Add(unsorted[index]);
                    unsorted.RemoveAt(index);
                    break;
                }
            }
            while(unsorted.Count > 0) {
                if(direction == 0) break;
                if(direction == 1) direction = 0;
                if(direction >= unsorted.Count) break;

                index += direction;

                if(index < 0 || index >= unsorted.Count) break;

                int newDirection = Math.Sign(unsorted[index].speed);
                sorted.Add(unsorted[index]);
                unsorted.RemoveAt(index);
                direction = newDirection;
            }

            return sorted;
        }*/
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
    }
}
