using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using MoonSharp.Interpreter;

using NLog;

using PPR.GUI;
using PPR.GUI.Elements;
using PPR.Main.Levels;
using PPR.Properties;

using PRR;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

using LoadingFailedException = SFML.LoadingFailedException;

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
                        string path = Path.Join("scores", $"{Map.currentLevel.metadata.name}.txt");
                        string text = File.Exists(path) ? File.ReadAllText(path) : "";
                        text =
                            $"{Map.TextFromScore(new LevelScore(new Vector2i(), score, accuracy, maxCombo, scores))}\n{text}";
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
                    string name = Path.GetFileName(Path.GetDirectoryName(currentMusicPath));
                    if(name == "Default" || name == Settings.GetPath("audio")) SwitchMusic();
                }
                if(_currentMenu != value) {
                    Core.pauseDrawing = true;
                    UI.FadeOut(value == Menu.Game ? 10f : 7f);
                }
                _currentMenu = value;
                switch(value) {
                    case Menu.Main:
                        RPC.SetPresence("In main menu");
                        break;
                    case Menu.LevelSelect:
                        RPC.SetPresence($"Choosing what to {(editing ? "edit" : "play")}");
                        break;
                    case Menu.Game:
                        if(Map.currentLevel != null)
                            RPC.SetPresence(editing ? "Editing" : auto ? "Watching" : "Playing",
                                Map.currentLevel.metadata.name);
                        break;
                    case Menu.LastStats:
                        if(Map.currentLevel != null)
                            RPC.SetPresence(editing ? "Paused Editing" :
                                statsState == StatsState.Pause ? "Paused" :
                                statsState == StatsState.Pass ? "Passed" : "Failed",
                                Map.currentLevel.metadata.name);
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
        static string _currentMusicPath;
        public static string currentMusicPath {
            get => _currentMusicPath;
            set {
                _currentMusicPath = value;
                if(currentMusicName == "Waterflame - Cove") return;
                string[] menusAnimLines =
                    File.ReadAllLines(Path.Join(Path.GetDirectoryName(value)!, "level.txt"));
                menusAnimInitialOffset = GetInitialOffset(menusAnimLines);
                menusAnimSpeeds = GetSpeeds(menusAnimLines);
            }
        }
        public static string currentMusicName {
            get {
                string name = Path.GetFileName(Path.GetDirectoryName(currentMusicPath));
                return name == "Default" || name == Settings.GetPath("audio") ? "Waterflame - Cove" : name;
            }
        }
        public static int menusAnimInitialOffset;
        public static List<LevelSpeed> menusAnimSpeeds;
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
        public static int accuracy { get; private set; } = 100;
        public static int[] scores = new int[3]; // score / 5 = index
        public static int combo;
        public static int maxCombo;
        public static bool editing;
        public static bool auto;
        static bool _usedAuto;
        public static bool changed;
        float _accumulator;
        public static bool exiting;
        public static Script script;
        public static Closure update;
        public static Closure tick;
        public static Closure drawMap;
        public static Closure drawUI;
        public Game() {
            Settings.settingChanged += SettingChanged;
            Settings.Reload();
        }
        public static void Start() {
            RPC.Initialize();

            try {
                currentMusicPath = GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "mainMenu"));
                music = new Music(currentMusicPath);
            }
            catch(LoadingFailedException) {
                currentMusicPath = GetSoundFilePath(Path.Join("resources", "audio", "Default", "mainMenu"));
                music = new Music(currentMusicPath);
            }

            music.Volume = Settings.GetInt("musicVolume");
            music.Play();
            
            UI.RegenPositionRandoms();

            UI.FadeIn(0.5f);
        }
        public static void UpdateSettings() {
            UI.RecreateButtons();
            UI.musicVolumeSlider.value = Settings.GetInt("musicVolume");
            UI.soundsVolumeSlider.value = Settings.GetInt("soundsVolume");
            UI.bloomSwitch.selected = Settings.GetBool("bloom");
            UI.showFpsSwitch.selected = Settings.GetBool("showFps");
            UI.fullscreenSwitch.selected = Settings.GetBool("fullscreen");
            UI.fpsLimitSlider.value = Settings.GetInt("fpsLimit");
            UI.uppercaseSwitch.selected = Settings.GetBool("uppercaseNotes");

            LevelObject.linesColors = new Color[] {
                ColorScheme.GetColor("line_1_fg_color"),
                ColorScheme.GetColor("line_2_fg_color"),
                ColorScheme.GetColor("line_3_fg_color"),
                ColorScheme.GetColor("line_4_fg_color")
            };
            LevelObject.linesDarkColors = new Color[] {
                ColorScheme.GetColor("line_1_bg_color"),
                ColorScheme.GetColor("line_2_bg_color"),
                ColorScheme.GetColor("line_3_bg_color"),
                ColorScheme.GetColor("line_4_bg_color")
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

        public static void ReloadSounds() {
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "hit")), out hitSound) ||
                TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "hit")), out hitSound))
                hitSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "tick")), out tickSound) ||
                TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "tick")), out tickSound))
                tickSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "fail")), out failSound) ||
                TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "fail")), out failSound))
                failSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "pass")), out passSound) ||
                TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "pass")), out passSound))
                passSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "buttonClick")), out buttonClickSound) ||
                TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "buttonClick")), out buttonClickSound))
                buttonClickSound.Volume = Settings.GetInt("soundsVolume");
            if(TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", Settings.GetPath("audio"), "slider")), out sliderSound) ||
                TryLoadSound(GetSoundFilePath(Path.Join("resources", "audio", "Default", "slider")), out sliderSound))
                sliderSound.Volume = Settings.GetInt("soundsVolume");
        }
        public static void End() {
            logger.Info("Exiting");
            
            UI.FadeOut(0.75f);
            logger.Info("Started shutdown animation");
            
            music.Stop();
            logger.Info("Stopped music");

            Settings.SaveConfig();
            logger.Info("Saved settings");

            RPC.client.ClearPresence();
            RPC.client.Dispose();
            logger.Info("Removed Discord RPC");

            logger.Info("F to the logger");
            LogManager.Shutdown();

            exiting = true;
        }

        public static void SettingChanged(object caller, SettingChangedEventArgs e) {
            switch (e.settingName) {
                case "font": {
                    string[] fontMappingsLines = File.ReadAllLines(Path.Join("resources", "fonts", Settings.GetPath("font"), "mappings.txt"));
                    string[] fontSizeStr = fontMappingsLines[0].Split(',');
                    Core.renderer.fontSize = new Vector2i(int.Parse(fontSizeStr[0]), int.Parse(fontSizeStr[1]));
                    Core.renderer.windowWidth = Core.renderer.width * Core.renderer.fontSize.X;
                    Core.renderer.windowHeight = Core.renderer.height * Core.renderer.fontSize.Y;

                    BitmapFont font = new BitmapFont(new Image(Path.Join("resources", "fonts", Settings.GetPath("font"), "font.png")),
                        fontMappingsLines[1], Core.renderer.fontSize);
                    Core.renderer.text = new BitmapText(font, new Vector2i(Core.renderer.width, Core.renderer.height)) {
                        text = Core.renderer.display
                    };

                    Core.renderer.UpdateWindow();
                    break;
                }
                case "colorScheme": ColorScheme.Reload();
                    break;
                case "fullscreen": Core.renderer.SetFullscreen(Settings.GetBool("fullscreen"));
                    break;
                case "musicVolume": music.Volume = Settings.GetInt("musicVolume");
                    break;
                case "soundsVolume":
                    hitSound.Volume = Settings.GetInt("soundsVolume");
                    tickSound.Volume = Settings.GetInt("soundsVolume");
                    failSound.Volume = Settings.GetInt("soundsVolume");
                    passSound.Volume = Settings.GetInt("soundsVolume");
                    buttonClickSound.Volume = Settings.GetInt("soundsVolume");
                    sliderSound.Volume = Settings.GetInt("soundsVolume");
                    break;
                case "audio": ReloadSounds();
                    break;
                case "fpsLimit":
                    Core.renderer.UpdateFramerateSetting();
                    break;
            }
        }
        public static void LostFocus(object caller, EventArgs args) {
            if(currentMenu == Menu.Game) currentMenu = Menu.LastStats;
            music.Volume = 0;
            Core.renderer.UpdateFramerateSetting();
        }
        public static void GainedFocus(object caller, EventArgs args) {
            music.Volume = Settings.GetInt("musicVolume");
            Core.renderer.UpdateFramerateSetting();
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

            if(Map.currentLevel.script != null)
                try {
                    Script.WarmUp();
                    
                    UserData.RegisterType<Scripts.Core>();
                    UserData.RegisterType<Scripts.Main.Game>();
                    UserData.RegisterType<Scripts.Main.Levels.Level>();
                    UserData.RegisterType<Scripts.Main.Levels.Map>();
                    UserData.RegisterType<Scripts.Rendering.Renderer>();
                    UserData.RegisterType<Scripts.IO.File>();

                    UserData.RegisterType<Renderer.Alignment>();
                    UserData.RegisterType<Vector2i>();
                    UserData.RegisterType<Vector2f>();
                    UserData.RegisterType<Color>();
                    UserData.RegisterType<RenderCharacter>();
                    
                    script = new Script(CoreModules.Preset_SoftSandbox);

                    DynValue core = UserData.Create(new Scripts.Core());
                    DynValue game = UserData.Create(new Scripts.Main.Game());
                    DynValue level = UserData.Create(new Scripts.Main.Levels.Level());
                    DynValue map = UserData.Create(new Scripts.Main.Levels.Map());
                    DynValue renderer = UserData.Create(new Scripts.Rendering.Renderer());
                    DynValue file = UserData.Create(new Scripts.IO.File());
                    
                    script.Globals["core"] = core;
                    script.Globals["game"] = game;
                    script.Globals["level"] = level;
                    script.Globals["map"] = map;
                    script.Globals["renderer"] = renderer;
                    script.Globals["file"] = file;

                    script.Globals["alignment"] = UserData.CreateStatic<Renderer.Alignment>();
                    
                    script.Globals["vector2i"] = (Func<int, int, Vector2i>)((x, y) => new Vector2i(x, y));
                    script.Globals["vector2f"] = (Func<float, float, Vector2f>)((x, y) => new Vector2f(x, y));
                    
                    script.Globals["rgb"] = (Func<byte, byte, byte, Color>)((r, g, b) => new Color(r, g, b));
                    script.Globals["rgba"] =
                        (Func<byte, byte, byte, byte, Color>)((r, g, b, a) => new Color(r, g, b, a));
                    script.Globals["black"] = Color.Black;
                    script.Globals["white"] = Color.White;
                    script.Globals["red"] = Color.Red;
                    script.Globals["green"] = Color.Green;
                    script.Globals["blue"] = Color.Blue;
                    script.Globals["yellow"] = Color.Yellow;
                    script.Globals["magenta"] = Color.Magenta;
                    script.Globals["cyan"] = Color.Cyan;
                    script.Globals["transparent"] = Color.Transparent;
                    
                    script.Globals["renderCharacter"] =
                        (Func<char, Color, Color, RenderCharacter>)((character, background, foreground) =>
                            new RenderCharacter(character, background, foreground));

                    script.Options.DebugPrint = message => {
                        Console.WriteLine(message);
                        logger.Debug(message);
                    };
                    
                    script.DoFile(Map.currentLevel.script);

                    update = DynValue.FromObject(script, script.Globals["update"]).Function;
                    tick = DynValue.FromObject(script, script.Globals["tick"]).Function;
                    drawMap = DynValue.FromObject(script, script.Globals["drawMap"]).Function;
                    drawUI = DynValue.FromObject(script, script.Globals["drawUI"]).Function;
                }
                catch(InterpreterException ex) {
                    logger.Error(ex.DecoratedMessage);
                }

            if(File.Exists(musicPath)) {
                currentMusicPath = musicPath;
                music = new Music(musicPath) {
                    Volume = Settings.GetInt("musicVolume"),
                    PlayingOffset = Time.FromMilliseconds(Map.currentLevel.metadata.initialOffsetMs)
                };
                if(!editing) music.Play();
            }

            logger.Info("Entered level '{0}' by {1}", Map.currentLevel.metadata.name, Map.currentLevel.metadata.author);
        }
        public static void ClearScript() {
            script = null;
            update = null;
            tick = null;
            drawMap = null;
            drawUI = null;
        }
        public static void SwitchMusic() {
            if(exiting) return;
            
            string newPath = currentMusicPath;
            string[] paths = Directory.GetDirectories("levels")
                .Where(path => Path.GetFileName(Path.GetDirectoryName(path)) != "_template")
                .Select(path => GetSoundFilePath(Path.Join(path, "music")))
                .Where(path => !string.IsNullOrEmpty(path)).ToArray();
            int index = 0;
            switch(paths.Length) {
                case 0 when music.Status != SoundStatus.Stopped: return;
                case 0 when music.Status == SoundStatus.Stopped:
                case 1: newPath = currentMusicPath;
                    break;
                default: {
                    while(currentMusicPath == newPath) {
                        index = random.Next(0, paths.Length);
                        newPath = paths[index];
                    }
                    break;
                }
            }
            UI.currentLevelSelectIndex = index;
            currentMusicPath = newPath;
            music.Stop();
            music = new Music(currentMusicPath) {
                Volume = Core.renderer.window.HasFocus() ? Settings.GetInt("musicVolume") : 0f
            };
            music.Play();
        }
        public void Update() {
            if(Core.pauseDrawing && UI.fadeOutFinished) {
                Core.pauseDrawing = false;
                UI.FadeIn(currentMenu == Menu.Game ? 10f : 7f);
            }
            if(currentMenu == Menu.Main && music.Status == SoundStatus.Stopped) SwitchMusic();
            if(currentMenu != Menu.Game) {
                if(Path.GetFileName(Path.GetDirectoryName(currentMusicPath)) == "Default" ||
                   music.Status != SoundStatus.Playing)
                    UI.menusAnimBPM = 60;
                else {
                    int step = (int)MillisecondsToSteps(music.PlayingOffset.AsMilliseconds() - menusAnimInitialOffset,
                        menusAnimSpeeds);
                    UI.menusAnimBPM = Math.Abs(GetBPMAtStep(step, menusAnimSpeeds));
                }

                return;
            }

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

            try {
                update?.Call();
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
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
                if(Core.renderer.mousePosition.Y == 0 && Core.renderer.leftButtonPressed) {
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

            try {
                tick?.Call();
            }
            catch(InterpreterException ex) {
                logger.Error(ex.DecoratedMessage);
            }

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

        public static char GetNoteBinding(Keyboard.Key key) => key switch
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
            if(Bindings.GetBinding("back").IsPressed(key) &&
               currentMenu != Menu.LevelSelect && currentMenu != Menu.LastStats)
                currentMenu = currentMenu switch {
                    Menu.Game => Menu.LastStats,
                    Menu.KeybindsEditor => Menu.Settings,
                    _ => Menu.Main
                };
            // Fullscreen
            if(Bindings.GetBinding("fullscreen").IsPressed(key))
                Settings.SetBool("fullscreen", !Settings.GetBool("fullscreen"));
            if(currentMenu != Menu.Game) return;
            char character = GetNoteBinding(key.Code);
            if(editing) {
                if(character == '\0') {
                    // Erase
                    if(Bindings.GetBinding("erase").IsPressed(key)) {
                        List<LevelObject> objects = Map.currentLevel.objects.FindAll(obj => obj.step == (int)steps &&
                                                                                            obj.character != LevelObject.SPEED_CHAR);
                        foreach(LevelObject obj in objects) obj.toDestroy = true;

                        changed = true;
                    }

                    // Lines
                    else if(Bindings.GetBinding("linesFrequencyUp").IsPressed(key))
                        Map.currentLevel.metadata.linesFrequency++;
                    else if(Bindings.GetBinding("linesFrequencyDown").IsPressed(key))
                        Map.currentLevel.metadata.linesFrequency--;

                    // Speed
                    else if(Bindings.GetBinding("speedUpSlow").IsPressed(key)) ChangeSpeed(1);
                    else if(Bindings.GetBinding("speedDownSlow").IsPressed(key)) ChangeSpeed(-1);
                    else if(Bindings.GetBinding("speedUp").IsPressed(key)) ChangeSpeed(10);
                    else if(Bindings.GetBinding("speedDown").IsPressed(key)) ChangeSpeed(-10);

                    // HP Drain/Restorage
                    else if(Bindings.GetBinding("hpRestorageUp").IsPressed(key))
                        Map.currentLevel.metadata.hpRestorage++;
                    else if(Bindings.GetBinding("hpRestorageDown").IsPressed(key))
                        Map.currentLevel.metadata.hpRestorage--;
                    else if(Bindings.GetBinding("hpDrainUp").IsPressed(key))
                        Map.currentLevel.metadata.hpDrain++;
                    else if(Bindings.GetBinding("hpDrainDown").IsPressed(key))
                        Map.currentLevel.metadata.hpDrain--;

                    // Initial offset
                    else if(Bindings.GetBinding("initialOffsetUpBoost").IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs += 10;
                    else if(Bindings.GetBinding("initialOffsetDownBoost").IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs -= 10;
                    else if(Bindings.GetBinding("initialOffsetUp").IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs++;
                    else if(Bindings.GetBinding("initialOffsetDown").IsPressed(key))
                        Map.currentLevel.metadata.initialOffsetMs--;

                    // Fast scroll
                    else if(Bindings.GetBinding("fastScrollUp").IsPressed(key)) ScrollTime(10);
                    else if(Bindings.GetBinding("fastScrollDown").IsPressed(key)) ScrollTime(-10);
                }
                else {
                    if(Map.currentLevel.objects.FindAll(obj => obj.character == character && obj.step == (int)steps).Count <= 0) {
                        Map.currentLevel.objects.Add(new LevelObject(character, (int)steps, Map.currentLevel.speeds));
                        if(key.Shift) {
                            character = LevelObject.HOLD_CHAR;
                            Map.currentLevel.objects.Add(new LevelObject(character, (int)steps, Map.currentLevel.speeds, Map.currentLevel.objects));
                        }
                    }

                    changed = true;
                }

                if(changed) {
                    List<int> objSteps = Map.currentLevel.objects.Select(obj => obj.step).ToList();
                    Map.currentLevel.metadata.maxStep = objSteps.Count > 0 ? objSteps.Max() : 0;
                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(
                        StepsToMilliseconds(Map.currentLevel.metadata.maxStep) -
                        Map.currentLevel.metadata.initialOffsetMs);
                    Map.currentLevel.metadata.length =
                        $"{(timeSpan < TimeSpan.Zero ? "-" : "")}{timeSpan.ToString($"{(timeSpan.Hours != 0 ? "h':'mm" : "m")}':'ss")}";
                    Map.currentLevel.metadata.difficulty = LevelMetadata.GetDifficulty(Map.currentLevel.objects,
                            Map.currentLevel.speeds, (int)timeSpan.TotalMinutes)
                        .ToString("0.00", CultureInfo.InvariantCulture);
                }

                RecalculatePosition();
            }
            else if(!auto) {
                bool anythingPressed = false;
                for(int step = roundedSteps - LevelObject.missRange; StepPassedLine(step, -LevelObject.missRange);
                    step++)
                    if(CheckLine(step)) {
                        anythingPressed = true;
                        break;
                    }

                if(!anythingPressed && character != '\0') Map.flashLine = LevelObject.GetXPosForCharacter(character);
            }
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
                    Vector2i mousePos = Core.renderer.mousePosition;
                    if(mousePos.Y >= 12 && mousePos.Y <= 49) {
                        if(mousePos.X >= 28 && mousePos.X <= 51) {
                            if(scroll.Delta > 0 && UI.levelSelectLevels.First().position.Y >= 12) return;
                            if(scroll.Delta < 0 && UI.levelSelectLevels.Last().position.Y <= 49) return;
                            foreach(Button button in UI.levelSelectLevels) button.position += new Vector2i(0, (int)scroll.Delta);
                        }
                        else if(mousePos.X >= 1 && mousePos.X <= 26 &&
                                UI.levelSelectScores[UI.currentLevelSelectIndex] != null &&
                                UI.levelSelectScores[UI.currentLevelSelectIndex].Count > 0) {
                            if(scroll.Delta > 0 && UI.levelSelectScores[UI.currentLevelSelectIndex].First().scorePosition.Y >= 12) return;
                            if(scroll.Delta < 0 && UI.levelSelectScores[UI.currentLevelSelectIndex].Last().scoresPosition.Y <= 49) return;
                            for(int i = 0; i < UI.levelSelectScores[UI.currentLevelSelectIndex].Count; i++) {
                                int increment = (int)scroll.Delta;
                                LevelScore score = UI.levelSelectScores[UI.currentLevelSelectIndex][i];
                                score.scorePosition += new Vector2i(0, increment);
                                score.accComboPosition += new Vector2i(0, increment);
                                score.accComboDividerPosition += new Vector2i(0, increment);
                                score.maxComboPosition += new Vector2i(0, increment);
                                score.scoresPosition += new Vector2i(0, increment);
                                score.linePosition += new Vector2i(0, increment);
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
        public static float StepsToMilliseconds(float steps) => StepsToMilliseconds(steps, Map.currentLevel.speeds);
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
        public static float MillisecondsToSteps(float time) => MillisecondsToSteps(time, Map.currentLevel.speeds);
        // ReSharper disable once MemberCanBePrivate.Global
        public static float MillisecondsToSteps(float time, List<LevelSpeed> sortedSpeeds) {
            float useTime = time;

            int speedIndex = 0;
            for(int i = 0; i < sortedSpeeds.Count; i++)
                if(StepsToMilliseconds(sortedSpeeds[i].step, sortedSpeeds) <= useTime) speedIndex = i;
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
        public static float StepsToOffset(float steps) => StepsToOffset(steps, Map.currentLevel.speeds);
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
        public static int StepsToDirectionLayer(float steps) => StepsToDirectionLayer(steps, Map.currentLevel.speeds);
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
        public static bool StepPassedLine(int step, int lineOffset = 0) => roundedSteps >= step + lineOffset;

        public static int GetBPMAtStep(int step, IEnumerable<LevelSpeed> sortedSpeeds) {
            int bpm = 0;
            foreach(LevelSpeed speed in sortedSpeeds)
                if(speed.step <= step) bpm = speed.speed;
                else break;
            return bpm;
        }
        public static IEnumerable<int> GetBPMsBetweenSteps(int start, int end, IEnumerable<LevelSpeed> sortedSpeeds) =>
            from speed in sortedSpeeds where speed.step > start && speed.step < end select speed.speed;
        public static List<LevelSpeed> GetSpeedsBetweenSteps(int start, int end, List<LevelSpeed> sortedSpeeds) =>
            sortedSpeeds.FindAll(speed => speed.step >= start && speed.step <= end);

        static int GetInitialOffset(IReadOnlyList<string> lines) {
            string[] meta = lines[4].Split(':');
            return meta.Length > 5 ? int.Parse(meta[5]) : 0;
        }
        static List<LevelSpeed> GetSpeeds(IReadOnlyList<string> lines) {
            int[] speeds = lines[2].Length > 0 ? lines[2].Split(':').Select(int.Parse).ToArray() : new int[0];
            int[] speedsStarts = lines[3].Length > 0 ? lines[3].Split(':').Select(int.Parse).ToArray() : new int[0];
            List<LevelSpeed> levelSpeeds = speedsStarts.Select((step, i) => new LevelSpeed(speeds[i], step)).ToList();
            levelSpeeds.Sort((speed1, speed2) => speed1.step.CompareTo(speed2.step));
            return levelSpeeds;
        }

        static void GenerateLevelList() {
            string[] directories = Directory.GetDirectories("levels")
                .Where(path => Path.GetFileName(path) != "_template").ToArray();
            List<Button> buttons = new List<Button>();
            List<LevelMetadata?> metadatas = new List<LevelMetadata?>();
            for(int i = 0; i < directories.Length; i++) {
                string name = Path.GetFileName(directories[i]);
                buttons.Add(new Button(new Vector2i(25, 12 + i), name, "levelSelect.level", 30));
                metadatas.Add(new LevelMetadata(File.ReadAllLines(Path.Join(directories[i], "level.txt")), name));
                logger.Info("Loaded metadata for level {0}", name);
            }
            UI.levelSelectLevels = buttons;
            UI.levelSelectMetadatas = metadatas;

            List<List<LevelScore>> scores = new List<List<LevelScore>> { Capacity = buttons.Count };
            if(Directory.Exists("scores"))
                for(int i = 0; i < directories.Length; i++) {
                    string name = Path.GetFileName(directories[i]);
                    string scoresPath = Path.Join("scores", $"{name}.txt");
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
        public static Color GetAccuracyColor(int accuracy) => accuracy >= 100 ? ColorScheme.GetColor("accuracy_good") :
            accuracy >= 70 ? ColorScheme.GetColor("accuracy_ok") : ColorScheme.GetColor("accuracy_bad");
        public static Color GetComboColor(int accuracy, int misses) => accuracy >= 100 ? ColorScheme.GetColor("perfect_combo") :
            misses <= 0 ? ColorScheme.GetColor("full_combo") : ColorScheme.GetColor("combo");
    }
}
